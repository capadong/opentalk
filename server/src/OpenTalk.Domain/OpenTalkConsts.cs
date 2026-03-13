using Volo.Abp.Identity;

namespace OpenTalk;

public static class OpenTalkConsts
{
    public const string DbTablePrefix = "opentalk";
    public const string? DbSchema = null;
    public const string AdminEmailDefaultValue = "opentalk@opentalk.com"; //IdentityDataSeedContributor.AdminEmailDefaultValue;
    public const string AdminPasswordDefaultValue = "opentalk";
}
