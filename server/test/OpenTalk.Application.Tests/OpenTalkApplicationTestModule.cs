using Volo.Abp.Modularity;

namespace OpenTalk;

[DependsOn(
    typeof(OpenTalkApplicationModule),
    typeof(OpenTalkDomainTestModule)
)]
public class OpenTalkApplicationTestModule : AbpModule
{

}
