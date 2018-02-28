using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IdentityServer.Data;
using IdentityServer.Models;
using System.Reflection;
using Microsoft.IdentityModel.Tokens;
using IdentityServer.Services;
using IdentityServer4.Services;
using Autofac;
using Autofac.Extensions.DependencyInjection;

namespace IdentityServer
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public IHostingEnvironment Environment { get; }

        public Startup(IConfiguration configuration, IHostingEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

		public IServiceProvider ConfigureServices(IServiceCollection services)
		{
            string connectionString = Configuration["ConnectionString"];
			string migrationsAssembly = typeof(Startup).GetTypeInfo().Assembly.GetName().Name;

            services.AddDbContext<ApplicationDbContext>(options =>
			 options.UseSqlServer(Configuration["ConnectionString"],
									 sqlServerOptionsAction: sqlOptions =>
									 {
										 sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
										 //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
										 sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
									 }));
			//options.UseSqlite(connectionString, sqliteOptionsAction: sqlOptions =>
			//	{
			//		sqlOptions.MigrationsAssembly(typeof(Startup).GetTypeInfo().Assembly.GetName().Name);
			//	}));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

			services.Configure<AppSettings>(Configuration);
			services.Configure<IISOptions>(iis =>
			{
				iis.AuthenticationDisplayName = "Windows";
				iis.AutomaticAuthentication = false;
			});

			services.AddMvc();

			IIdentityServerBuilder b = services.AddIdentityServer()
				.AddAspNetIdentity<ApplicationUser>()
				// this adds the config data from DB (clients, resources)
				.AddConfigurationStore(options =>
				{
					options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
									 sqlServerOptionsAction: sqlOptions =>
									 {
										 sqlOptions.MigrationsAssembly(migrationsAssembly);
										 //Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
										 sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
									 });
				})
				.AddOperationalStore(options =>
				{
					options.ConfigureDbContext = builder => builder.UseSqlServer(connectionString,
									sqlServerOptionsAction: sqlOptions =>
									{
										sqlOptions.MigrationsAssembly(migrationsAssembly);
										//Configuring Connection Resiliency: https://docs.microsoft.com/en-us/ef/core/miscellaneous/connection-resiliency 
										sqlOptions.EnableRetryOnFailure(maxRetryCount: 15, maxRetryDelay: TimeSpan.FromSeconds(30), errorNumbersToAdd: null);
									});
					//					 options.EnableTokenCleanup = true;
					//					 options.TokenCleanupInterval = 30;
				});
				b.Services.AddTransient<IProfileService, ProfileService>();

			if (Environment.IsDevelopment())
            {
                b.AddDeveloperSigningCredential();
            }
            else
            {
				b.AddDeveloperSigningCredential();
				//                throw new Exception("need to configure key material");
			}

			services.AddSingleton<ApplicationDbContextSeed, ApplicationDbContextSeed>();
			services.AddSingleton<ConfigurationDbContextSeed, ConfigurationDbContextSeed>();

			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy",
					builder => builder.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowCredentials());
			});

			services.AddAuthentication()
                .AddGoogle(options =>
                {
                    options.ClientId = "708996912208-9m4dkjb5hscn7cjrn5u0r4tbgkbj1fko.apps.googleusercontent.com";
                    options.ClientSecret = "wdfPY6t8H8cecgjlxud__4Gh";
                })
                .AddOpenIdConnect("oidc", "OpenID Connect", options =>
                {
                    options.Authority = "https://demo.identityserver.io/";
                    options.ClientId = "implicit";
                    options.SaveTokens = true;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });


			ContainerBuilder container = new ContainerBuilder();
			container.Populate(services);

			return new AutofacServiceProvider(container.Build());
		}

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

			app.UseCors("CorsPolicy");
			app.UseStaticFiles();
            app.UseIdentityServer();
            app.UseMvcWithDefaultRoute();
        }
    }
}
