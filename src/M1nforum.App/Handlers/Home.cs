using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace M1nforum.Web.Handlers
{
    public class Home
    {
        public async Task Get(HttpContext httpContext)
        {
            httpContext.Response.Redirect("/categories", false);
            return;
        }
    }
}
