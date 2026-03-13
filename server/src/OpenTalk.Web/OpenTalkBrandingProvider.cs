using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenTalk.Localization;

namespace OpenTalk.Web;

[Dependency(ReplaceServices = true)]
public class OpenTalkBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<OpenTalkResource> _localizer;

    public OpenTalkBrandingProvider(IStringLocalizer<OpenTalkResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
