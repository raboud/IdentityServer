// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Api.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class IdentityController : ControllerBase
    {
		private readonly ILogger<IdentityController> _logger;
		public IdentityController(ILogger<IdentityController> logger)
		{
			this._logger = logger;

		}

		[HttpGet]
        public IActionResult Get()
        {
			this._logger.LogWarning("IdentityController - Get");
			return new JsonResult(from c in User.Claims select new { c.Type, c.Value });
        }
    }
}