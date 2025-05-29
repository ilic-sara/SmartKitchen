using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Moq;
using Services;
using Models;
using Repositories;

namespace Tests.Unit.Services
{
    public class RecipeServiceTests
    {
        private readonly RecipeService _serviceWithAdminUser;
        private readonly RecipeService _serviceWithRegularUser;
        private readonly Mock<IRecipeRepository> _recipeRepositoryMock;
        private readonly Mock<ILogger<RecipeService>> _loggerMock;
        private readonly ClaimsPrincipal _adminUser;
        private readonly ClaimsPrincipal _regularUser;

        public RecipeServiceTests()
        {
            _recipeRepositoryMock = new Mock<IRecipeRepository>();
            _loggerMock = new Mock<ILogger<RecipeService>>();
            _adminUser = new ClaimsPrincipal(new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.Role, "Admin")
                ], "mock"));

            _regularUser = new ClaimsPrincipal(new ClaimsIdentity());


            _serviceWithAdminUser = new RecipeService(
                _recipeRepositoryMock.Object,
                _loggerMock.Object,
                _adminUser);

            _serviceWithRegularUser = new RecipeService(
                _recipeRepositoryMock.Object,
                _loggerMock.Object,
                _regularUser);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRecipes()
        {
            var recipes = new List<Recipe>
            {
                new() { Id = "1", Name = "Pizza" },
                new() { Id = "2", Name = "Pasta" }
            };

            _recipeRepositoryMock.Setup(repo => repo.GetAllAsync()).ReturnsAsync(recipes);

            var result = await _serviceWithAdminUser.GetAllAsync();

            Assert.Equal(2, result.Count);

            _recipeRepositoryMock.Verify(repo => repo.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMultipleByIdsAsync_ShouldReturnMatchingRecipes()
        {
            var ids = new List<string> { "1", "2" };
            var recipes = new List<Recipe>
            {
                new() { Id = "1", Name = "Pizza" },
                new() { Id = "2", Name = "Pasta" }
            };

            _recipeRepositoryMock.Setup(repo => repo.GetMultipleByIdsAsync(ids)).ReturnsAsync(recipes);

            var result = await _serviceWithAdminUser.GetMultipleByIdsAsync(ids);

            Assert.Equal(2, result.Count);  
            Assert.Equal(recipes, result);

            _recipeRepositoryMock.Verify(repo => repo.GetMultipleByIdsAsync(ids), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnRecipe_WhenExists()
        {
            var recipe = new Recipe { Id = "1", Name = "Pizza" };
            _recipeRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync(recipe);

            var result = await _serviceWithAdminUser.GetOneByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal(recipe, result);

            _recipeRepositoryMock.Verify(repo => repo.GetOneByIdAsync("1"), Times.Once);
        }

        [Fact]
        public async Task GetOneByIdAsync_ShouldReturnNull_WhenRecipeNotFound()
        {
            _recipeRepositoryMock.Setup(repo => repo.GetOneByIdAsync("1")).ReturnsAsync((Recipe?)null);

            var result = await _serviceWithRegularUser.GetOneByIdAsync("1");

            Assert.Null(result);
        }

        [Fact]
        public async Task InsertAsync_ShouldInsertRecipe_WhenUserIsAdmin()
        {
            var recipe = new Recipe { Id = "1", Name = "Pizza" };
            _recipeRepositoryMock.Setup(repo => repo.InsertAsync(recipe)).ReturnsAsync("1");

            var result = await _serviceWithAdminUser.InsertAsync(recipe);

            Assert.Equal("1", result);

            _recipeRepositoryMock.Verify(repo => repo.InsertAsync(recipe), Times.Once);
        }

        [Fact]
        public async Task InsertAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var recipe = new Recipe { Id = "1", Name = "Pizza" };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.InsertAsync(recipe));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);

            _recipeRepositoryMock.Verify(repo => repo.InsertAsync(recipe), Times.Never);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateRecipe_WhenUserIsAdmin()
        {
            var recipe = new Recipe { Id = "1", Name = "Pizza" };
            _recipeRepositoryMock.Setup(repo => repo.UpdateAsync(recipe, default)).ReturnsAsync(true);

            await _serviceWithAdminUser.UpdateAsync(recipe);

            _recipeRepositoryMock.Verify(repo => repo.UpdateAsync(recipe, default), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowException_WhenUserIsNotAdmin()
        {
            var recipe = new Recipe { Id = "1", Name = "Pizza" };

            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.UpdateAsync(recipe));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);

            _recipeRepositoryMock.Verify(repo => repo.UpdateAsync(recipe, default), Times.Never);
        }

        [Fact]
        public async Task DeleteAsync_ShouldDeleteRecipe_WhenUserIsAdmin()
        {
            await _serviceWithAdminUser.DeleteAsync("1");

            _recipeRepositoryMock.Verify(repo => repo.DeleteAsync("1", null), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ShouldThrowUnauthorizedAccessException_WhenUserIsNotAdmin()
        {
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _serviceWithRegularUser.DeleteAsync("1"));
            Assert.Equal("You are not authorized to perform this action.", exception.Message);
        }


        [Fact]
        public async Task GetRecipesByCategory_ShouldReturnMatchingRecipes()
        {
            var categoryId = "123";
            var expectedRecipes = new List<Recipe> 
            { 
                new() { Id = "1", Name = "Pizza" }, 
                new() { Id = "2", Name = "Bread"}, 
                new() { Id = "3", Name = "Lemonade"},
                new() { Id = "4", Name = "Cake"},
                new() { Id = "5", Name = "Ice cream"}
            };
            var numberOfRecipes = 5;
            _recipeRepositoryMock.Setup(repo => repo.GetRecipesByCategory(categoryId, 0, numberOfRecipes)).ReturnsAsync(expectedRecipes);

            var result = await _serviceWithRegularUser.GetRecipesByCategory(categoryId, 0, numberOfRecipes);

            Assert.Equal(expectedRecipes, result);

            _recipeRepositoryMock.Verify(repo => repo.GetRecipesByCategory(categoryId, 0 , numberOfRecipes), Times.Once);
        }

        [Fact]
        public async Task GetRecipesByCuisine_ShouldReturnMatchingRecipes()
        {
            var cuisineId = "123";
            var expectedRecipes = new List<Recipe>
            {
                new() { Id = "1", Name = "Pizza" },
                new() { Id = "2", Name = "Bread"},
                new() { Id = "3", Name = "Lemonade"},
                new() { Id = "4", Name = "Cake"},
                new() { Id = "5", Name = "Ice cream"}
            };
            var numberOfRecipes = 5;
            _recipeRepositoryMock.Setup(repo => repo.GetRecipesByCuisine(cuisineId, 0, numberOfRecipes)).ReturnsAsync(expectedRecipes);

            var result = await _serviceWithRegularUser.GetRecipesByCuisine(cuisineId, 0, numberOfRecipes);

            Assert.Equal(expectedRecipes, result);

            _recipeRepositoryMock.Verify(repo => repo.GetRecipesByCuisine(cuisineId, 0, numberOfRecipes), Times.Once);
        }

        [Fact]
        public async Task GetRecipesByDiet_ShouldReturnMatchingRecipes()
        {
            var dietId = "123";
            var expectedRecipes = new List<Recipe>
            {
                new() { Id = "1", Name = "Pizza" },
                new() { Id = "2", Name = "Bread"},
                new() { Id = "3", Name = "Lemonade"},
                new() { Id = "4", Name = "Cake"},
                new() { Id = "5", Name = "Ice cream"}
            };
            var numberOfRecipes = 5;
            _recipeRepositoryMock.Setup(repo => repo.GetRecipesByDiet(dietId, 0, numberOfRecipes)).ReturnsAsync(expectedRecipes);

            var result = await _serviceWithRegularUser.GetRecipesByDiet(dietId, 0, numberOfRecipes);

            Assert.Equal(expectedRecipes, result);

            _recipeRepositoryMock.Verify(repo => repo.GetRecipesByDiet(dietId, 0, numberOfRecipes), Times.Once);
        }

        [Fact]
        public async Task GetRecipesByMethod_ShouldReturnMatchingRecipes()
        {
            var methodId = "123";
            var expectedRecipes = new List<Recipe>
            {
                new() { Id = "1", Name = "Pizza" },
                new() { Id = "2", Name = "Bread"},
                new() { Id = "3", Name = "Lemonade"},
                new() { Id = "4", Name = "Cake"},
                new() { Id = "5", Name = "Ice cream"}
            };
            var numberOfRecipes = 5;
            _recipeRepositoryMock.Setup(repo => repo.GetRecipesByMethod(methodId, 0, numberOfRecipes)).ReturnsAsync(expectedRecipes);

            var result = await _serviceWithRegularUser.GetRecipesByMethod(methodId, 0, numberOfRecipes);

            Assert.Equal(expectedRecipes, result);

            _recipeRepositoryMock.Verify(repo => repo.GetRecipesByMethod(methodId, 0, numberOfRecipes), Times.Once);
        }

        [Fact]
        public async Task GetRecipesByName_ShouldReturnMatchingRecipes()
        {
            var name = "chicken";
            var expectedRecipes = new List<Recipe>
            {
                new() { Id = "1", Name = "Chicken salad" },
                new() { Id = "2", Name = "Chicken pizza"},
                new() { Id = "3", Name = "Chicken pasta"},
                new() { Id = "4", Name = "Chicken and rice"},
                new() { Id = "5", Name = "Baked chicken"}
            };
            var numberOfRecipes = 5;
            _recipeRepositoryMock.Setup(repo => repo.GetRecipesByName(name, 0, numberOfRecipes)).ReturnsAsync(expectedRecipes);

            var result = await _serviceWithRegularUser.GetRecipesByName(name, 0, numberOfRecipes);

            Assert.Equal(expectedRecipes, result);

            _recipeRepositoryMock.Verify(repo => repo.GetRecipesByName(name, 0, numberOfRecipes), Times.Once);
        }

        [Fact]
        public async Task GetRecipesByIngredients_ShouldReturnMatchingRecipes()
        {
            var ingredients = new List<string> { "salt", "pepper", "chicken" };
            var expectedRecipes = new List<Recipe>
            {
                new() { Id = "1", Name = "Chicken salad" },
                new() { Id = "2", Name = "Chicken pizza"},
                new() { Id = "3", Name = "Chicken pasta"},
                new() { Id = "4", Name = "Chicken and rice"},
                new() { Id = "5", Name = "Baked chicken"}
            };
            var numberOfRecipes = 5;
            _recipeRepositoryMock.Setup(repo => repo.GetRecipesByIngredients(ingredients, 0, numberOfRecipes)).ReturnsAsync(expectedRecipes);

            var result = await _serviceWithRegularUser.GetRecipesByIngredients(ingredients, 0, numberOfRecipes);

            Assert.Equal(expectedRecipes, result);

            _recipeRepositoryMock.Verify(repo => repo.GetRecipesByIngredients(ingredients, 0, numberOfRecipes), Times.Once);
        }

        [Fact]
        public async Task GetNewestRecipes_ShouldReturnMatchingRecipes()
        {
            var expectedRecipes = new List<Recipe>
            {
                new() { Id = "1", Name = "Chicken salad" },
                new() { Id = "2", Name = "Chicken pizza"},
                new() { Id = "3", Name = "Chicken pasta"},
                new() { Id = "4", Name = "Chicken and rice"},
                new() { Id = "5", Name = "Baked chicken"}
            };
            var numberOfRecipes = 5;
            _recipeRepositoryMock.Setup(repo => repo.GetNewestRecipes(0, numberOfRecipes)).ReturnsAsync(expectedRecipes);

            var result = await _serviceWithRegularUser.GetNewestRecipes(0, numberOfRecipes);

            Assert.Equal(expectedRecipes, result);

            _recipeRepositoryMock.Verify(repo => repo.GetNewestRecipes(0, numberOfRecipes), Times.Once);
        }


        [Fact]
        public async Task GetNumberOfRecipesByCategory_ShouldReturnCount()
        {
            var categoryId = "123";
            _recipeRepositoryMock.Setup(repo => repo.GetNumberOfRecipesByCategory(categoryId)).ReturnsAsync(5);

            var result = await _serviceWithRegularUser.GetNumberOfRecipesByCategory(categoryId);

            Assert.Equal(5, result);

            _recipeRepositoryMock.Verify(repo => repo.GetNumberOfRecipesByCategory(categoryId), Times.Once);
        }

        [Fact]
        public async Task GetNumberOfRecipesByCuisine_ShouldReturnCount()
        {
            var cuisineId = "123";
            _recipeRepositoryMock.Setup(repo => repo.GetNumberOfRecipesByCuisine(cuisineId)).ReturnsAsync(5);

            var result = await _serviceWithRegularUser.GetNumberOfRecipesByCuisine(cuisineId);

            Assert.Equal(5, result);

            _recipeRepositoryMock.Verify(repo => repo.GetNumberOfRecipesByCuisine(cuisineId), Times.Once);
        }

        [Fact]
        public async Task GetNumberOfRecipesByDiet_ShouldReturnCount()
        {
            var dietId = "123";
            _recipeRepositoryMock.Setup(repo => repo.GetNumberOfRecipesByDiet(dietId)).ReturnsAsync(5);

            var result = await _serviceWithRegularUser.GetNumberOfRecipesByDiet(dietId);

            Assert.Equal(5, result);

            _recipeRepositoryMock.Verify(repo => repo.GetNumberOfRecipesByDiet(dietId), Times.Once);
        }

        [Fact]
        public async Task GetNumberOfRecipesByMethod_ShouldReturnCount()
        {
            var methodId = "123";
            _recipeRepositoryMock.Setup(repo => repo.GetNumberOfRecipesByMethod(methodId)).ReturnsAsync(5);

            var result = await _serviceWithRegularUser.GetNumberOfRecipesByMethod(methodId);

            Assert.Equal(5, result);

            _recipeRepositoryMock.Verify(repo => repo.GetNumberOfRecipesByMethod(methodId), Times.Once);
        }

        [Fact]
        public async Task GetNumberOfRecipesByName_ShouldReturnCount()
        {
            var name = "chicken";
            _recipeRepositoryMock.Setup(repo => repo.GetNumberOfRecipesByName(name)).ReturnsAsync(5);

            var result = await _serviceWithRegularUser.GetNumberOfRecipesByName(name);

            Assert.Equal(5, result);

            _recipeRepositoryMock.Verify(repo => repo.GetNumberOfRecipesByName(name), Times.Once);
        }

        [Fact]
        public async Task GetNumberOfRecipesByIngredients_ShouldReturnCount()
        {
            var ingredients = new List<string> { "salt", "pepper", "chicken" };
            _recipeRepositoryMock.Setup(repo => repo.GetNumberOfRecipesByIngredients(ingredients)).ReturnsAsync(5);

            var result = await _serviceWithRegularUser.GetNumberOfRecipesByIngredients(ingredients);

            Assert.Equal(5, result);

            _recipeRepositoryMock.Verify(repo => repo.GetNumberOfRecipesByIngredients(ingredients), Times.Once);
        }
    }
}
