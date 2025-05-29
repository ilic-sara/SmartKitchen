using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace Tests.Integration
{
    public class CustomWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddJsonFile("appsettings.IntegrationTests.json", optional: false);
            });

            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IMongoClient));
                if (descriptor != null) services.Remove(descriptor);

                services.AddSingleton<IMongoClient>(_ => new MongoClient("mongodb://localhost:27017"));

                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var dbClient = scope.ServiceProvider.GetRequiredService<IMongoClient>();
                var database = dbClient.GetDatabase("IntegrationTestDB");

                database.DropCollection("Categories");
                database.DropCollection("Cuisines");
                database.DropCollection("Diets");
                database.DropCollection("Methods");
                database.DropCollection("Users");

                database.CreateCollection("Categories");
                database.CreateCollection("Cuisines");
                database.CreateCollection("Diets");
                database.CreateCollection("Methods");
                database.CreateCollection("Users");
            });
        }
    }
}

