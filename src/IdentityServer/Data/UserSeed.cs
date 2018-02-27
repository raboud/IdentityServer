using IdentityServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityServer
{
    public class UserSeed
    {
		public List<string> Roles { get; set; }
		public List<ApplicationUser> Users { get; set; }
		public List<UserRole> UserRoles { get; set; }

		public class UserRole
		{
			public string Email { get; set; }
			public string Role { get; set; }
		}
	}
}
