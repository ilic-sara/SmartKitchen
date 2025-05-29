using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Models;
using MongoDB.Driver;
using Repositories;
using SarasKitchenApp;
using SarasKitchenApp.Client.ApiServices;
using SarasKitchenApp.Components;
using Services;
using System.Security.Claims;
using System.Text;

var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production";

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{environment}.json", optional: true)
    .AddEnvironmentVariables();

builder.Services.Configure<MongoSettings>(builder.Configuration.GetSection("MongoSettings"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var settings = sp.GetRequiredService<IOptions<MongoSettings>>().Value;
    return new MongoClient(settings.Connection);
});
builder.Services.AddHttpClient();

builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<ICuisineRepository, CuisineRepository>();
builder.Services.AddScoped<IDietRepository, DietRepository>();
builder.Services.AddScoped<IMethodRepository, MethodRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICuisineService, CuisineService>();
builder.Services.AddScoped<IDietService, DietService>();
builder.Services.AddScoped<IMethodService, MethodService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddScoped<CategoryApiService>();
builder.Services.AddScoped<CuisineApiService>();
builder.Services.AddScoped<DietApiService>();
builder.Services.AddScoped<MethodApiService>();
builder.Services.AddScoped<RecipeApiService>();
builder.Services.AddScoped<UserApiService>();
builder.Services.AddScoped<FileUploadApiService>();
builder.Services.AddScoped<RecipeGeneratorService>();
builder.Services.AddSingleton<ImageUploadService>();

var apiBaseAddress = builder.Configuration.GetValue<string>("ApiBaseAddress");
if (string.IsNullOrWhiteSpace(apiBaseAddress))
{
    throw new ArgumentNullException("ApiBaseAddress", "ApiBaseAddress configuration is missing.");
}

builder.Services.AddServerSideBlazor(options =>
{
    options.DetailedErrors = true;
});



builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]
                    ?? throw new ArgumentNullException("Jwt:Secret is missing"))
                )

        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.HttpContext.Request.Cookies["authToken"];
                if (!string.IsNullOrWhiteSpace(token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ClaimsPrincipal>(provider =>
{
    var httpContextAccessor = provider.GetRequiredService<IHttpContextAccessor>();
    return httpContextAccessor.HttpContext?.User ?? new ClaimsPrincipal();
});

builder.Services.AddAuthorizationCore();

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(SarasKitchenApp.Client._Imports).Assembly);

app.Run();
