using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Models;
using Repositories;
using MongoDB.Driver;

namespace Tests.Unit.Services
{
    public class DietServiceTests
    {
        private readonly DietService _serviceWithAdminUser;
        private readonly DietService _serviceWithRegularUser;
        private readonly Mock<IDietRepository> _dietRepositoryMock;
        private readonly Mock<IRecipeRepository> _recipeRepositoryMock;
        private readonly Mock<IMongoClient> _mongoClientMock;
        private readonly Mock<IClientSessionHandle> _sessionMock;
        private readonly Mock<ILogger<DietService>> _loggerMock;
        private readonly ClaimsPrincipal _adminUser;
        private readonly ClaimsPrincipal _regularUser;

        public DietServiceTests()
        {
            _dietRepositoryMock = new Mock<IDietRepository>();
            _recipeRepositoryMock = new Mock<IRecipeRepository>();
            _loggerMock = new Mock<ILogger<DietService>>();
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

            _serviceWithAdminUser = new DietService(
                _dietRepositoryMock.Object,
                _recipeRepositoryMock.Object,
                _mongoClientMock.Object,
                _loggerMock.Object,
                _adminUser);

            _serviceWithRegularUser = new DietService(
                _dietRepositoryMock.Object,
                _recipeRepositoryMock.Object,
                _mongoClientMock.Object,
                _loggerMock.Object,
                _regularUser);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllDiets()
        {
            var diets = new List<Diet>
            {
                new() { Id = "1", Name = "Keto", PictureUrl = "keto.jpg" },
                new() { Id = "2", Name = "Vegan", PictureUrl = "vegan.jpg" }
            };

            _dietRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(diets);

            var result = await _serviceWithRegularUser.GetAllAsync();

            Assert.Equal(diets, result);
            _dietRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMultipleByIdsAsync_ShouldReturnMatchingDiets()
        {
            var ids = new List<string> { "1", "2" };
            var diets = new List<Diet>
            {
                new() { Id = "1", Name = "Keto", PictureUrl = "keto.jpg" },
                new() { Id = "2", Name = "Vegan", PictureUrl = "vegan.jpg" }
            };

            _dietRepositoryMock.Setup(repo => repo.GetMultipleByIdsAsync(ids)).ReturnsAsync(diets);

            var result = await _serviceWithRegularUser.GetMultipleByIdsAsync(ids);

            Assert.Equal(2, result.Count);
            Assert.Equal(diets, result);
            _dietRepositoryMock.Verify(repo => repo.GetMultipleByIdsAsync(ids), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnDiet_WhenExists()
        {
            var diet = new Diet { Id = "1", Name = "Vegan", PictureUrl = "vegan.jpg" };
            _dietRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync(diet);

            var result = await _serviceWithRegularUser.GetOneByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal(diet, result);
            _dietRepositoryMock.Verify(repo => repo.GetOneByIdAsync("1"), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            _dietRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync((Diet?)null);

            var result = await _serviceWithRegularUser.GetOneByIdAsync("1");

            Assert.Null(result);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertDiet_WhenUserIsAdmin()
        {
            var diet = new Diet { Id = "1", Name = "Vegan", PictureUrl = "vegan.jpg" };
            _dietRepositoryMock.Setup(repo => repo.InsertAsync(diet)).ReturnsAsync("1");

            var result = await _serviceWithAdminUser.InsertAsync(diet);

            Assert.Equal("1", result);
            _dietRepositoryMock.Verify(repo => repo.InsertAsync(diet), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var diet = new Diet { Id = "1", Name = "Vegan", PictureUrl = "vegan.jpg" };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.InsertAsync(diet));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateDietAndRecipes_WhenUserIsAdmin()
        {
            var diet = new Diet { Id = "1", Name = "Vegan", PictureUrl = "vegan.jpg" };
            _dietRepositoryMock
                .Setup(repo => repo.UpdateAsync(diet, _sessionMock.Object))
                .ReturnsAsync(true);

            await _serviceWithAdminUser.UpdateAsync(diet);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _dietRepositoryMock.Verify(repo => repo.UpdateAsync(diet, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateDietInRecipes(diet.Id, diet.Name, diet.PictureUrl, _sessionMock.Object), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var diet = new Diet { Id = "1", Name = "Vegan", PictureUrl = "vegan.jpg" };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.UpdateAsync(diet));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);

            _mongoClientMock.Verify(m => m.StartSessionAsync(default, default), Times.Never);
            _sessionMock.Verify(s => s.StartTransaction(default), Times.Never);
            _dietRepositoryMock.Verify(repo => repo.UpdateAsync(diet, _sessionMock.Object), Times.Never);
            _recipeRepositoryMock.Verify(repo => repo.UpdateDietInRecipes(diet.Id, diet.Name, diet.PictureUrl, _sessionMock.Object), Times.Never);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldAbortTransaction_WhenExceptionInUpdateDietOccurs()
        {
            var diet = new Diet { Id = "1", Name = "New Name", PictureUrl = "new.jpg" };

            _dietRepositoryMock
                .Setup(repo => repo.UpdateAsync(diet, _sessionMock.Object))
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _serviceWithAdminUser.UpdateAsync(diet));
            Assert.Equal("Database error", exception.Message);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _dietRepositoryMock.Verify(repo => repo.UpdateAsync(diet, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateDietInRecipes(diet.Id, diet.Name, diet.PictureUrl, _sessionMock.Object), Times.Never);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldAbortTransaction_WhenExceptionInUpdateRecipesOccurs()
        {
            var diet = new Diet { Id = "1", Name = "New Name", PictureUrl = "new.jpg" };

            _dietRepositoryMock
                .Setup(repo => repo.UpdateAsync(diet, _sessionMock.Object))
                .ReturnsAsync(true); 

            _recipeRepositoryMock
                .Setup(repo => repo.UpdateDietInRecipes(diet.Id, diet.Name, diet.PictureUrl, _sessionMock.Object))
                .ThrowsAsync(new Exception("Recipe update error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _serviceWithAdminUser.UpdateAsync(diet));
            Assert.Equal("Recipe update error", exception.Message);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _dietRepositoryMock.Verify(repo => repo.UpdateAsync(diet, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateDietInRecipes(diet.Id, diet.Name, diet.PictureUrl, _sessionMock.Object), Times.Once);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteDiet_WhenUserIsAdmin()
        {
            await _serviceWithAdminUser.DeleteAsync("1");

            _dietRepositoryMock.Verify(repo => repo.DeleteAsync("1", default), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.DeleteAsync("1"));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);
        }
    }
}
