using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Data
{
	public class ConfigurationDbContextSeed
	{
		private readonly ConfigurationDbContext _context;
		private readonly IConfiguration _configuration;
		private readonly ILogger<ConfigurationDbContext> _logger;
		private IHostingEnvironment _env;
		private IOptions<AppSettings> _settings;

		public ConfigurationDbContextSeed(
			ConfigurationDbContext context, 
			IConfiguration configuration,
			ILogger<ConfigurationDbContext> logger,
			IHostingEnvironment env,
			IOptions<AppSettings> settings
)
		{
			_context = context;
			_configuration = configuration;
			_logger = logger;
			_env = env;
			_settings = settings;
		}

		private async Task<Config> GetConfig()
		{
			if (_settings.Value.UseCustomizationData)
			{
				string jsonFile = Path.Combine(_env.ContentRootPath, "Setup", "Config.json");

				if (File.Exists(jsonFile))
				{
					string seedData = await File.ReadAllTextAsync(jsonFile);
					return JsonConvert.DeserializeObject<Config>(seedData);
				}
			}
			return Config.Default();
		}

		public async Task SeedAsync()
		{
			try
			{
				Config config =await GetConfig();
				if (!_context.Clients.Any())
				{
					_logger.LogInformation("Clients being populated");
					foreach (IdentityServer4.Models.Client client in config.Clients)
					{
						_context.Clients.Add(client.ToEntity());
					}
					await _context.SaveChangesAsync();
				}
				else
				{
					_logger.LogInformation("Clients already populated");
				}

				if (!_context.IdentityResources.Any())
				{
					_logger.LogInformation("IdentityResources being populated");
					foreach (IdentityServer4.Models.IdentityResource resource in config.IdentityResources)
					{
						_context.IdentityResources.Add(resource.ToEntity());
					}
					await _context.SaveChangesAsync();
				}
				else
				{
					_logger.LogInformation("IdentityResources already populated");
				}

				if (!_context.ApiResources.Any())
				{
					_logger.LogInformation("ApiResources being populated");
					foreach (IdentityServer4.Models.ApiResource resource in config.ApiResources)
					{
						_context.ApiResources.Add(resource.ToEntity());
					}
					await _context.SaveChangesAsync();
				}
				else
				{
					_logger.LogInformation("ApiResources already populated");
				}
			}
			catch (Exception e)
			{
				_logger.LogError(e, "SeedAsync");
			}
		}
	}
}
