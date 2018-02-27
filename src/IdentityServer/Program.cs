using IdentityServer.Data;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System.IO;

using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using System;

namespace IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "IdentityServer";

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("System", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore.Authentication", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.File(@"identityserver4_log.txt")
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level}] {SourceContext}{NewLine}{Message:lj}{NewLine}{Exception}{NewLine}", theme: AnsiConsoleTheme.Literate)
                .CreateLogger();

              BuildWebHost(args)
                .MigrateDbContext<PersistedGrantDbContext>((_, __) => { })
				.MigrateDbContext<ApplicationDbContext>((context, services) =>
				{
					services.GetService<ApplicationDbContextSeed>()
						.SeedAsync()
						.Wait();
				})
				.MigrateDbContext<ConfigurationDbContext>((context, services) =>
				{
					services.GetService<ConfigurationDbContextSeed>()
						.SeedAsync()
						.Wait();
				}).Run();


        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
				.UseKestrel()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseIISIntegration()
				.UseStartup<Startup>()
				.ConfigureAppConfiguration((builderContext, config) =>
				{
					config.AddEnvironmentVariables();
				})
				.ConfigureLogging(builder =>
				{
					builder.ClearProviders();
					builder.AddSerilog();
					builder.AddConsole();
					builder.AddDebug();
				})
				.Build();
    }
}
