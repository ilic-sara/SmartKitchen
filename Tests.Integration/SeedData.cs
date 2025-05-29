using Models;
using System.Net.Http.Json;

namespace Tests.Integration
{
    public static class SeedData
    {
        public static List<Category> Categories =
        [
            new() { Name = "ToRead", PictureUrl = "https://ToRead.jpg" },
            new() { Name = "ToUpdate", PictureUrl = "https://ToUpdate.jpg" },
            new() { Name = "ToDelete", PictureUrl = "https://ToDelete.jpg" }
        ];

        public static List<Cuisine> Cuisines =
        [
            new() { Name = "ToRead", PictureUrl = "https://ToRead.jpg" },
            new() { Name = "ToUpdate", PictureUrl = "https://ToUpdate.jpg" },
            new() { Name = "ToDelete", PictureUrl = "https://ToDelete.jpg" }
        ];

        public static List<Diet> Diets =
        [
            new() { Name = "ToRead", PictureUrl = "https://ToRead.jpg" },
            new() { Name = "ToUpdate", PictureUrl = "https://ToUpdate.jpg" },
            new() { Name = "ToDelete", PictureUrl = "https://ToDelete.jpg" }
        ];

        public static List<Method> Methods =
        [
            new() { Name = "ToRead", PictureUrl = "https://ToRead.jpg" },
            new() { Name = "ToUpdate", PictureUrl = "https://ToUpdate.jpg" },
            new() { Name = "ToDelete", PictureUrl = "https://ToDelete.jpg" }
        ];


        public static async Task InsertCategories(HttpClient client)
        {
            for(int i = 0; i< Categories.Count; i++)
            {
                var response = await client.PostAsJsonAsync("/api/category", Categories[i]);
                var savedCategory = await response.Content.ReadFromJsonAsync<Category>();
                Categories[i].Id = savedCategory?.Id ?? throw new NullReferenceException("Saving category failed");
            }
        }

        public static async Task InsertCuisines(HttpClient client)
        {
            for (int i = 0; i < Cuisines.Count; i++)
            {
                var response = await client.PostAsJsonAsync("/api/cuisine", Cuisines[i]);
                var savedCuisine = await response.Content.ReadFromJsonAsync<Cuisine>();
                Cuisines[i].Id = savedCuisine?.Id ?? throw new NullReferenceException("Saving cuisine failed");
            }
        }

        public static async Task InsertDiets(HttpClient client)
        {
            for (int i = 0; i < Diets.Count; i++)
            {
                var response = await client.PostAsJsonAsync("/api/diet", Diets[i]);
                var savedDiet = await response.Content.ReadFromJsonAsync<Diet>();
                Diets[i].Id = savedDiet?.Id ?? throw new NullReferenceException("Saving diet failed");
            }
        }

        public static async Task InsertMethods(HttpClient client)
        {
            for (int i = 0; i < Methods.Count; i++)
            {
                var response = await client.PostAsJsonAsync("/api/method", Methods[i]);
                var savedMethod = await response.Content.ReadFromJsonAsync<Method>();
                Methods[i].Id = savedMethod?.Id ?? throw new NullReferenceException("Saving method failed");
            }
        }
    }

}
