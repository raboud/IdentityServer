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
				.ConfigureAppConfiguration((builderContext, config) =>
				{
					config.AddEnvironmentVariables();
				})
				.UseSerilog((hostingContext, loggerConfiguration) =>
					loggerConfiguration.ReadFrom.Configuration(hostingContext.Configuration)
				)
				.UseKestrel()
				.UseIISIntegration()
				.UseContentRoot(Directory.GetCurrentDirectory())
				.UseStartup<Startup>()
				.Build();
	}
}
