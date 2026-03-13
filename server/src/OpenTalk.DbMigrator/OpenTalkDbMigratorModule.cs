using OpenTalk.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace OpenTalk.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(OpenTalkEntityFrameworkCoreModule),
    typeof(OpenTalkApplicationContractsModule)
)]
public class OpenTalkDbMigratorModule : AbpModule
{
}
