using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SarasKitchenApp.Client.ApiServices;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

var apiBaseAddress = builder.Configuration.GetValue<string>("ApiBaseAddress");

if (string.IsNullOrWhiteSpace(apiBaseAddress))
{
    throw new ArgumentNullException("ApiBaseAddress", "ApiBaseAddress configuration is missing.");
}

builder.Services.AddScoped<CategoryApiService>();
builder.Services.AddScoped<CuisineApiService>();
builder.Services.AddScoped<DietApiService>();
builder.Services.AddScoped<MethodApiService>();
builder.Services.AddScoped<RecipeApiService>();
builder.Services.AddScoped<UserApiService>(); 

builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
