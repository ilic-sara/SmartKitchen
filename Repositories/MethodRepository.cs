using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Models;

namespace Repositories
{
    public interface IMethodRepository : IBaseRepository<Method>
    {

    }
    public class MethodRepository (IOptions<MongoSettings> mongoSettings, ILogger<MethodRepository> logger) 
        : BaseRepository<Method> (mongoSettings, logger), IMethodRepository
    {

    }
}
