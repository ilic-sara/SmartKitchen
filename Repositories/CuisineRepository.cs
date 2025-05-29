using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;

namespace Repositories
{
    public interface ICuisineRepository : IBaseRepository<Cuisine>
    {

    }
    public class CuisineRepository(IOptions<MongoSettings> mongoSettings, ILogger<CuisineRepository> logger) 
        : BaseRepository<Cuisine> (mongoSettings, logger), ICuisineRepository
    {

    }
}
