using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;
using MongoDB.Driver;

namespace Repositories
{

    public interface IUserRepository : IBaseRepository<User>
    {
        Task<User?> GetUserByUsernameAsync(string username);
    }

    public class UserRepository(IOptions<MongoSettings> mongoSettings, ILogger<UserRepository> logger)
        : BaseRepository<User>(mongoSettings, logger), IUserRepository
    {
        public async Task<User?> GetUserByUsernameAsync(string username)
        {
            try
            {
                var filter = Builders<User>.Filter.Eq(x => x.Username, username);

                return await _mongoCollection.Find(filter).FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                logger.LogError($"{DateTime.Now.ToString("dd MMM yyyy HH:mm:ss")} " +
                    $"[ERROR] GetUserByUsernameAsync :: An error occured while fetching User with username {username} from database.\n{ex}");
                throw;
            }
        }
    }
}
