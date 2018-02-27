using System;
using System.Linq;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using IdentityServer;
using IdentityServer.Data;
using IdentityServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;

namespace IdentityServer.Data
{
	public class ApplicationDbContextSeed
	{
		private RoleManager<IdentityRole> _roleManager;
		private UserManager<ApplicationUser> _userManager;
		private ILogger<ApplicationDbContextSeed> _logger;
		private IHostingEnvironment _env;
		private IOptions<AppSettings> _settings;


		public ApplicationDbContextSeed(
			RoleManager<IdentityRole> roleManager,
			UserManager<ApplicationUser> userManager,
			ILogger<ApplicationDbContextSeed> logger,
			IHostingEnvironment env,
			IOptions<AppSettings> settings
			)
		{
			_roleManager = roleManager;
			_userManager = userManager;
			_logger = logger;
			_env = env;
			_settings = settings;
		}

		public async Task SeedAsync(int? retry = 0)
		{
			int retryForAvaiability = retry.Value;

			try
			{
				UserSeed seed = await GetSeedData();

				await CreateRoles(seed.Roles);
				await CreateUsers(seed.Users);
				await AddRolesToUsers(seed.UserRoles);
			}
			catch (Exception ex)
			{
				if (retryForAvaiability < 10)
				{
					retryForAvaiability++;

					_logger.LogError(ex.Message, $"There is an error migrating data for ApplicationDbContext");

					await SeedAsync(retryForAvaiability);
				}
			}
		}

		private async Task<UserSeed> GetSeedData()
		{
			string seedData = "{\"roles\":[\"admin\",\"user\"],\"users\":[{\"Email\":\"BobSmith@email.com\",\"UserName\":\"bob\",\"PasswordHash\":\"Pass123$\"},{\"Email\":\"AliceSmith@email.com\",\"UserName\":\"alice\",\"PasswordHash\":\"Pass123$\"}],\"userRoles\":[{\"Email\":\"BobSmith@email.com\",\"Role\":\"admin\"},{\"Email\":\"BobSmith@email.com\",\"Role\":\"user\"},{\"Email\":\"AliceSmith@email.com\",\"Role\":\"user\"}]}";

			if (_settings.Value.UseCustomizationData)
			{
				string jsonFile = Path.Combine(_env.ContentRootPath, "Setup", "UserSeed.json");

				if (File.Exists(jsonFile))
				{
					seedData = await File.ReadAllTextAsync(jsonFile);
				}
			}

			UserSeed seed = JsonConvert.DeserializeObject<UserSeed>(seedData);
			return seed;
		}


		private async Task CreateUsers(List<ApplicationUser> users)
		{
			//initializing custom roles 
			IdentityResult result;

			foreach (ApplicationUser user in users)
			{
				ApplicationUser appUser = await _userManager.FindByEmailAsync(user.Email);
				if (appUser == null)
				{
					string password = user.PasswordHash;
					user.PasswordHash = null;
					//create the roles and seed them to the database: Question 1
					result = await _userManager.CreateAsync(user, password);
				}
			}
		}

		private async Task CreateRoles(List<string> roles)
		{
			//initializing custom roles 
			IdentityResult roleResult;

			foreach (string roleName in roles)
			{
				bool roleExist = await _roleManager.RoleExistsAsync(roleName);
				if (!roleExist)
				{
					//create the roles and seed them to the database: Question 1
					roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
				}
			}
		}

		private async Task AddRolesToUsers(List<UserSeed.UserRole> userRoles)
		{
			IdentityResult result;

			foreach (UserSeed.UserRole userRole in userRoles)
			{
				if (await _roleManager.RoleExistsAsync(userRole.Role))
				{
					ApplicationUser user = await _userManager.FindByEmailAsync(userRole.Email);
					if (user != null)
					{
						result = await _userManager.AddToRoleAsync(user, userRole.Role);
					}
				}
			}
		}


	}
}
