using Volo.Abp.Modularity;

namespace OpenTalk;

[DependsOn(
    typeof(OpenTalkDomainModule),
    typeof(OpenTalkTestBaseModule)
)]
public class OpenTalkDomainTestModule : AbpModule
{

}
