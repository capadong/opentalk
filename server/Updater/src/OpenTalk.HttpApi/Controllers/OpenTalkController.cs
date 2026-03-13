using OpenTalk.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace OpenTalk.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class OpenTalkController : AbpControllerBase
{
    protected OpenTalkController()
    {
        LocalizationResource = typeof(OpenTalkResource);
    }
}
