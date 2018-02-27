// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace JavaScriptClient
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
			services.AddCors(options =>
			{
				options.AddPolicy("CorsPolicy",
					builder => builder.AllowAnyOrigin()
					.AllowAnyMethod()
					.AllowAnyHeader()
					.AllowCredentials());
			});
		}

		public void Configure(IApplicationBuilder app)
        {
			app.UseCors("CorsPolicy");
			app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
