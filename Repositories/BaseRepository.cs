using Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDbGenericRepository.Attributes;
using System.Reflection;

namespace Repositories
{
    public interface IBaseRepository<TEntity>
    {
        Task<List<TEntity>> GetAllAsync();
        Task<TEntity?> GetOneByIdAsync(string id);
        Task<List<TEntity>> GetMultipleByIdsAsync(List<string> listOfIds);
        Task<string> InsertAsync(TEntity obj);
        Task<bool> DeleteAsync(string id, IClientSessionHandle? session = null);
        Task<bool> DeleteManyAsync(List<string> ids, IClientSessionHandle? session = null);
        Task<bool> UpdateAsync(TEntity obj, IClientSessionHandle? session = null);
        Task<long> GetNumberOfDocumentsAsync();
    }

    public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : Base
    {
        private readonly IMongoClient _mongoClient;
        private readonly IMongoDatabase _mongoDatabase;
        public readonly IMongoCollection<TEntity> _mongoCollection;
        private readonly ILogger<BaseRepository<TEntity>> logger;
        private string? collectionName;


        public BaseRepository(IOptions<MongoSettings> mongoSettings, ILogger<BaseRepository<TEntity>> logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(mongoSettings?.Value.Connection))
                throw new ArgumentNullException(nameof(mongoSettings));

            if (string.IsNullOrWhiteSpace(mongoSettings?.Value.DatabaseName))
                throw new ArgumentNullException(nameof(mongoSettings));

            _mongoClient = new MongoClient(mongoSettings?.Value.Connection);
            _mongoDatabase = _mongoClient.GetDatabase(mongoSettings?.Value.DatabaseName);

            var collectionNameAttribute = typeof(TEntity).GetTypeInfo().GetCustomAttribute<CollectionNameAttribute>();
            collectionName = collectionNameAttribute?.Name;

            if (string.IsNullOrWhiteSpace(collectionName))
                throw new ArgumentNullException($"The entity type {typeof(TEntity).Name} does not have a valid collection name specified.");

            _mongoCollection = _mongoDatabase.GetCollection<TEntity>(collectionName);
        }

        public async Task<List<TEntity>> GetAllAsync()
        {
            try
            {
                var filter = Builders<TEntity>.Filter.Empty;

                return await _mongoCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetAllAsync :: An error occured while fetching all {collectionName} data from database.\n{ex}");
                throw;
            }

        }

        public async Task<TEntity?> GetOneByIdAsync(string id)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    throw new ArgumentException($"Invalid ID format, couldn't parse ObjectId from provided string {id}.");
                }

                var filter = Builders<TEntity>.Filter.Eq("_id", objectId);

                return await _mongoCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetOneByIdAsync :: An error occured while fetching {collectionName} with id {id} from database.\n{ex}");
                throw;
            }
        }


        public async Task<List<TEntity>> GetMultipleByIdsAsync(List<string> listOfIds)
        {
            try
            {
                var objectIdList = listOfIds.Select(id => ObjectId.Parse(id)).ToList();
                var filter = Builders<TEntity>.Filter.In("_id", objectIdList);

                return await _mongoCollection.Find(filter).ToListAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetMultipleByIdsAsync :: An error occured while fetching {collectionName} data with ids {string.Join(", ", listOfIds)} from database.\n{ex}");
                throw;
            }
        }

        public async Task<string> InsertAsync(TEntity obj)
        {
            try
            {
                await _mongoCollection.InsertOneAsync(obj);

                if (string.IsNullOrWhiteSpace(obj.Id))
                {
                    throw new InvalidOperationException($"Insert failed: The Id for the new {typeof(TEntity).Name} item is still empty after insertion.");
                }

                return obj.Id;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] InsertAsync :: An error occured while inserting new data in {collectionName}.\n{ex}");
                throw;
            }
        }

        public async Task<bool> DeleteAsync(string id, IClientSessionHandle? session = null)
        {
            try
            {
                if (!ObjectId.TryParse(id, out ObjectId objectId))
                {
                    throw new ArgumentException($"Invalid ID format, couldn't parse ObjectId from provided string {id}.");
                }

                var filter = Builders<TEntity>.Filter.Eq("_id", objectId);

                DeleteResult result;
                if (session is null)
                    result = await _mongoCollection.DeleteOneAsync(filter);
                else
                    result = await _mongoCollection.DeleteOneAsync(session, filter);

                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteAsync :: An error occured while deleting data from {collectionName} with id {id}.\n{ex}");
                throw;
            }
        }

        public async Task<bool> DeleteManyAsync(List<string> ids, IClientSessionHandle? session = null)
        {
            try
            {
                var objectIdList = ids.Select(id => ObjectId.Parse(id)).ToList();
                var filter = Builders<TEntity>.Filter.In("_id", objectIdList);

                DeleteResult result;
                if(session is null)
                    result = await _mongoCollection.DeleteManyAsync(filter);
                else 
                    result = await _mongoCollection.DeleteManyAsync(session,filter);

                return result.DeletedCount > 0;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] DeleteManyAsync :: An error occured while deleting data from {collectionName} with ids {string.Join(", ", ids)}.\n{ex}");
                throw;
            }
        }

        public async Task<bool> UpdateAsync(TEntity obj, IClientSessionHandle? session = null)
        {
            try
            {
                if (!ObjectId.TryParse(obj.Id, out ObjectId objectId))
                {
                    throw new ArgumentException($"Invalid ID format, couldn't parse ObjectId from provided string {obj.Id}.");
                }
                var filter = Builders<TEntity>.Filter.Eq("_id", objectId);

                ReplaceOneResult result;
                if (session is null)
                    result = await _mongoCollection.ReplaceOneAsync(filter, obj);
                else
                    result = await _mongoCollection.ReplaceOneAsync(session, filter, obj);

                return result.ModifiedCount > 0;
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] UpdateAsync :: An error occured while updating data from {collectionName} with id {obj.Id}.\n{ex}");
                throw;
            }
        }

        public async Task<long> GetNumberOfDocumentsAsync()
        {
            try
            {
                var filter = Builders<TEntity>.Filter.Empty;

                return await _mongoCollection.CountDocumentsAsync(filter);
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetNumberOfDocumentsAsync :: An error occured while fetching the number of documents in {collectionName} collection.\n{ex}");
                throw;
            }
        }
    }
}
