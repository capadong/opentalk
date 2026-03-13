using Microsoft.AspNetCore.Builder;
using OpenTalk;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();
builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("OpenTalk.Web.csproj"); 
await builder.RunAbpModuleAsync<OpenTalkWebTestModule>(applicationName: "OpenTalk.Web");

public partial class Program
{
}
