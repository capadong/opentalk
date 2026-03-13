using Volo.Abp.Settings;

namespace OpenTalk.Settings;

public class OpenTalkSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        //Define your own settings here. Example:
        //context.Add(new SettingDefinition(OpenTalkSettings.MySetting1));
    }
}
