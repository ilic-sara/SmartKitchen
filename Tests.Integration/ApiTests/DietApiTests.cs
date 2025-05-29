using System.Net;
using System.Net.Http.Json;
using Models;

namespace Tests.Integration.ApiTests
{
    public class DietApiTests : IClassFixture<TestFixture>
    {
        private readonly HttpClient _clientAdmin;
        private readonly HttpClient _clientUnauthenticated;
        private readonly List<Diet> _diets;

        public DietApiTests(TestFixture fixture)
        {
            _clientAdmin = fixture.AdminClient;
            _clientUnauthenticated = fixture.UnauthenticatedClient;
            _diets = fixture.Diets;
        }


        #region Admin Tests

        [Fact]
        public async Task PostDiet_ShouldReturn201_AndPersistInDb_WhenAdminUser()
        {
            var diet = new Diet
            {
                Name = "ToCreate",
                PictureUrl = "https://ToCreate.jpg"
            };

            var response = await _clientAdmin.PostAsJsonAsync("/api/diet", diet);
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var addedDiet = await response.Content.ReadFromJsonAsync<Diet>();
            string id = addedDiet?.Id ?? throw new Exception("Diet not saved.");

            //check if inserted
            var getResponse = await _clientAdmin.GetAsync($"/api/diet/{id}");
            var fetchedDiet = await getResponse.Content.ReadFromJsonAsync<Diet>();

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.NotNull(fetchedDiet);
            Assert.Equal("ToCreate", fetchedDiet.Name);
            Assert.Equal("https://ToCreate.jpg", fetchedDiet.PictureUrl);
        }

        [Fact]
        public async Task PutDiet_ShouldReturn204_AndUpdateDiet_WhenAdminUser()
        {
            var toUpdate = _diets[1];
            toUpdate.Name = "UpdatedDiet";
            toUpdate.PictureUrl = "https://UpdatedDiet.jpg";

            var response = await _clientAdmin.PutAsJsonAsync($"/api/diet/{toUpdate.Id}", toUpdate);
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //check if updated
            var getResponse = await _clientAdmin.GetAsync($"/api/diet/{toUpdate.Id}");
            var fetchedDiet = await getResponse.Content.ReadFromJsonAsync<Diet>();

            Assert.NotNull(fetchedDiet);
            Assert.Equal("UpdatedDiet", fetchedDiet.Name);
            Assert.Equal("https://UpdatedDiet.jpg", fetchedDiet.PictureUrl);
        }

        [Fact]
        public async Task DeleteDiet_ShouldReturn204_AndRemoveFromDb_WhenAdminUser()
        {
            var id = _diets[2].Id;
            var response = await _clientAdmin.DeleteAsync($"/api/diet/{id}");
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //check if removed
            var getResponse = await _clientAdmin.GetAsync($"/api/diet/{id}");

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        #endregion

        #region Unauthenticated Tests
        [Fact]
        public async Task GetDiets_ShouldReturnListOfDiets()
        {
            var response = await _clientUnauthenticated.GetAsync("/api/diet");
            var dietsFromDb = await response.Content.ReadFromJsonAsync<List<Diet>>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(dietsFromDb);
            Assert.True(dietsFromDb.Count > 0);
        }

        [Fact]
        public async Task GetDietById_ShouldReturnDiet_WhenExists()
        {
            string id = _diets[0].Id;
            var response = await _clientUnauthenticated.GetAsync($"/api/diet/{id}");
            var diet = await response.Content.ReadFromJsonAsync<Diet>();

            Assert.NotNull(diet);
            Assert.Equal(id, diet.Id);
        }

        [Fact]
        public async Task GetDietById_ShouldReturn404_WhenDietNotFound()
        {
            var response = await _clientUnauthenticated.GetAsync($"/api/diet/67c480be772887a95c508c10");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PostDiet_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var diet = new Diet
            {
                Name = "ToCreateUnauthorized",
                PictureUrl = "https://ToCreateUnauthorized.jpg"
            };

            var response = await _clientUnauthenticated.PostAsJsonAsync("/api/diet", diet);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PutDiet_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var updatedDiet = _diets[1];
            updatedDiet.Name = "UpdatedUnauthorized";
            updatedDiet.PictureUrl = "https://UpdatedUnauthorized.jpg";

            var response = await _clientUnauthenticated.PutAsJsonAsync($"/api/diet/{updatedDiet.Id}", updatedDiet);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteDiet_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var response = await _clientUnauthenticated.DeleteAsync($"/api/diet/{_diets[0].Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        #endregion
    }
}
