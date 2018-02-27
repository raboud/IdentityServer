using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer.Data
{
	public class ConfigurationDbContextSeed
	{
		private readonly ConfigurationDbContext _context;
		private readonly IConfiguration _configuration;
		private readonly ILogger<ConfigurationDbContext> _logger;

		public ConfigurationDbContextSeed(
			ConfigurationDbContext context, 
			IConfiguration configuration,
			ILogger<ConfigurationDbContext> logger
)
		{
			_context = context;
			_configuration = configuration;
			_logger = logger;
		}

		public async Task SeedAsync()
		{
			try
			{
				if (!_context.Clients.Any())
				{
					_logger.LogInformation("Clients being populated");
					foreach (IdentityServer4.Models.Client client in Config.GetClients().ToList())
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
					foreach (IdentityServer4.Models.IdentityResource resource in Config.GetIdentityResources().ToList())
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
					foreach (IdentityServer4.Models.ApiResource resource in Config.GetApiResources().ToList())
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
