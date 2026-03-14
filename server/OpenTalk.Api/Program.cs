using Dapper;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTalk.Application.Services;
using OpenTalk.Hubs;
using OpenTalk.Infrastructure;
using OpenTalk.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                     .AddEnvironmentVariables();

builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Default", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials()
              .SetIsOriginAllowed(_ => true);
    });
});

builder.Services.AddSingleton<IDbConnectionFactory>(_ =>
    new MySqlConnectionFactory(builder.Configuration.GetConnectionString("Default") ?? ""));
builder.Services.AddScoped<IMessageRepository, DapperMessageRepository>();
builder.Services.AddScoped<IGroupRepository, DapperGroupRepository>();
builder.Services.AddScoped<IUserRepository, DapperUserRepository>();
builder.Services.AddScoped<MessageService>();
builder.Services.AddScoped<GroupService>();
builder.Services.AddScoped<UserService>();

var app = builder.Build();

app.UseCors("Default");
app.UseSwagger();
app.UseSwaggerUI();
app.UseStaticFiles();
app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();
