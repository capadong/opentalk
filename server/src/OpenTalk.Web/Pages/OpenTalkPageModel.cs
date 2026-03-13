using OpenTalk.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace OpenTalk.Web.Pages;

public abstract class OpenTalkPageModel : AbpPageModel
{
    protected OpenTalkPageModel()
    {
        LocalizationResourceType = typeof(OpenTalkResource);
    }
}
