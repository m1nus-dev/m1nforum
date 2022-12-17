using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using M1nforum.Web.Infrastructure;
using M1nforum.Web.Templates;
using M1nforum.Web.Infrastructure.Exceptions;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Collections.Generic;

namespace M1nforum.Web.Handlers
{
	public class Login
	{
		// note:  its weird to include domain in these requests.  todo:  is this correct?
		public async Task Get(HttpContext httpContext)
		{
			// model
			var domain = Program.Cache.Business.GetDomainFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.GetUserByClaims(domain.Id, httpContext.User);
			var returnUrl = (string)httpContext.Request.Query["ReturnURL"] ?? "/";

			if (user != null)
			{
				httpContext.Response.Redirect(returnUrl, false);
				return;
			}

			// response
			await using (var body = await httpContext.StartHtmlResponse())
			{
				await body.WriteDocumentHeader(new
				{
					User = user,
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
					ReturnURL = returnUrl,
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
			var returnUrl = (string)httpContext.Request.Form["ReturnURL"] ?? "/";
			var userIsLoggedIn = false;

			// if there is a current logged in user
			if (userIsLoggedIn)
			{
				httpContext.Response.Redirect(returnUrl, false);
				return;
			}

			var username = (string)httpContext.Request.Form["username"] ?? string.Empty;
			var password = (string)httpContext.Request.Form["password"] ?? string.Empty;

			if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
			{
				await httpContext.WriteFlashMessage("error", "Username and password required.");
				httpContext.Response.Redirect("login?ReturnURL=" + returnUrl, false);
				return;
			}

			if (username.Length > 200 || password.Length > 200)
			{
				await httpContext.WriteFlashMessage("error", "Username or password too long.");
				httpContext.Response.Redirect("login?ReturnURL=" + returnUrl, false);
				return;
			}

			// todo:  csrf

			var user = Program.Cache.Business.Login(domain.Id, username, password);

			if (user == null)
			{
				await httpContext.WriteFlashMessage("error", "Unable to login with those credentials.");
				httpContext.Response.Redirect("login?ReturnURL=" + returnUrl, false);
				return;
			}
			else
			{
				var claims = new List<Claim>
				{
					new Claim(ClaimTypes.Name, username),
					new Claim("IsAdmin", user.IsAdmin.ToString()), 
					new Claim("Id", user.Id.ToString())
				};

				var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
				var authProperties = new AuthenticationProperties();
				await httpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity), authProperties);

				httpContext.Response.Redirect(returnUrl, false);
				return;
			}
		}
	}
}
