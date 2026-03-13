using Volo.Abp.Modularity;

namespace OpenTalk;

/* Inherit from this class for your domain layer tests. */
public abstract class OpenTalkDomainTestBase<TStartupModule> : OpenTalkTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
