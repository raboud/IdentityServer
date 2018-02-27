using IdentityModel;
using IdentityServer.Models;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace IdentityServer.Services
{
    public class ProfileService : IProfileService
    {
        private readonly UserManager<ApplicationUser> _userManager;
		private readonly IUserClaimsPrincipalFactory<ApplicationUser> _claimsFactory;

		public ProfileService(IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory, UserManager<ApplicationUser> userManager)
        {
			_claimsFactory = claimsFactory;
            _userManager = userManager;
        }

        async public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
			ClaimsPrincipal subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));

			string subjectId = subject.Claims.Where(x => x.Type == "sub").FirstOrDefault().Value;

			ApplicationUser user = await _userManager.FindByIdAsync(subjectId);
            if (user == null)
			{
				throw new ArgumentException("Invalid subject identifier");
			}

			ClaimsPrincipal principal = await _claimsFactory.CreateAsync(user);
			List<Claim> claims = principal.Claims.ToList();

			claims.AddRange(GetClaimsFromUser(user));
            context.IssuedClaims = claims;
        }

        async public Task IsActiveAsync(IsActiveContext context)
        {
			ClaimsPrincipal subject = context.Subject ?? throw new ArgumentNullException(nameof(context.Subject));

			string subjectId = subject.Claims.Where(x => x.Type == "sub").FirstOrDefault().Value;
			ApplicationUser user = await _userManager.FindByIdAsync(subjectId);

            context.IsActive = false;

            if (user != null)
            {
                if (_userManager.SupportsUserSecurityStamp)
                {
					string security_stamp = subject.Claims.Where(c => c.Type == "security_stamp").Select(c => c.Value).SingleOrDefault();
                    if (security_stamp != null)
                    {
						string db_security_stamp = await _userManager.GetSecurityStampAsync(user);
                        if (db_security_stamp != security_stamp)
						{
							return;
						}
					}
                }

                context.IsActive =
                    !user.LockoutEnabled ||
                    !user.LockoutEnd.HasValue ||
                    user.LockoutEnd <= DateTime.Now;
            }
        }

        private IEnumerable<Claim> GetClaimsFromUser(ApplicationUser user)
        {
			List<Claim> claims = new List<Claim>();
/*
            if (!string.IsNullOrWhiteSpace(user.LastName))
                claims.Add(new Claim("last_name", user.LastName));

            if (!string.IsNullOrWhiteSpace(user.CardNumber))
                claims.Add(new Claim("card_number", user.CardNumber));

            if (!string.IsNullOrWhiteSpace(user.CardHolderName))
                claims.Add(new Claim("card_holder", user.CardHolderName));

            if (!string.IsNullOrWhiteSpace(user.SecurityNumber))
                claims.Add(new Claim("card_security_number", user.SecurityNumber));

            if (!string.IsNullOrWhiteSpace(user.Expiration))
                claims.Add(new Claim("card_expiration", user.Expiration));

            if (!string.IsNullOrWhiteSpace(user.City))
                claims.Add(new Claim("address_city", user.City));

            if (!string.IsNullOrWhiteSpace(user.Country))
                claims.Add(new Claim("address_country", user.Country));

            if (!string.IsNullOrWhiteSpace(user.State))
                claims.Add(new Claim("address_state", user.State));

            if (!string.IsNullOrWhiteSpace(user.Street))
                claims.Add(new Claim("address_street", user.Street));

            if (!string.IsNullOrWhiteSpace(user.ZipCode))
                claims.Add(new Claim("address_zip_code", user.ZipCode));
*/
            return claims;
        }
    }
}
