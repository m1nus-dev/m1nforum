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
			// context
			var domain = Program.Cache.Business.DomainGetFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.UserGetByClaims(domain.Id, httpContext.User).AsAdmin() ?? throw new AccessViolationException(); // todo:  this is for IO, not for web.  Throw 401 here.
			
			// data
			var domains = Program.Cache.Business.DomainsList(user);

			// cache - do not cache if debugging is enabled - caching is difficult.  The data may not have changed but the css changed and this would cache that bad css. It is a trade off and inperfect.
			if (!Program.Cache.DebuggingEnabled && httpContext.CacheContent(domains.Max(d => d.UpdatedOn)))
			{
				return;
			}

			// response
			await RenderView(httpContext, "List", user, "", false, null, domains);
		}

		public async Task Read(HttpContext httpContext, ulong domainId)
		{
			// context
			var domain = Program.Cache.Business.DomainGetFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.UserGetByClaims(domain.Id, httpContext.User).AsAdmin() ?? throw new AccessViolationException(); // todo:  this is for IO, not for web.  Throw 401 here.

			// data
			var readDomain = Program.Cache.Business.DomainGetById(user, domainId);

			// response
			await RenderView(httpContext, "Read", user, "", true, readDomain);
		}

		public async Task Edit(HttpContext httpContext, ulong domainId)
		{
			// context
			var domain = Program.Cache.Business.DomainGetFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.UserGetByClaims(domain.Id, httpContext.User).AsAdmin() ?? throw new AccessViolationException(); // todo:  this is for IO, not for web.  Throw 401 here.

			// data
			var serializedDomain = httpContext.ReadAndDeleteCookie("serializedDomain");
			var editDomain = string.IsNullOrEmpty(serializedDomain) ?
				Program.Cache.Business.DomainGetById(user, domainId) :
				serializedDomain.Deserialize<Domain>();
			var validationExceptions = httpContext.ReadAndDeleteCookie("validation").Deserialize<List<ValidationException>>() ?? new List<ValidationException>();

			// response
			await RenderView(httpContext, "Edit", user, "/domains/edit/" + editDomain.Id.ToString(), false, editDomain, null, validationExceptions);
		}

		public async Task EditPost(HttpContext httpContext, ulong domainId)
		{
			// context
			var domain = Program.Cache.Business.DomainGetFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.UserGetByClaims(domain.Id, httpContext.User).AsAdmin() ?? throw new AccessViolationException(); // todo:  this is for IO, not for web.  Throw 401 here.

			// data
			var editDomain = GetDomainFromForm(httpContext.Request.Form);

			var validationExceptions = Program.Cache.Business.DomainGetValidationExceptions(domain);

			if (validationExceptions.Count == 0)
			{
				Program.Cache.Business.DomainUpdate(user, editDomain);

				await httpContext.WriteFlashMessage("success", "Domain updated.");
				httpContext.Response.Redirect("/domains", false);
				return;
			}
			else
			{
				await httpContext.WriteCookie("flash_message", "error-Domain edit failed.  Correct the errors below and try again.");
				await httpContext.WriteCookie("validation", JsonSerializer.Serialize(validationExceptions));
				await httpContext.WriteCookie("serializedDomain", JsonSerializer.Serialize(editDomain));
				httpContext.Response.Redirect("/domains/edit/" + domainId.ToString(), false);
				return;
			}
		}

		public async Task Add(HttpContext httpContext)
		{
			// context
			var domain = Program.Cache.Business.DomainGetFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.UserGetByClaims(domain.Id, httpContext.User).AsAdmin() ?? throw new AccessViolationException(); // todo:  this is for IO, not for web.  Throw 401 here.

			// data
			var serializedDomain = httpContext.ReadAndDeleteCookie("serializedDomain");
			var addDomain = string.IsNullOrEmpty(serializedDomain) ?
				new Domain() { Name = string.Empty, Title = string.Empty, Description = string.Empty } : // empty domain
				serializedDomain.Deserialize<Domain>();
			var validationExceptions = httpContext.ReadAndDeleteCookie("validation").Deserialize<List<ValidationException>>() ?? new List<ValidationException>();

			// response
			await RenderView(httpContext, "Add", user, "/domains/add", false, addDomain, null, validationExceptions);
		}

		public async Task AddPost(HttpContext httpContext)
		{
			// context
			var domain = Program.Cache.Business.DomainGetFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.UserGetByClaims(domain.Id, httpContext.User).AsAdmin() ?? throw new AccessViolationException(); // todo:  this is for IO, not for web.  Throw 401 here.

			// data 
			var addDomain = GetDomainFromForm(httpContext.Request.Form);
			addDomain.Id = IdGenerator.NewId();

			var validationExceptions = Program.Cache.Business.DomainGetValidationExceptions(domain);

			if (validationExceptions.Count == 0)
			{
				Program.Cache.Business.DomainInsert(user, addDomain);

				await httpContext.WriteFlashMessage("success", "Domain added.");
				httpContext.Response.Redirect("/domains", false);
				return;
			}
			else
			{
				await httpContext.WriteCookie("flash_message", "error-Domain add failed.  Correct the errors below and try again.");
				await httpContext.WriteCookie("validation", JsonSerializer.Serialize(validationExceptions));
				await httpContext.WriteCookie("serializedDomain", JsonSerializer.Serialize(addDomain));
				httpContext.Response.Redirect("/domains/add", false);
				return;
			}
		}

		public async Task Delete(HttpContext httpContext, ulong domainId)
		{
			// context
			var domain = Program.Cache.Business.DomainGetFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.UserGetByClaims(domain.Id, httpContext.User).AsAdmin() ?? throw new AccessViolationException(); // todo:  this is for IO, not for web.  Throw 401 here.
			
			// data
			var deleteDomain = Program.Cache.Business.DomainGetById(user, domainId);

			// response
			await RenderView(httpContext, "Delete", user, "/domains/delete/" + deleteDomain.Id.ToString(), true, deleteDomain);
		}

		public async Task DeletePost(HttpContext httpContext, ulong domainId)
		{
			// context
			var domain = Program.Cache.Business.DomainGetFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.UserGetByClaims(domain.Id, httpContext.User).AsAdmin() ?? throw new AccessViolationException(); // todo:  this is for IO, not for web.  Throw 401 here.

			// data
			var deleteDomain = GetDomainFromForm(httpContext.Request.Form);

			Program.Cache.Business.DomainDelete(user, deleteDomain);

			await httpContext.WriteFlashMessage("success", "Domain deleted.");
			httpContext.Response.Redirect("/domains", false);
		}

		private Domain GetDomainFromForm(IFormCollection form)
		{
			var domain = new Domain()
			{
				Id = (string)form["Id"] == null ? 0 : ulong.Parse(form["Id"]),  
				Name = form["Name"],
				Title = form["Title"],
				Description = form["Description"],
			};

			return domain;
		}

		public async Task RenderView(HttpContext httpContext, string methodName, User user, string action, bool isReadOnly, Domain domain = null, List<Domain> domains = null, List<ValidationException> validationExceptions = null)
		{
			var siteName = "Domain Admin";
			var pageHeader = "Domain " + methodName;
			var pageSubheader = "Domain Administration";
			var pageTitle = pageHeader + " - " + siteName;
			var actionButton = pageHeader;

			await using (var body = await httpContext.StartHtmlResponse())
			{
				await body.WriteDocumentHeader(new
				{
					GetCSSFileUrl = Program.Cache.GetCSSFileUrl(),
					FlashMessage = httpContext.ReadFlashMessage(),
					PageHeader = pageHeader,
					IsAdmin = Program.Cache.Business.UserIsAdmin(user),
					SiteName = siteName,
					PageSubheader = pageSubheader,
					PageTitle = pageTitle,
					User = user
				});

				if (domain != null)
				{
					await body.WriteDomain(new
					{
						Action = action,
						IsReadonly = isReadOnly,
						ActionButton = actionButton,
						Domain = domain,
						ValidationExceptions = validationExceptions ?? new List<ValidationException>()
					});
				}
				else
				{
					// todo:  dont show archived?
					await body.WriteDomains(new
					{
						User = user,
						Domains = domains,
						IsAdmin = Program.Cache.Business.UserIsAdmin(user)
					});
				}

				await body.WriteDocumentFooter(new
				{
					PageTitle = siteName
				});
			}
		}
	}
}
