// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using IdentityServer.Attributes;
using IdentityServer.Models.Grant;

namespace IdentityServer.Controllers
{
    /// <summary>
    /// This sample controller allows a user to revoke grants given to clients
    /// </summary>
    [SecurityHeaders]
    [Authorize]
    public class GrantsController : Controller
    {
        private readonly IIdentityServerInteractionService _interaction;
        private readonly IClientStore _clients;
        private readonly IResourceStore _resources;

        public GrantsController(IIdentityServerInteractionService interaction,
            IClientStore clients,
            IResourceStore resources)
        {
            _interaction = interaction;
            _clients = clients;
            _resources = resources;
        }

        /// <summary>
        /// Show list of grants
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View("Index", await BuildViewModelAsync());
        }

        /// <summary>
        /// Handle postback to revoke a client
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Revoke(string clientId)
        {
            await _interaction.RevokeUserConsentAsync(clientId);
            return RedirectToAction("Index");
        }

        private async Task<GrantsViewModel> BuildViewModelAsync()
        {
			IEnumerable<IdentityServer4.Models.Consent> grants = await _interaction.GetAllUserConsentsAsync();

			List<GrantViewModel> list = new List<GrantViewModel>();
			foreach (IdentityServer4.Models.Consent grant in grants)
			{
				await NewMethod(list, grant);
			}

			return new GrantsViewModel
            {
                Grants = list
            };
        }

		private async Task NewMethod(List<GrantViewModel> list, IdentityServer4.Models.Consent grant)
		{
			IdentityServer4.Models.Client client = await _clients.FindClientByIdAsync(grant.ClientId);
			if (client != null)
			{
				IdentityServer4.Models.Resources resources = await _resources.FindResourcesByScopeAsync(grant.Scopes);

				GrantViewModel item = new GrantViewModel()
				{
					ClientId = client.ClientId,
					ClientName = client.ClientName ?? client.ClientId,
					ClientLogoUrl = client.LogoUri,
					ClientUrl = client.ClientUri,
					Created = grant.CreationTime,
					Expires = grant.Expiration,
					IdentityGrantNames = resources.IdentityResources.Select(x => x.DisplayName ?? x.Name).ToArray(),
					ApiGrantNames = resources.ApiResources.Select(x => x.DisplayName ?? x.Name).ToArray()
				};

				list.Add(item);
			}
		}
	}
}