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

				await EnsureSeedData();
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
			string seedData = "{\"Roles\":[\"admin\",\"user\"],\"Users\":[{\"CardHolderName\":\"DemoUser\",\"CardNumber\":\"4012888888881881\",\"CardType\":1,\"City\":\"Redmond\",\"Country\":\"U.S.\",\"Email\":\"demouser@microsoft.com\",\"Expiration\":\"12/20\",\"LastName\":\"DemoLastName\",\"Name\":\"DemoUser\",\"PhoneNumber\":\"1234567890\",\"UserName\":\"demouser@microsoft.com\",\"ZipCode\":\"98052\",\"State\":\"WA\",\"Street\":\"15703 NE 61st Ct\",\"SecurityNumber\":\"535\",\"PasswordHash\":\"Pass@word1\"},{\"CardHolderName\":\"DemoAdmin\",\"CardNumber\":\"4012888888881881\",\"CardType\":1,\"City\":\"Redmond\",\"Country\":\"U.S.\",\"Email\":\"demoadmin@microsoft.com\",\"Expiration\":\"12/20\",\"LastName\":\"DemoLastName\",\"Name\":\"DemoAdmin\",\"PhoneNumber\":\"1234567890\",\"UserName\":\"demoadmin@microsoft.com\",\"ZipCode\":\"98052\",\"State\":\"WA\",\"Street\":\"15703 NE 61st Ct\",\"SecurityNumber\":\"535\",\"PasswordHash\":\"Pass@word1\"}],\"UserRoles\":[{\"Email\":\"demoadmin@microsoft.com\",\"Role\":\"admin\"},{\"Email\":\"demouser@microsoft.com\",\"Role\":\"user\"}]}";

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


		public async Task EnsureSeedData()
		{
			_logger.LogInformation("Seeding database...");

			ApplicationUser alice = await _userManager.FindByNameAsync("alice");
			if (alice == null)
			{
				alice = new ApplicationUser
				{
					UserName = "alice"
				};
				IdentityResult result = await _userManager.CreateAsync(alice, "Pass123$");
				if (!result.Succeeded)
				{
					throw new Exception(result.Errors.First().Description);
				}

				result = await _userManager.AddClaimsAsync(alice, new Claim[]{
							new Claim(JwtClaimTypes.Name, "Alice Smith"),
							new Claim(JwtClaimTypes.GivenName, "Alice"),
							new Claim(JwtClaimTypes.FamilyName, "Smith"),
							new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
							new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
							new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
							new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json)
						});
				if (!result.Succeeded)
				{
					throw new Exception(result.Errors.First().Description);
				}
				_logger.LogInformation("alice created");
			}
			else
			{
				_logger.LogInformation("alice already exists");
			}

			ApplicationUser bob = _userManager.FindByNameAsync("bob").Result;
			if (bob == null)
			{
				bob = new ApplicationUser
				{
					UserName = "bob",
					EmailConfirmed = true,
				};
				IdentityResult result = await _userManager.CreateAsync(bob, "Pass123$");
				if (!result.Succeeded)
				{
					throw new Exception(result.Errors.First().Description);
				}

				result = await _userManager.AddClaimsAsync(bob, new Claim[]{
						new Claim(JwtClaimTypes.Name, "Bob Smith"),
						new Claim(JwtClaimTypes.GivenName, "Bob"),
						new Claim(JwtClaimTypes.FamilyName, "Smith"),
						new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
						new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
						new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
						new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServer4.IdentityServerConstants.ClaimValueTypes.Json),
						new Claim("location", "somewhere")
					});
				if (!result.Succeeded)
				{
					throw new Exception(result.Errors.First().Description);
				}
				_logger.LogInformation("bob created");
			}
			else
			{
				_logger.LogInformation("bob already exists");
			}



			_logger.LogInformation("Done seeding database.");
		}

	}
}
