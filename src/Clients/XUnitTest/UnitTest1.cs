using System;
using System.Threading.Tasks;
using Xunit;
using XUnitTest.Identity;

namespace XUnitTest
{
    public class UnitTest1
    {
        [Fact]
        public async Task RequestClientCredentials_pass()
        {
			using (IdentityTestServer idServer = IdentityTestServer.CreateServer())
			{
				var token = await idServer.RequestClientCredentialsAsync("client", "secret", "api1");
				Assert.False(token.IsError);
			}
		}

		[Fact]
		public async Task RequestClientCredentials_badClient()
		{
			using (IdentityTestServer idServer = IdentityTestServer.CreateServer())
			{
				var token = await idServer.RequestClientCredentialsAsync("clientbad", "secret", "api1");
				Assert.True(token.IsError);
			}
		}

		[Fact]
		public async Task RequestClientCredentials_badSecret()
		{
			using (IdentityTestServer idServer = IdentityTestServer.CreateServer())
			{
				var token = await idServer.RequestClientCredentialsAsync("client", "secretbad", "api1");
				Assert.True(token.IsError);
			}
		}

		[Fact]
		public async Task RequestClientCredentials_badScope()
		{
			using (IdentityTestServer idServer = IdentityTestServer.CreateServer())
			{
				var token = await idServer.RequestClientCredentialsAsync("client", "secret", "api1bad");
				Assert.True(token.IsError);
			}
		}

		[Fact]
		public async Task RequestResourceOwnerPassword_pass()
		{
			using (IdentityTestServer idServer = IdentityTestServer.CreateServer())
			{
				var token = await idServer.RequestResourceOwnerPasswordAsync("ro.client", "secret", "alice", "P@ssword1234", "api1");
				Assert.False(token.IsError);
			}
		}
	}
}
