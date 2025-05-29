using System.Net;
using System.Net.Http.Json;
using Models;

namespace Tests.Integration.ApiTests
{
    public class MethodApiTests : IClassFixture<TestFixture>
    {
        private readonly HttpClient _clientAdmin;
        private readonly HttpClient _clientUnauthenticated;
        private readonly List<Method> _methods;

        public MethodApiTests(TestFixture fixture)
        {
            _clientAdmin = fixture.AdminClient;
            _clientUnauthenticated = fixture.UnauthenticatedClient;
            _methods = fixture.Methods;
        }


        #region Admin Tests

        [Fact]
        public async Task PostMethod_ShouldReturn201_AndPersistInDb_WhenAdminUser()
        {
            var method = new Method
            {
                Name = "ToCreate",
                PictureUrl = "https://ToCreate.jpg"
            };

            var response = await _clientAdmin.PostAsJsonAsync("/api/method", method);
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var addedMethod = await response.Content.ReadFromJsonAsync<Method>();
            string id = addedMethod?.Id ?? throw new Exception("Method not saved.");

            //check if inserted
            var getResponse = await _clientAdmin.GetAsync($"/api/method/{id}");
            var fetchedMethod = await getResponse.Content.ReadFromJsonAsync<Method>();

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            Assert.NotNull(fetchedMethod);
            Assert.Equal("ToCreate", fetchedMethod.Name);
            Assert.Equal("https://ToCreate.jpg", fetchedMethod.PictureUrl);
        }

        [Fact]
        public async Task PutMethod_ShouldReturn204_AndUpdateMethod_WhenAdminUser()
        {
            var toUpdate = _methods[1];
            toUpdate.Name = "UpdatedMethod";
            toUpdate.PictureUrl = "https://UpdatedMethod.jpg";

            var response = await _clientAdmin.PutAsJsonAsync($"/api/method/{toUpdate.Id}", toUpdate);
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //check if updated
            var getResponse = await _clientAdmin.GetAsync($"/api/method/{toUpdate.Id}");
            var fetchedMethod = await getResponse.Content.ReadFromJsonAsync<Method>();

            Assert.NotNull(fetchedMethod);
            Assert.Equal("UpdatedMethod", fetchedMethod.Name);
            Assert.Equal("https://UpdatedMethod.jpg", fetchedMethod.PictureUrl);
        }

        [Fact]
        public async Task DeleteMethod_ShouldReturn204_AndRemoveFromDb_WhenAdminUser()
        {
            var id = _methods[2].Id;
            var response = await _clientAdmin.DeleteAsync($"/api/method/{id}");
            response.EnsureSuccessStatusCode();

            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            //check if removed
            var getResponse = await _clientAdmin.GetAsync($"/api/method/{id}");

            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        #endregion

        #region Unauthenticated Tests
        [Fact]
        public async Task GetMethods_ShouldReturnListOfMethods()
        {
            var response = await _clientUnauthenticated.GetAsync("/api/method");
            var methodsFromDb = await response.Content.ReadFromJsonAsync<List<Method>>();

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.NotNull(methodsFromDb);
            Assert.True(methodsFromDb.Count > 0);
        }

        [Fact]
        public async Task GetMethodById_ShouldReturnMethod_WhenExists()
        {
            string id = _methods[0].Id;
            var response = await _clientUnauthenticated.GetAsync($"/api/method/{id}");
            var method = await response.Content.ReadFromJsonAsync<Method>();

            Assert.NotNull(method);
            Assert.Equal(id, method.Id);
        }

        [Fact]
        public async Task GetMethodById_ShouldReturn404_WhenMethodNotFound()
        {
            var response = await _clientUnauthenticated.GetAsync($"/api/method/67c480be772887a95c508c10");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task PostMethod_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var method = new Method
            {
                Name = "ToCreateUnauthorized",
                PictureUrl = "https://ToCreateUnauthorized.jpg"
            };

            var response = await _clientUnauthenticated.PostAsJsonAsync("/api/method", method);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task PutMethod_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var updatedMethod = _methods[1];
            updatedMethod.Name = "UpdatedUnauthorized";
            updatedMethod.PictureUrl = "https://UpdatedUnauthorized.jpg";

            var response = await _clientUnauthenticated.PutAsJsonAsync($"/api/method/{updatedMethod.Id}", updatedMethod);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task DeleteMethod_ShouldReturn401_WhenUnauthenticatedUser()
        {
            var response = await _clientUnauthenticated.DeleteAsync($"/api/method/{_methods[0].Id}");

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        #endregion
    }
}
