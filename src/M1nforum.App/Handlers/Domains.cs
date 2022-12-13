using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Linq;
using M1nforum.Web.Infrastructure;
using M1nforum.Web.Services.Entities;
using M1nforum.Web.Templates;
using System.Collections.Generic;

namespace M1nforum.Web.Handlers
{
	public class Domains
	{
		public async Task Browse(HttpContext httpContext)
		{
			// model
			var user = Program.Cache.Business.GetuserByClaims(httpContext.User);

			// todo:  permissions
			if (user?.IsAdmin == true && !user?.IsBanned == false)
			{
				// todo:  401
				await httpContext.Response.WriteAsJsonAsync("Not Authorized");
			}

			var domains = Program.Cache.Business.GetDomains(user);

			// cache - do not cache if debugging is enabled - caching is difficult.  The data may not have changed but the css changed and this would cache that bad css. It is a trade off and inperfect.
			if (!Program.Cache.DebuggingEnabled && httpContext.CacheContent(domains.Max(d => d.UpdatedOn)))
			{
				return;
			}

			// response
			await using (var body = await httpContext.StartHtmlResponse())
			{
				await body.WriteDocumentHeader(new
				{
					User = user,
					SiteName = "Domain Admin", // todo
					Title = "Domains - Domain Admin",
					CSSFilename = Program.Cache.DebuggingEnabled ?
						"app.css?v=" + "wwwroot/css/app.css".GetCSSFileTimestamp() :
						"app.min.css?v=" + "wwwroot/css/app.min.css".GetCSSFileTimestamp(), 
					Header = "Domain Admin",
					Subheader = "Domain Admin",
					FlashMessage = httpContext.ReadFlashMessage() 
				});

				// todo:  dont show archived?
				await body.WriteDomains(new
				{
					User = user,
					Domains = domains
				});

				await body.WriteDocumentFooter(new
				{
					Title = "Domain Admin",
					GeneratedOn = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
				});
			}
		}

		public async Task Add(HttpContext httpContext)
		{
			// model
			var user = Program.Cache.Business.GetuserByClaims(httpContext.User);

			// todo:  permissions
			if (user?.IsAdmin == true && !user?.IsBanned == false)
			{
				// todo:  401
				await httpContext.Response.WriteAsJsonAsync("Not Authorized");
			}

			// response
			await using (var body = await httpContext.StartHtmlResponse())
			{
				await body.WriteDocumentHeader(new
				{
					User = user,
					SiteName = "Domain Admin", // todo
					Title = "Add Domain - Domain Admin",
					CSSFilename = Program.Cache.DebuggingEnabled ?
						"app.css?v=" + "wwwroot/css/app.css".GetCSSFileTimestamp() :
						"app.min.css?v=" + "wwwroot/css/app.min.css".GetCSSFileTimestamp(),
					Header = "Add Domain",
					Subheader = "Domain adding happens here",
					FlashMessage = httpContext.ReadFlashMessage()
				});

				// todo:  dont show archived?
				await body.WriteDomain(new
				{
					CSRF = "",

					Action = "add",
					ActionTitle = "Add",
					User = user,
					Domain = new Domain() { Name = "", Title = "", Description = "" }
				});

				await body.WriteDocumentFooter(new
				{
					Title = "Domain Admin",
					GeneratedOn = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
				});
			}
		}

		public async Task AddPost(HttpContext httpContext)
		{
			// model
			var form = httpContext.Request.Form;
			var user = Program.Cache.Business.GetuserByClaims(httpContext.User);

			// todo:  permissions
			if (user?.IsAdmin == true && !user?.IsBanned == false)
			{
				// todo:  401
				await httpContext.Response.WriteAsJsonAsync("Not Authorized");
			}

			var domain = new Domain()
			{
				Id = IdGenerator.NewId(),
				Name = form["Name"],
				Title = form["Title"],
				Description = form["Description"],
			};

			// todo:  validation

			Program.Cache.Business.InsertDomain(user, domain);

			httpContext.WriteFlashMessage("success", "Domain added.");
			httpContext.Response.Redirect("/domains", false);
			return;
		}
	}
}
