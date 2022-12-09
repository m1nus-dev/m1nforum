using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace M1nforum.Web.Features.Home
{
	public class Browse
	{
		public class ViewModel
		{
		}

		public async Task Get(HttpContext httpContext)
		{
			httpContext.Response.Redirect("/categories", false);
			return;
		}
	}
}
