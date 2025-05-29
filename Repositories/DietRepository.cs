using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;

namespace Repositories
{
    public interface IDietRepository : IBaseRepository<Diet>
    {

    }
    public class DietRepository(IOptions<MongoSettings> mongoSettings, ILogger<DietRepository> logger) 
        : BaseRepository<Diet>(mongoSettings, logger), IDietRepository
    {

    }
}
