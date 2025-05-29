using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Models;
using Repositories;
using MongoDB.Driver;

namespace Tests.Unit.Services
{
    public class CuisineServiceTests
    {
        private readonly CuisineService _serviceWithAdminUser;
        private readonly CuisineService _serviceWithRegularUser;
        private readonly Mock<ICuisineRepository> _cuisineRepositoryMock;
        private readonly Mock<IRecipeRepository> _recipeRepositoryMock;
        private readonly Mock<IMongoClient> _mongoClientMock;
        private readonly Mock<IClientSessionHandle> _sessionMock;
        private readonly Mock<ILogger<CuisineService>> _loggerMock;
        private readonly ClaimsPrincipal _adminUser;
        private readonly ClaimsPrincipal _regularUser;

        public CuisineServiceTests()
        {
            _cuisineRepositoryMock = new Mock<ICuisineRepository>();
            _recipeRepositoryMock = new Mock<IRecipeRepository>();
            _loggerMock = new Mock<ILogger<CuisineService>>();
            _mongoClientMock = new Mock<IMongoClient>();
            _sessionMock = new Mock<IClientSessionHandle>();

            _mongoClientMock
                .Setup(m => m.StartSessionAsync(It.IsAny<ClientSessionOptions>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(_sessionMock.Object);

            _sessionMock
                .Setup(s => s.StartTransaction(It.IsAny<TransactionOptions>()));

            _adminUser = new ClaimsPrincipal(new ClaimsIdentity(
               [
                   new Claim(ClaimTypes.Role, "Admin")
               ], "mock"));

            _regularUser = new ClaimsPrincipal(new ClaimsIdentity());

            _serviceWithAdminUser = new CuisineService(
                _cuisineRepositoryMock.Object,
                _recipeRepositoryMock.Object,
                _mongoClientMock.Object,
                _loggerMock.Object,
                _adminUser);

            _serviceWithRegularUser = new CuisineService(
                _cuisineRepositoryMock.Object,
                _recipeRepositoryMock.Object,
                _mongoClientMock.Object,
                _loggerMock.Object,
                _regularUser);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllCuisines()
        {
            var cuisines = new List<Cuisine>
            {
                new() { Id = "1", Name = "Italian", PictureUrl = "italian.jpg" },
                new() { Id = "2", Name = "Mexican", PictureUrl = "mexican.jpg" }
            };

            _cuisineRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(cuisines);

            var result = await _serviceWithRegularUser.GetAllAsync();

            Assert.Equal(cuisines, result);
            _cuisineRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMultipleByIdsAsync_ShouldReturnMatchingCuisines()
        {
            var ids = new List<string> { "1", "2" };
            var cuisines = new List<Cuisine>
            {
                new() { Id = "1", Name = "Italian", PictureUrl = "italian.jpg" },
                new() { Id = "2", Name = "Mexican", PictureUrl = "mexican.jpg" }
            };

            _cuisineRepositoryMock.Setup(repo => repo.GetMultipleByIdsAsync(ids)).ReturnsAsync(cuisines);

            var result = await _serviceWithRegularUser.GetMultipleByIdsAsync(ids);

            Assert.Equal(2, result.Count);
            Assert.Equal(cuisines, result);
            _cuisineRepositoryMock.Verify(repo => repo.GetMultipleByIdsAsync(ids), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnCuisine_WhenExists()
        {
            var cuisine = new Cuisine { Id = "1", Name = "French", PictureUrl = "french.jpg" };
            _cuisineRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync(cuisine);

            var result = await _serviceWithRegularUser.GetOneByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal(cuisine, result);
            _cuisineRepositoryMock.Verify(repo => repo.GetOneByIdAsync("1"), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            _cuisineRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync((Cuisine?)null);

            var result = await _serviceWithRegularUser.GetOneByIdAsync("1");

            Assert.Null(result);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertCuisine_WhenUserIsAdmin()
        {
            var cuisine = new Cuisine { Id = "1", Name = "Japanese", PictureUrl="japanese.jpg" };
            _cuisineRepositoryMock.Setup(repo => repo.InsertAsync(cuisine)).ReturnsAsync("1");

            var result = await _serviceWithAdminUser.InsertAsync(cuisine);

            Assert.Equal("1", result);
            _cuisineRepositoryMock.Verify(repo => repo.InsertAsync(cuisine), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var cuisine = new Cuisine { Id = "1", Name = "Japanese", PictureUrl = "japanese.jpg" };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.InsertAsync(cuisine));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCuisineAndRecipes_WhenUserIsAdmin()
        {
            var cuisine = new Cuisine { Id = "1", Name = "Greek", PictureUrl = "greek.jpg" };
            _cuisineRepositoryMock
                .Setup(repo => repo.UpdateAsync(cuisine, _sessionMock.Object))
                .ReturnsAsync(true); 

            await _serviceWithAdminUser.UpdateAsync(cuisine);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _cuisineRepositoryMock.Verify(repo => repo.UpdateAsync(cuisine, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateCuisineInRecipes(cuisine.Id, cuisine.Name, cuisine.PictureUrl, _sessionMock.Object), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var cuisine = new Cuisine { Id = "1", Name = "Greek", PictureUrl = "greek.jpg" };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.UpdateAsync(cuisine));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);

            _mongoClientMock.Verify(m => m.StartSessionAsync(default, default), Times.Never);
            _sessionMock.Verify(s => s.StartTransaction(default), Times.Never);
            _cuisineRepositoryMock.Verify(repo => repo.UpdateAsync(cuisine, _sessionMock.Object), Times.Never);
            _recipeRepositoryMock.Verify(repo => repo.UpdateCuisineInRecipes(cuisine.Id, cuisine.Name, cuisine.PictureUrl, _sessionMock.Object), Times.Never);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldAbortTransaction_WhenExceptionInUpdateCuisineOccurs()
        {
            var cuisine = new Cuisine { Id = "1", Name = "New Name", PictureUrl = "new.jpg" };

            _cuisineRepositoryMock
                .Setup(repo => repo.UpdateAsync(cuisine, _sessionMock.Object))
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _serviceWithAdminUser.UpdateAsync(cuisine));
            Assert.Equal("Database error", exception.Message);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _cuisineRepositoryMock.Verify(repo => repo.UpdateAsync(cuisine, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateCuisineInRecipes(cuisine.Id, cuisine.Name, cuisine.PictureUrl, _sessionMock.Object), Times.Never);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldAbortTransaction_WhenExceptionInUpdateRecipesOccurs()
        {
            var cuisine = new Cuisine { Id = "1", Name = "New Name", PictureUrl = "new.jpg" };
            _cuisineRepositoryMock
                            .Setup(repo => repo.UpdateAsync(cuisine, _sessionMock.Object))
                            .ReturnsAsync(true);
            _recipeRepositoryMock
                .Setup(repo => repo.UpdateCuisineInRecipes(cuisine.Id, cuisine.Name, cuisine.PictureUrl, _sessionMock.Object))
                .ThrowsAsync(new Exception("Recipe update error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _serviceWithAdminUser.UpdateAsync(cuisine));
            Assert.Equal("Recipe update error", exception.Message);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _cuisineRepositoryMock.Verify(repo => repo.UpdateAsync(cuisine, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateCuisineInRecipes(cuisine.Id, cuisine.Name, cuisine.PictureUrl, _sessionMock.Object), Times.Once);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCuisine_WhenUserIsAdmin()
        {
            await _serviceWithAdminUser.DeleteAsync("1");

            _cuisineRepositoryMock.Verify(repo => repo.DeleteAsync("1", default), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.DeleteAsync("1"));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);
        }
    }
}
