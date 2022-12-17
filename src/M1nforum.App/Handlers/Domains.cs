using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Linq;
using M1nforum.Web.Infrastructure;
using M1nforum.Web.Services.Entities;
using M1nforum.Web.Templates;
using M1nforum.Web.Infrastructure.Validation;
using M1nforum.Web.Infrastructure.Exceptions;
using System.Text.Json;
using System.Collections.Generic;

namespace M1nforum.Web.Handlers
{
	public class Domains
	{
		public async Task Browse(HttpContext httpContext)
		{
			// model
			var domain = Program.Cache.Business.GetDomainFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.GetUserByClaims(domain.Id, httpContext.User);
			var isAdmin = user?.IsAdmin == true && user?.IsBanned == false;

			// todo:  permissions
			if (!isAdmin)
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
					CSSFilename = Program.Cache.DebuggingEnabled ?
						"app.css?v=" + "wwwroot/css/app.css".GetCSSFileTimestamp() :
						"app.min.css?v=" + "wwwroot/css/app.min.css".GetCSSFileTimestamp(),
					FlashMessage = httpContext.ReadFlashMessage(), 
					Header = "Domain Admin",
					IsAdmin = isAdmin, 
					SiteName = "Domain Admin", // todo
					Subheader = "Domain Admin",
					Title = "Domains - Domain Admin",
					User = user
				});

				// todo:  dont show archived?
				await body.WriteDomains(new
				{
					User = user,
					Domains = domains, 
					IsAdmin = isAdmin
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
			var domain = Program.Cache.Business.GetDomainFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.GetUserByClaims(domain.Id, httpContext.User);
			var isAdmin = user?.IsAdmin == true && user?.IsBanned == false;

			if (!isAdmin)
			{
				// todo:  401
				throw new UnauthorizedAccessException(); // todo:  this is for IO, not for web
			}

			var model = httpContext.ReadAndDeleteCookie("model");
			var newDomain = string.IsNullOrEmpty(model) ?
				new Domain() { Name = "a", Title = "a", Description = "a" } : // empty domain // todo:  temp
				model.Deserialize<Domain>();
			var validationExceptions = httpContext.ReadAndDeleteCookie("validation").Deserialize<List<ValidationException>>() ?? new List<ValidationException>();

			// response
			await using (var body = await httpContext.StartHtmlResponse())
			{
				await body.WriteDocumentHeader(new
				{
					CSSFilename = Program.Cache.DebuggingEnabled ?
						"app.css?v=" + "wwwroot/css/app.css".GetCSSFileTimestamp() :
						"app.min.css?v=" + "wwwroot/css/app.min.css".GetCSSFileTimestamp(),
					FlashMessage = httpContext.ReadFlashMessage(), 
					Header = "Add Domain",
					IsAdmin = isAdmin, 
					SiteName = "Domain Admin", // todo
					Subheader = "Domain adding happens here",
					Title = "Add Domain - Domain Admin",
					User = user
				});

				// todo:  dont show archived?
				await body.WriteDomain(new
				{
					Action = "add",
					ActionTitle = "Add",
					CSRF = "", // todo
					Domain = newDomain,
					User = user,
					ValidationExceptions = validationExceptions
				});

				await body.WriteDocumentFooter(new
				{
					GeneratedOn = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"), 
					Title = "Domain Admin"
				});
			}
		}

		public async Task AddPost(HttpContext httpContext)
		{
			// model
			var domain = Program.Cache.Business.GetDomainFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var form = httpContext.Request.Form;
			var user = Program.Cache.Business.GetUserByClaims(domain.Id, httpContext.User);
			var isAdmin = user?.IsAdmin == true && user?.IsBanned == false;

			if (!isAdmin)
			{
				// todo:  401
				throw new UnauthorizedAccessException(); // todo:  this is for IO, not for web
			}

			var newDomain = new Domain()
			{
				Id = IdGenerator.NewId(),
				Name = form["Name"],
				Title = form["Title"],
				Description = form["Description"],
			};

			// todo:  validation

			var exceptions = new Validation() // todo:  these validations should be in the bll also.
				.IsNotNull(newDomain.Title, nameof(newDomain.Title))
				.IsLengthInRange(newDomain.Title, nameof(newDomain.Title), 1, 64)
				.IsNotNull(newDomain.Name, nameof(newDomain.Name))
				.IsLengthInRange(newDomain.Name, nameof(newDomain.Name), 5, 64)
				.IsNotNull(newDomain.Description, nameof(newDomain.Description))
				.IsLengthInRange(newDomain.Description, nameof(newDomain.Description), 0, 128)
				.GetValidationExceptions();

			if (exceptions.Count == 0)
			{
				Program.Cache.Business.InsertDomain(user, newDomain);

				await httpContext.WriteFlashMessage("success", "Domain added.");
				httpContext.Response.Redirect("/domains", false);
				return;
			}
			else
			{
				await httpContext.WriteCookie("flash_message", "error-Domain add failed.  Correct the errors below and try again.");
				await httpContext.WriteCookie("validation", JsonSerializer.Serialize(exceptions));
				await httpContext.WriteCookie("model", JsonSerializer.Serialize(newDomain));
				httpContext.Response.Redirect("/domains/add", false);
				return;
			}
		}
	}
}
