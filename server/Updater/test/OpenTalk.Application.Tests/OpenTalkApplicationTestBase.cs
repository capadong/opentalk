using Volo.Abp.Modularity;

namespace OpenTalk;

public abstract class OpenTalkApplicationTestBase<TStartupModule> : OpenTalkTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
