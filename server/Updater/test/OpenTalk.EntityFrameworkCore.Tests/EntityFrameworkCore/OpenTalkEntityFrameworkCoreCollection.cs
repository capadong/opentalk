using Xunit;

namespace OpenTalk.EntityFrameworkCore;

[CollectionDefinition(OpenTalkTestConsts.CollectionDefinitionName)]
public class OpenTalkEntityFrameworkCoreCollection : ICollectionFixture<OpenTalkEntityFrameworkCoreFixture>
{

}
