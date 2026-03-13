using OpenTalk.Localization;
using Volo.Abp.Application.Services;

namespace OpenTalk;

/* Inherit your application services from this class.
 */
public abstract class OpenTalkAppService : ApplicationService
{
    protected OpenTalkAppService()
    {
        LocalizationResource = typeof(OpenTalkResource);
    }
}
