using IdentityModel.Client;
using IdentityServer;
using IdentityServer.Data;
using IdentityServer4.EntityFramework.DbContexts;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace XUnitTest.Identity
{
	public class IdentityTestServer : TestServer
	{
		public static IdentityTestServer CreateServer()
		{
			var webHostBuilder = WebHost.CreateDefaultBuilder();
			webHostBuilder.UseContentRoot(Directory.GetCurrentDirectory() + "\\Identity");
			webHostBuilder.UseStartup<Startup>();

			var testServer = new IdentityTestServer(webHostBuilder);

			testServer.Host
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
				});
			return testServer;
		}

		public IdentityTestServer(IWebHostBuilder builder)
			: base(builder) { }

		public IdentityTestServer(IWebHostBuilder builder, IFeatureCollection featureCollection)
			: base(builder, featureCollection) { }

		public async Task<string> GetTokenAsync(string username, string password, string clientId, string clientSecret)
		{
			HttpClient idClient = this.CreateClient();
			var formContent = new FormUrlEncodedContent(new[]
			{
				new KeyValuePair<string, string>("grant_type", "password"),
				new KeyValuePair<string, string>("username", username),
				new KeyValuePair<string, string>("password", password),
				new KeyValuePair<string, string>("client_id", clientId),
				new KeyValuePair<string, string>("client_secret", clientSecret)
			});

			var response = await idClient.PostAsync("connect/token", formContent);
			var stringContent = await response.Content.ReadAsStringAsync();
			var temp = JObject.Parse(stringContent);
			return (string)temp.Property("access_token");
		}

		public async Task<DiscoveryResponse> GetDiscoveryAsync()
		{
			DiscoveryClient dc = new DiscoveryClient("http://localhost", this.CreateHandler());
			DiscoveryResponse disco = await dc.GetAsync();
			dc.Dispose();
			return disco;
		}

		public async Task<TokenResponse> RequestClientCredentialsAsync(string clientId, string clientSecret, string scope)
		{
			DiscoveryResponse disco = await GetDiscoveryAsync();
			TokenClient tokenClient = new TokenClient(disco.TokenEndpoint, clientId, clientSecret, this.CreateHandler());
			TokenResponse tokenResponse = await tokenClient.RequestClientCredentialsAsync(scope);
			return tokenResponse;
		}

		public async Task<TokenResponse> RequestResourceOwnerPasswordAsync(string clientId, string clientSecret, string userName, string passWord, string scope)
		{
			DiscoveryResponse disco = await GetDiscoveryAsync();
			TokenClient tokenClient = new TokenClient(disco.TokenEndpoint, clientId, clientSecret, this.CreateHandler());
			TokenResponse tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(userName, passWord, scope);
			return tokenResponse;
		}
	}

}
