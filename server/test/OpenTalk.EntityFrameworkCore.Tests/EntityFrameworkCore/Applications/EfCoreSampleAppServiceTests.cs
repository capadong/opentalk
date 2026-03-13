using OpenTalk.Samples;
using Xunit;

namespace OpenTalk.EntityFrameworkCore.Applications;

[Collection(OpenTalkTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<OpenTalkEntityFrameworkCoreTestModule>
{

}
