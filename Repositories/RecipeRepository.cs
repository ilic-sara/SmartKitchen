using System.Collections;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace Repositories
{
    public interface IRecipeRepository : IBaseRepository<Recipe>
    {
        Task<List<Recipe>> GetAllRecipesFromLatest(int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetRecipesByCategory(string id, int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetRecipesByCuisine(string id, int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetRecipesByDiet(string id, int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetRecipesByMethod(string id, int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetRecipesByName(string name, int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetNewestRecipes(int startIndex, int numberOfObjects);
        Task<List<Recipe>> GetRecipesByIngredients(List<string> ingredients, int startIndex, int numberOfObjects);
        Task<long> GetNumberOfRecipesByCategory(string id);
        Task<long> GetNumberOfRecipesByCuisine(string id);
        Task<long> GetNumberOfRecipesByDiet(string id);
        Task<long> GetNumberOfRecipesByMethod(string id);
        Task<long> GetNumberOfRecipesByName(string name);
        Task<long> GetNumberOfRecipesByIngredients(List<string> ingredients);
        Task UpdateCategoryInRecipes(string categoryId, string newName, string newPictureUrl, IClientSessionHandle session);
        Task UpdateCuisineInRecipes(string cuisineId, string newName, string newPictureUrl, IClientSessionHandle session);
        Task UpdateDietInRecipes(string dietId, string newName, string newPictureUrl, IClientSessionHandle session);
        Task UpdateMethodInRecipes(string methodId, string newName, string newPictureUrl, IClientSessionHandle session);
        Task DeleteCategoryInRecipes(string categoryId, IClientSessionHandle session);
        Task DeleteCuisineInRecipes(string cuisineId, IClientSessionHandle session);
        Task DeleteDietInRecipes(string dietId, IClientSessionHandle session);
        Task DeleteMethodInRecipes(string methodId, IClientSessionHandle session);
    }
    public class RecipeRepository(IOptions<MongoSettings> mongoSettings, ILogger<RecipeRepository> logger)
        : BaseRepository<Recipe>(mongoSettings, logger), IRecipeRepository
    {

        public async Task<List<Recipe>> GetAllRecipesFromLatest(int startIndex, int numberOfObjects)
        {
            try
            {
                var filter = Builders<Recipe>.Filter.Empty;

                return await _mongoCollection.Find(filter)
                                             .SortByDescending(r => r.CreatedAt)
                                             .Skip(startIndex * numberOfObjects)
                                             .Limit(numberOfObjects)
                                             .ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllRecipesFromLatest :: An error occured while fetching Recipes from database.\n" +
                    $"Start index is {startIndex}, number of objects is {numberOfObjects}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByCategory(string id, int startIndex, int numberOfObjects)
        {
            try
            {
                var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Categories, c => c.Id == id);

                return await _mongoCollection.Find(filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync();
            }
            catch (Exception ex) 
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByCategory :: An error occured while fetching recipes with category id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByCuisine(string id, int startIndex, int numberOfObjects)
        {
            try
            {
                var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Cuisines, c => c.Id == id);

                return await _mongoCollection.Find(filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByCuisine :: An error occured while fetching recipes with cuisine id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByDiet(string id, int startIndex, int numberOfObjects)
        {
            try
            {
                var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Diets, d => d.Id == id);

                return await _mongoCollection.Find(filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByDiet :: An error occured while fetching recipes with diet id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByMethod(string id, int startIndex, int numberOfObjects)
        {
            try
            {
                var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Methods, m => m.Id == id);

                return await _mongoCollection.Find(filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByMethod :: An error occured while fetching recipes with method id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByName(string name, int startIndex, int numberOfObjects)
        {
            try
            {
                var regexPattern = new BsonRegularExpression(name, "i");
                var filter = Builders<Recipe>.Filter.Regex(x => x.Name, regexPattern);

                return await _mongoCollection.Find(filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByName :: An error occured while fetching recipes with name {name}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetRecipesByIngredients(List<string> ingredients, int startIndex, int numberOfObjects)
        {
            try
            {
                var regexFilters = ingredients.Select(ingredient =>
                            Builders<Recipe>.Filter.ElemMatch(x => x.Ingredients, i => i.Name.ToLower().Contains(ingredient.ToLower()))
                        ).ToList();

                var filter = Builders<Recipe>.Filter.And(regexFilters);

                return await _mongoCollection.Find(filter).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetRecipesByIngredients :: An error occured while fetching recipes by the following ingredients {string.Join(", ", ingredients)}.\n{ex}");
                throw;
            }
        }

        public async Task<List<Recipe>> GetNewestRecipes(int startIndex, int numberOfObjects)
        {
            try
            {
                var filter = Builders<Recipe>.Filter.Empty;

                return await _mongoCollection.Find(filter).SortByDescending(r => r.CreatedAt).Skip(startIndex * numberOfObjects).Limit(numberOfObjects).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNewestRecipes :: An error occured while fetching {numberOfObjects} newest recipes, starting from page {startIndex}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByCategory(string id)
        {
            try
            {
                var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Categories, c => c.Id == id);

                return await _mongoCollection.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByCategory :: An error occured while fetching total number of recipes with category id {id}.\n{ex}");
                throw;
            }
        }


        public async Task<long> GetNumberOfRecipesByCuisine(string id)
        {
            try
            {
                var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Cuisines, c => c.Id == id);

                return await _mongoCollection.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByCuisine :: An error occured while fetching total number of recipes with cuisine id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByDiet(string id)
        {
            try
            {
                var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Diets, d => d.Id == id);

                return await _mongoCollection.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByDiet :: An error occured while fetching total number of recipes with diet id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByMethod(string id)
        {
            try
            {
                var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Methods, m => m.Id == id);

                return await _mongoCollection.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByMethod :: An error occured while fetching total number of recipes with method id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByName(string name)
        {
            try
            {
                var regexPattern = new BsonRegularExpression(name, "i");
                var filter = Builders<Recipe>.Filter.Regex(x => x.Name, regexPattern);

                return await _mongoCollection.CountDocumentsAsync(filter);
            }
            catch (Exception ex) 
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByName :: An error occured while fetching total number of recipes with name {name}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfRecipesByIngredients(List<string> ingredients)
        {
            try
            {
                var regexFilters = ingredients.Select(ingredient =>
                            Builders<Recipe>.Filter.ElemMatch(x => x.Ingredients, i => i.Name.ToLower().Contains(ingredient.ToLower()))
                        ).ToList();

                var filter = Builders<Recipe>.Filter.And(regexFilters);
                return await _mongoCollection.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                       $"[ERROR] GetNumberOfRecipesByIngredients :: An error occured while fetching total number of recipes by the following ingredients {string.Join(", ", ingredients)}.\n{ex}");
                throw;
            }
        }

        public async Task UpdateCategoryInRecipes(string categoryId, string newName, string newPictureUrl, IClientSessionHandle session)
        {
            var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Categories, c => c.Id == categoryId);
            var update = Builders<Recipe>.Update
                .Set("Categories.$[elem].Name", newName)
                .Set("Categories.$[elem].PictureUrl", newPictureUrl);
            var options = new UpdateOptions
            {
                ArrayFilters =
                [
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem._id", new ObjectId(categoryId)))
                ]
            };

            await _mongoCollection.UpdateManyAsync(session, filter, update, options);
        }

        public async Task UpdateCuisineInRecipes(string cuisineId, string newName, string newPictureUrl, IClientSessionHandle session)
        {
            var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Cuisines, c => c.Id == cuisineId);
            var update = Builders<Recipe>.Update
                .Set("Cuisines.$[elem].Name", newName)
                .Set("Cuisines.$[elem].PictureUrl", newPictureUrl);
            var options = new UpdateOptions
            {
                ArrayFilters =
                [
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem._id", new ObjectId(cuisineId)))
                ]
            };

            await _mongoCollection.UpdateManyAsync(session, filter, update, options);
        }

        public async Task UpdateDietInRecipes(string dietId, string newName, string newPictureUrl, IClientSessionHandle session)
        {
            var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Diets, d => d.Id == dietId);
            var update = Builders<Recipe>.Update
                .Set("Diets.$[elem].Name", newName)
                .Set("Diets.$[elem].PictureUrl", newPictureUrl);
            var options = new UpdateOptions
            {
                ArrayFilters =
                [
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem._id", new ObjectId(dietId)))
                ]
            };

            await _mongoCollection.UpdateManyAsync(session, filter, update, options);
        }

        public async Task UpdateMethodInRecipes(string methodId, string newName, string newPictureUrl, IClientSessionHandle session)
        {
            var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Methods, m => m.Id == methodId);
            var update = Builders<Recipe>.Update
                .Set("Methods.$[elem].Name", newName)
                .Set("Methods.$[elem].PictureUrl", newPictureUrl);
            var options = new UpdateOptions
            {
                ArrayFilters = 
                [
                    new BsonDocumentArrayFilterDefinition<BsonDocument>(new BsonDocument("elem._id", new ObjectId(methodId)))
                ]
            };

            await _mongoCollection.UpdateManyAsync(session, filter, update, options);
        }

        public async Task DeleteCategoryInRecipes(string categoryId, IClientSessionHandle session)
        {
            var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Categories, c => c.Id == categoryId);
            var update = Builders<Recipe>.Update.PullFilter(r => r.Categories, c => c.Id == categoryId);

            await _mongoCollection.UpdateManyAsync(session, filter, update);
        }

        public async Task DeleteCuisineInRecipes(string cuisineId, IClientSessionHandle session)
        {
            var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Cuisines, c => c.Id == cuisineId);
            var update = Builders<Recipe>.Update.PullFilter(r => r.Cuisines, c => c.Id == cuisineId);

            await _mongoCollection.UpdateManyAsync(session, filter, update);
        }

        public async Task DeleteDietInRecipes(string dietId, IClientSessionHandle session)
        {
            var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Diets, d => d.Id == dietId);
            var update = Builders<Recipe>.Update.PullFilter(r => r.Diets, d => d.Id == dietId);

            await _mongoCollection.UpdateManyAsync(session, filter, update);
        }

        public async Task DeleteMethodInRecipes(string methodId, IClientSessionHandle session)
        {
            var filter = Builders<Recipe>.Filter.ElemMatch(r => r.Methods, m => m.Id == methodId);
            var update = Builders<Recipe>.Update.PullFilter(r => r.Methods, m => m.Id == methodId);

            await _mongoCollection.UpdateManyAsync(session, filter, update);
        }

    }
}
