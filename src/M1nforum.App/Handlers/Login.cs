using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using M1nforum.Web.Infrastructure;
using M1nforum.Web.Templates;
using M1nforum.Web.Infrastructure.Exceptions;

namespace M1nforum.Web.Handlers
{
	public class Login
	{
		public async Task Get(HttpContext httpContext)
		{
			// model
			var domain = Program.Cache.Business.GetDomainFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var redirectUrl = (string)httpContext.Request.Query["next"] ?? "/";
			var userIsLoggedIn = false;

			// if there is a current logged in user
			if (userIsLoggedIn)
			{
				httpContext.Response.Redirect(redirectUrl, false);
				return;
			}

			// user is not logged in, show login form

			// response
			await using (var body = await httpContext.StartHtmlResponse())
			{
				await body.WriteDocumentHeader(new
				{
					SiteName = domain.Title,
					Title = "Categories - " + domain.Title,
					CSSFilename = Program.Cache.DebuggingEnabled ?
						"app.css?v=" + "wwwroot/css/app.css".GetCSSFileTimestamp() :
						"app.min.css?v=" + "wwwroot/css/app.min.css".GetCSSFileTimestamp(), 
					Header = domain.Title,
					Subheader = domain.Description, 
					FlashMessage = httpContext.ReadFlashMessage()
				});

				await body.WriteLogin(new
				{
					CSRF = "",
					Next = redirectUrl,
				});

				await body.WriteDocumentFooter(new
				{
					domain.Title,
					GeneratedOn = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
				});
			}
		}

		public async Task Post(HttpContext httpContext)
		{
			// model
			var domain = Program.Cache.Business.GetDomainFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var redirectUrl = (string)httpContext.Request.Query["next"] ?? "/";
			var userIsLoggedIn = false;

			// if there is a current logged in user
			if (userIsLoggedIn)
			{
				httpContext.Response.Redirect(redirectUrl, false);
				return;
			}

			var username = (string)httpContext.Request.Form["username"] ?? string.Empty;
			var password = (string)httpContext.Request.Form["password"] ?? string.Empty;

			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				httpContext.WriteFlashMessage("error", "Username and password required.");
				httpContext.Response.Redirect("login?next=" + redirectUrl, false);
				return;
			}

			if (username.Length > 200 || password.Length > 200)
			{
				httpContext.WriteFlashMessage("error", "Username or password too long.");
				httpContext.Response.Redirect("login?next=" + redirectUrl, false);
				return;
			}

			// todo:  csrf

			// todo:  login
			var user = Program.Cache.Business.Login(username, password);

			// temp
			if (username == "asdf")
			{
				httpContext.WriteFlashMessage("success", "Logged in successfully.");
				httpContext.Response.Redirect("login?next=" + redirectUrl, false);
				return;
			}


			if (user == null)
			{
				httpContext.WriteFlashMessage("error", "Unable to login with those credentials.");
				httpContext.Response.Redirect("login?next=" + redirectUrl, false);
				return;
			}
			else
			{
				httpContext.Response.Redirect(redirectUrl, false);
				return;
			}
		}
	}
}
