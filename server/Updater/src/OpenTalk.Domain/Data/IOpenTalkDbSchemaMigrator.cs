using System.Threading.Tasks;

namespace OpenTalk.Data;

public interface IOpenTalkDbSchemaMigrator
{
    Task MigrateAsync();
}
