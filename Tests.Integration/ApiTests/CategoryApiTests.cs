using System.Net;
using System.Net.Http.Json;
using Models;

namespace Tests.Integration.ApiTests
{
    public class CategoryApiTests : IClassFixture<TestFixture>
    {
        private readonly HttpClient _clientAdmin;
        private readonly HttpClient _clientUnauthenticated;
        private readonly List<Category> _categories;

        public CategoryApiTests(TestFixture fixture)
        {
            _clientAdmin = fixture.AdminClient;
            _clientUnauthenticated = fixture.UnauthenticatedClient;
            _categories = fixture.Categories;
        }


        #region Admin Tests
        
        [Fact]
        public async Task PostCategory_ShouldReturn201_AndPersistInDb_WhenAdminUser()
        {
            var category = new Category 
            { 
                Name = "ToCreate", 
                PictureUrl = "https://ToCreate.jpg"
            };

            var response = await _clientAdmin.PostAsJsonAsync("/api/category", category);
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var addedCategory = await response.Content.ReadFromJsonAsync<Category>();
            string id = addedCategory?.Id ?? throw new Exception("Category not saved.");

            //check if inserted
            var getResponse = await _clientAdmin.GetAsync($"/api/category/{id}");
            var fetchedCategory = await getResponse.Content.ReadFromJsonAsync<Category>();

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.NotNull(fetchedCategory);
            Assert.Equal("ToCreate", fetchedCategory.Name);
            Assert.Equal("https://ToCreate.jpg", fetchedCategory.PictureUrl);
        }

        [Fact]
        public async Task PutCategory_ShouldReturn204_AndUpdateCategory_WhenAdminUser()
        {
            var toUpdate = _categories[1];
            toUpdate.Name = "UpdatedCategory";
            toUpdate.PictureUrl = "https://UpdatedCategory.jpg";

            var response = await _clientAdmin.PutAsJsonAsync($"/api/category/{toUpdate.Id}", toUpdate);
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //check if updated
            var getResponse = await _clientAdmin.GetAsync($"/api/category/{toUpdate.Id}");
            var fetchedCategory = await getResponse.Content.ReadFromJsonAsync<Category>();

            Assert.NotNull(fetchedCategory);
            Assert.Equal("UpdatedCategory", fetchedCategory.Name);
            Assert.Equal("https://UpdatedCategory.jpg", fetchedCategory.PictureUrl);
        }

        [Fact]
        public async Task DeleteCategory_ShouldReturn204_AndRemoveFromDb_WhenAdminUser()
        {
            var id = _categories[2].Id;
            var response = await _clientAdmin.DeleteAsync($"/api/category/{id}");
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //check if removed
            var getResponse = await _clientAdmin.GetAsync($"/api/category/{id}");

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        #endregion

        #region Unauthenticated Tests
        [Fact]
        public async Task GetCategories_ShouldReturnListOfCategories()
        {
            var response = await _clientUnauthenticated.GetAsync("/api/category");
            var categoriesFromDb = await response.Content.ReadFromJsonAsync<List<Category>>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(categoriesFromDb);
            Assert.True(categoriesFromDb.Count > 0);
        }

        [Fact]
        public async Task GetCategoryById_ShouldReturnCategory_WhenExists()
        {
            string id = _categories[0].Id;
            var response = await _clientUnauthenticated.GetAsync($"/api/category/{id}");
            var category = await response.Content.ReadFromJsonAsync<Category>();

            Assert.NotNull(category);
            Assert.Equal(id, category.Id);
        }

        [Fact]
        public async Task GetCategoryById_ShouldReturn404_WhenCategoryNotFound()
        {
            var response = await _clientUnauthenticated.GetAsync($"/api/category/67c480be772887a95c508c10");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PostCategory_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var category = new Category 
            { 
                Name = "ToCreateUnauthorized", 
                PictureUrl = "https://ToCreateUnauthorized.jpg"
            };

            var response = await _clientUnauthenticated.PostAsJsonAsync("/api/category", category);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PutCategory_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var updatedCategory = _categories[1];
            updatedCategory.Name = "UpdatedUnauthorized";
            updatedCategory.PictureUrl = "https://UpdatedUnauthorized.jpg";

            var response = await _clientUnauthenticated.PutAsJsonAsync($"/api/category/{updatedCategory.Id}", updatedCategory);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCategory_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var response = await _clientUnauthenticated.DeleteAsync($"/api/category/{_categories[0].Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        #endregion
    }
}
