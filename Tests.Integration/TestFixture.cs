using Microsoft.Extensions.Configuration;
using Models;
using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Tests.Integration
{
    public class TestFixture : IDisposable
    {
        public HttpClient AdminClient { get; }
        public HttpClient UnauthenticatedClient { get; }
        public List<Category> Categories { get; }
        public List<Cuisine> Cuisines { get; }
        public List<Diet> Diets { get; }
        public List<Method> Methods { get; }

        private string _authToken;
        private string testUsername;
        private string testPassword;

        public TestFixture()
        {
            var factory = new CustomWebApplicationFactory<Program>();

            AdminClient = factory.CreateClient();
            UnauthenticatedClient = factory.CreateClient();

            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.IntegrationTests.json")
                .Build();

            testUsername = configuration["TestUser:Username"] ?? throw new NullReferenceException("TestUser's Username is null.");
            testPassword = configuration["TestUser:Password"] ?? throw new NullReferenceException("TestUser's Password is null.");

            CreateNewTestUser().Wait();

            _authToken = AuthenticateTestUser().GetAwaiter().GetResult();
            AdminClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _authToken);

            Task.Run(async () => await SeedAllData(AdminClient)).GetAwaiter().GetResult();


            Categories = SeedData.Categories;
            Cuisines = SeedData.Cuisines;
            Diets = SeedData.Diets;
            Methods = SeedData.Methods;
        }

        private async Task SeedAllData(HttpClient client)
        {
            await SeedData.InsertCategories(client);
            await SeedData.InsertCuisines(client);
            await SeedData.InsertDiets(client);
            await SeedData.InsertMethods(client);
        }

        private async Task<string> AuthenticateTestUser()
        {
            UserLoginModel loginModel = new()
            {
                Username = testUsername,
                Password = testPassword
            };

            var response = await AdminClient.PostAsJsonAsync("/api/user/authenticate", loginModel);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var token = JObject.Parse(jsonResponse)["token"]?.ToString();

            return token ?? throw new InvalidOperationException("Failed to retrieve authentication token.");
        }

        private async Task CreateNewTestUser()
        {
            UserSignUpModel signUpModel = new()
            {
                Username = testUsername,
                Password = testPassword,
                ConfirmPassword = testPassword
            };

            await AdminClient.PostAsJsonAsync("/api/user", signUpModel);
        }

        public void Dispose()
        {

        }
    }

}
