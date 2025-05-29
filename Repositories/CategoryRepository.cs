using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;

namespace Repositories
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {

    }
    public class CategoryRepository (IOptions<MongoSettings> mongoSettings, ILogger<CategoryRepository> logger) 
        : BaseRepository<Category>(mongoSettings, logger), ICategoryRepository 
    {

    }
}
