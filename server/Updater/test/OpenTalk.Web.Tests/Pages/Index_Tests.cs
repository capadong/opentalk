using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace OpenTalk.Pages;

[Collection(OpenTalkTestConsts.CollectionDefinitionName)]
public class Index_Tests : OpenTalkWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
