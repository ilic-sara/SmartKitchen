using Moq;
using Services;
using Models;
using Repositories;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using System.Security.Claims;

namespace Tests.Unit.Services
{
    public class CategoryServiceTests
    {
        private readonly CategoryService _serviceWithAdminUser;
        private readonly CategoryService _serviceWithRegularUser;
        private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
        private readonly Mock<IRecipeRepository> _recipeRepositoryMock;
        private readonly Mock<IMongoClient> _mongoClientMock;
        private readonly Mock<IClientSessionHandle> _sessionMock;
        private readonly Mock<ILogger<CategoryService>> _loggerMock;
        private readonly ClaimsPrincipal _adminUser;
        private readonly ClaimsPrincipal _regularUser;

        public CategoryServiceTests()
        {
            _categoryRepositoryMock = new Mock<ICategoryRepository>();
            _recipeRepositoryMock = new Mock<IRecipeRepository>();
            _loggerMock = new Mock<ILogger<CategoryService>>();
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

            _serviceWithAdminUser = new CategoryService(
                _categoryRepositoryMock.Object,
                _recipeRepositoryMock.Object,
                _loggerMock.Object,
                _mongoClientMock.Object,
                _adminUser
            );

            _serviceWithRegularUser = new CategoryService(
                _categoryRepositoryMock.Object,
                _recipeRepositoryMock.Object,
                _loggerMock.Object,
                _mongoClientMock.Object,
                _regularUser
            );
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnCategories()
        {
            var categories = new List<Category>
            {
                new() { Id = "1", Name = "Breakfast", PictureUrl = "breakfast.jpg" },
                new() { Id = "2", Name = "Lunch", PictureUrl = "lunch.jpg" }
            };

            _categoryRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(categories);

            var result = await _serviceWithRegularUser.GetAllAsync();

            Assert.Equal(2, result.Count);
            Assert.Equal("Breakfast", result[0].Name);
            Assert.Equal("Lunch", result[1].Name);
            Assert.Equal("1", result[0].Id);
            Assert.Equal("2", result[1].Id);

            _categoryRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMultipleByIdsAsync_ShouldReturnMatchingCategories()
        {
            var ids = new List<string> { "1", "2" };
            var categories = new List<Category>
            {
                new() { Id = "1", Name = "Breakfast", PictureUrl = "breakfast.jpg" },
                new() { Id = "2", Name = "Lunch", PictureUrl = "lunch.jpg" }
            };

            _categoryRepositoryMock.Setup(repo => repo.GetMultipleByIdsAsync(ids)).ReturnsAsync(categories);

            var result = await _serviceWithRegularUser.GetMultipleByIdsAsync(ids);

            Assert.Equal(2, result.Count);
            Assert.Equal(categories, result);

            _categoryRepositoryMock.Verify(repo => repo.GetMultipleByIdsAsync(ids), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnCategory_WhenExists()
        {
            var category = new Category { Id = "1", Name = "Breakfast", PictureUrl = "breakfast.jpg" };
            _categoryRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync(category);

            var result = await _serviceWithRegularUser.GetOneByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal("Breakfast", result.Name);
            Assert.Equal("breakfast.jpg", result.PictureUrl);

            _categoryRepositoryMock.Verify(repo => repo.GetOneByIdAsync("1"), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnNull_WhenNotFound()
        {
            _categoryRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync((Category?)null);

            var result = await _serviceWithRegularUser.GetOneByIdAsync("1");

            Assert.Null(result);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertCategory_WhenUserIsAdmin()
        {
            var category = new Category { Id = "3", Name = "Dessert", PictureUrl = "dessert.jpg" };
            _categoryRepositoryMock.Setup(repo => repo.InsertAsync(category)).ReturnsAsync("3");

            var result = await _serviceWithAdminUser.InsertAsync(category);

            Assert.Equal("3", result);

            _categoryRepositoryMock.Verify(repo => repo.InsertAsync(category), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var category = new Category { Id = "3", Name = "Dessert", PictureUrl = "dessert.jpg" };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.InsertAsync(category));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateCategoryAndRecipes_WhenUserIsAdmin()
        {
            var category = new Category { Id = "1", Name = "New Name", PictureUrl = "new.jpg" };
            _categoryRepositoryMock
                .Setup(repo => repo.UpdateAsync(category, _sessionMock.Object))
                .ReturnsAsync(true);

            await _serviceWithAdminUser.UpdateAsync(category);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _categoryRepositoryMock.Verify(repo => repo.UpdateAsync(category, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateCategoryInRecipes(category.Id, category.Name, category.PictureUrl, _sessionMock.Object), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var category = new Category { Id = "1", Name = "New Name", PictureUrl = "new.jpg" };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.UpdateAsync(category));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);

            _mongoClientMock.Verify(m => m.StartSessionAsync(default, default), Times.Never);
            _sessionMock.Verify(s => s.StartTransaction(default), Times.Never);
            _categoryRepositoryMock.Verify(repo => repo.UpdateAsync(category, _sessionMock.Object), Times.Never);
            _recipeRepositoryMock.Verify(repo => repo.UpdateCategoryInRecipes(category.Id, category.Name, category.PictureUrl, _sessionMock.Object), Times.Never);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldAbortTransaction_WhenExceptionInUpdateCategoryOccurs()
        {
            var category = new Category { Id = "1", Name = "New Name", PictureUrl = "new.jpg" };

            _categoryRepositoryMock
                .Setup(repo => repo.UpdateAsync(category, _sessionMock.Object))
                .ThrowsAsync(new Exception("Database error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _serviceWithAdminUser.UpdateAsync(category));
            Assert.Equal("Database error", exception.Message);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _categoryRepositoryMock.Verify(repo => repo.UpdateAsync(category, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateCategoryInRecipes(category.Id, category.Name, category.PictureUrl, _sessionMock.Object), Times.Never);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldAbortTransaction_WhenExceptionInUpdateRecipesOccurs()
        {
            var category = new Category { Id = "1", Name = "New Name", PictureUrl = "new.jpg" };
            _categoryRepositoryMock
                            .Setup(repo => repo.UpdateAsync(category, _sessionMock.Object))
                            .ReturnsAsync(true);
            _recipeRepositoryMock
                .Setup(repo => repo.UpdateCategoryInRecipes(category.Id, category.Name, category.PictureUrl, _sessionMock.Object))
                .ThrowsAsync(new Exception("Recipe update error"));

            var exception = await Assert.ThrowsAsync<Exception>(() => _serviceWithAdminUser.UpdateAsync(category));
            Assert.Equal("Recipe update error", exception.Message);

            _sessionMock.Verify(s => s.StartTransaction(default), Times.Once);
            _categoryRepositoryMock.Verify(repo => repo.UpdateAsync(category, _sessionMock.Object), Times.Once);
            _recipeRepositoryMock.Verify(repo => repo.UpdateCategoryInRecipes(category.Id, category.Name, category.PictureUrl, _sessionMock.Object), Times.Once);
            _sessionMock.Verify(s => s.AbortTransactionAsync(default), Times.Once);
            _sessionMock.Verify(s => s.CommitTransactionAsync(default), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteCategory_WhenUserIsAdmin()
        {
            await _serviceWithAdminUser.DeleteAsync("1");

            _categoryRepositoryMock.Verify(repo => repo.DeleteAsync("1", default), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.DeleteAsync("1"));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);
        }
    }
}
