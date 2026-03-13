using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace OpenTalk.Data;

/* This is used if database provider does't define
 * IOpenTalkDbSchemaMigrator implementation.
 */
public class NullOpenTalkDbSchemaMigrator : IOpenTalkDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
