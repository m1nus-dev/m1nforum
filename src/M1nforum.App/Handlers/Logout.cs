using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using M1nforum.Web.Infrastructure;
using Microsoft.AspNetCore.Authentication;

namespace M1nforum.Web.Handlers
{
	public class Logout
	{
		public async Task Get(HttpContext httpContext)
		{
			var authProperties = new AuthenticationProperties();
			await httpContext.SignOutAsync(authProperties);

			await httpContext.WriteFlashMessage("info", "You have been logged out.");
			httpContext.Response.Redirect("/", false);
			return;
		}
	}
}
