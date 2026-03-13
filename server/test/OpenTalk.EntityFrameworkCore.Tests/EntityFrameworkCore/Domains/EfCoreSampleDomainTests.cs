using OpenTalk.Samples;
using Xunit;

namespace OpenTalk.EntityFrameworkCore.Domains;

[Collection(OpenTalkTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<OpenTalkEntityFrameworkCoreTestModule>
{

}
