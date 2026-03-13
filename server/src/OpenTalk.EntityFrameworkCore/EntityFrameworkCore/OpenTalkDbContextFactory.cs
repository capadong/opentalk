using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OpenTalk.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class OpenTalkDbContextFactory : IDesignTimeDbContextFactory<OpenTalkDbContext>
{
    public OpenTalkDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        OpenTalkEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<OpenTalkDbContext>()
            .UseMySQL(configuration.GetConnectionString("Default"));
        
        return new OpenTalkDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../OpenTalk.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
