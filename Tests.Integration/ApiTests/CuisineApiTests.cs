using System.Net;
using System.Net.Http.Json;
using Models;

namespace Tests.Integration.ApiTests
{
    public class CuisineApiTests : IClassFixture<TestFixture>
    {
        private readonly HttpClient _clientAdmin;
        private readonly HttpClient _clientUnauthenticated;
        private readonly List<Cuisine> _cuisines;

        public CuisineApiTests(TestFixture fixture)
        {
            _clientAdmin = fixture.AdminClient;
            _clientUnauthenticated = fixture.UnauthenticatedClient;
            _cuisines = fixture.Cuisines;
        }


        #region Admin Tests

        [Fact]
        public async Task PostCuisine_ShouldReturn201_AndPersistInDb_WhenAdminUser()
        {
            var cuisine = new Cuisine
            {
                Name = "ToCreate",
                PictureUrl = "https://ToCreate.jpg"
            };

            var response = await _clientAdmin.PostAsJsonAsync("/api/cuisine", cuisine);
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var addedCuisine = await response.Content.ReadFromJsonAsync<Cuisine>();
            string id = addedCuisine?.Id ?? throw new Exception("Cuisine not saved.");

            //check if inserted
            var getResponse = await _clientAdmin.GetAsync($"/api/cuisine/{id}");
            var fetchedCuisine = await getResponse.Content.ReadFromJsonAsync<Cuisine>();

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.NotNull(fetchedCuisine);
            Assert.Equal("ToCreate", fetchedCuisine.Name);
            Assert.Equal("https://ToCreate.jpg", fetchedCuisine.PictureUrl);
        }

        [Fact]
        public async Task PutCuisine_ShouldReturn204_AndUpdateCuisine_WhenAdminUser()
        {
            var toUpdate = _cuisines[1];
            toUpdate.Name = "UpdatedCuisine";
            toUpdate.PictureUrl = "https://UpdatedCuisine.jpg";

            var response = await _clientAdmin.PutAsJsonAsync($"/api/cuisine/{toUpdate.Id}", toUpdate);
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //check if updated
            var getResponse = await _clientAdmin.GetAsync($"/api/cuisine/{toUpdate.Id}");
            var fetchedCuisine = await getResponse.Content.ReadFromJsonAsync<Cuisine>();

            Assert.NotNull(fetchedCuisine);
            Assert.Equal("UpdatedCuisine", fetchedCuisine.Name);
            Assert.Equal("https://UpdatedCuisine.jpg", fetchedCuisine.PictureUrl);
        }

        [Fact]
        public async Task DeleteCuisine_ShouldReturn204_AndRemoveFromDb_WhenAdminUser()
        {
            var id = _cuisines[2].Id;
            var response = await _clientAdmin.DeleteAsync($"/api/cuisine/{id}");
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //check if removed
            var getResponse = await _clientAdmin.GetAsync($"/api/cuisine/{id}");

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        #endregion

        #region Unauthenticated Tests
        [Fact]
        public async Task GetCuisines_ShouldReturnListOfCuisines()
        {
            var response = await _clientUnauthenticated.GetAsync("/api/cuisine");
            var cuisinesFromDb = await response.Content.ReadFromJsonAsync<List<Cuisine>>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(cuisinesFromDb);
            Assert.True(cuisinesFromDb.Count > 0);
        }

        [Fact]
        public async Task GetCuisineById_ShouldReturnCuisine_WhenExists()
        {
            string id = _cuisines[0].Id;
            var response = await _clientUnauthenticated.GetAsync($"/api/cuisine/{id}");
            var cuisine = await response.Content.ReadFromJsonAsync<Cuisine>();

            Assert.NotNull(cuisine);
            Assert.Equal(id, cuisine.Id);
        }

        [Fact]
        public async Task GetCuisineById_ShouldReturn404_WhenCuisineNotFound()
        {
            var response = await _clientUnauthenticated.GetAsync($"/api/cuisine/67c480be772887a95c508c10");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PostCuisine_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var cuisine = new Cuisine
            {
                Name = "ToCreateUnauthorized",
                PictureUrl = "https://ToCreateUnauthorized.jpg"
            };

            var response = await _clientUnauthenticated.PostAsJsonAsync("/api/cuisine", cuisine);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PutCuisine_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var updatedCuisine = _cuisines[1];
            updatedCuisine.Name = "UpdatedUnauthorized";
            updatedCuisine.PictureUrl = "https://UpdatedUnauthorized.jpg";

            var response = await _clientUnauthenticated.PutAsJsonAsync($"/api/cuisine/{updatedCuisine.Id}", updatedCuisine);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteCuisine_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var response = await _clientUnauthenticated.DeleteAsync($"/api/cuisine/{_cuisines[0].Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        #endregion
    }
}
