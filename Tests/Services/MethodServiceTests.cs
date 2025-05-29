using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Models;
using Repositories;
using MongoDB.Driver;

namespace Tests.Unit.Services
{
    public class MethodServiceTests
    {
        private readonly MethodService _serviceWithAdminUser;
        private readonly MethodService _serviceWithRegularUser;
        private readonly Mock<IMethodRepository> _methodRepositoryMock;
        private readonly Mock<IRecipeRepository> _recipeRepositoryMock;
        private readonly Mock<IMongoClient> _mongoClientMock;
        private readonly Mock<IClientSessionHandle> _sessionMock;
        private readonly Mock<ILogger<MethodService>> _loggerMock;
        private readonly ClaimsPrincipal _adminUser;
        private readonly ClaimsPrincipal _regularUser;

        public MethodServiceTests()
        {
            _methodRepositoryMock = new Mock<IMethodRepository>();
            _recipeRepositoryMock = new Mock<IRecipeRepository>();
            _loggerMock = new Mock<ILogger<MethodService>>();
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

            _serviceWithAdminUser = new MethodService(
                _methodRepositoryMock.Object,
                _recipeRepositoryMock.Object,
                _mongoClientMock.Object,
                _loggerMock.Object,
                _adminUser);

            _serviceWithRegularUser = new MethodService(
                _methodRepositoryMock.Object,
                _recipeRepositoryMock.Object,
                _mongoClientMock.Object,
                _loggerMock.Object,
                _regularUser);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllMethods()
        {
            var methods = new List<Method>
            {
                new() { Id = "1", Name = "Baking", PictureUrl = "baking.jpg" },
                new() { Id = "2", Name = "Grilling", PictureUrl = "grilling.jpg" }
            };

            _methodRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(methods);

            var result = await _serviceWithRegularUser.GetAllAsync();

            Assert.Equal(methods, result);
            _methodRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMultipleByIdsAsync_ShouldReturnMatchingMethods()
        {
            var ids = new List<string> { "1", "2" };
            var methods = new List<Method>
            {
                new() { Id = "1", Name = "Baking", PictureUrl = "baking.jpg" },
                new() { Id = "2", Name = "Grilling", PictureUrl = "grilling.jpg" }
            };

            _methodRepositoryMock.Setup(repo => repo.GetMultipleByIdsAsync(ids)).ReturnsAsync(methods);

            var result = await _serviceWithRegularUser.GetMultipleByIdsAsync(ids);

            Assert.Equal(2, result.Count);
            Assert.Equal(methods, result);
            _methodRepositoryMock.Verify(repo => repo.GetMultipleByIdsAsync(ids), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnMethod_WhenExists()
        {
            var method = new Method { Id = "1", Name = "Frying", PictureUrl = "frying.jpg" };
            _methodRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync(method);

            var result = await _serviceWithRegularUser.GetOneByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal(method, result);
            _methodRepositoryMock.Verify(repo => repo.GetOneByIdAsync("1"), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            _methodRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync((Method?)null);

            var result = await _serviceWithRegularUser.GetOneByIdAsync("1");

            Assert.Null(result);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertMethod_WhenUserIsAdmin()
        {
            var method = new Method { Id = "1", Name = "Frying", PictureUrl = "Frying.jpg" };
            _methodRepositoryMock.Setup(repo => repo.InsertAsync(method)).ReturnsAsync("1");

            var result = await _serviceWithAdminUser.InsertAsync(method);

            Assert.Equal("1", result);
            _methodRepositoryMock.Verify(repo => repo.InsertAsync(method), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var method = new Method { Id = "1", Name = "Frying", PictureUrl = "Frying.jpg" };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.InsertAsync(method));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateMethodAndRecipes_WhenUserIsAdmin()
        {
            var method = new Method { Id = "1", Name = "Frying", PictureUrl = "Frying.jpg" };
            _methodRepositoryMock
                .Setup(repo => repo.UpdateAsync(method, _sessionMock.Object))
                .ReturnsAsync(true);

            await _serviceWithAdminUser.UpdateAsync(method);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _methodRepositoryMock.Verify(repo => repo.UpdateAsync(method, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateMethodInRecipes(method.Id, method.Name, method.PictureUrl, _sessionMock.Object), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var method = new Method { Id = "1", Name = "Frying", PictureUrl = "Frying.jpg" };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.UpdateAsync(method));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);

            _mongoClientMock.Verify(m => m.StartSessionAsync(default, default), Times.Never);
            _sessionMock.Verify(s => s.StartTransaction(default), Times.Never);
            _methodRepositoryMock.Verify(repo => repo.UpdateAsync(method, _sessionMock.Object), Times.Never);
            _recipeRepositoryMock.Verify(repo => repo.UpdateMethodInRecipes(method.Id, method.Name, method.PictureUrl, _sessionMock.Object), Times.Never);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldAbortTransaction_WhenExceptionInUpdateMethodOccurs()
        {
            var method = new Method { Id = "1", Name = "New Name", PictureUrl = "new.jpg" };

            _methodRepositoryMock
                .Setup(repo => repo.UpdateAsync(method, _sessionMock.Object))
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _serviceWithAdminUser.UpdateAsync(method));
            Assert.Equal("Database error", exception.Message);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _methodRepositoryMock.Verify(repo => repo.UpdateAsync(method, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateMethodInRecipes(method.Id, method.Name, method.PictureUrl, _sessionMock.Object), Times.Never);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldAbortTransaction_WhenExceptionInUpdateRecipesOccurs()
        {
            var method = new Method { Id = "1", Name = "New Name", PictureUrl = "new.jpg" };
            _methodRepositoryMock
                            .Setup(repo => repo.UpdateAsync(method, _sessionMock.Object))
                            .ReturnsAsync(true);
            _recipeRepositoryMock
                .Setup(repo => repo.UpdateMethodInRecipes(method.Id, method.Name, method.PictureUrl, _sessionMock.Object))
                .ThrowsAsync(new Exception("Recipe update error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _serviceWithAdminUser.UpdateAsync(method));
            Assert.Equal("Recipe update error", exception.Message);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _methodRepositoryMock.Verify(repo => repo.UpdateAsync(method, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateMethodInRecipes(method.Id, method.Name, method.PictureUrl, _sessionMock.Object), Times.Once);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteMethod_WhenUserIsAdmin()
        {
            await _serviceWithAdminUser.DeleteAsync("1");

            _methodRepositoryMock.Verify(repo => repo.DeleteAsync("1", default), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.DeleteAsync("1"));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);
        }
    }
}
