﻿using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using System.Linq;
using M1nforum.Web.Infrastructure;
using M1nforum.Web.Services.Entities;
using M1nforum.Web.Templates;
using System.Collections.Generic;
using M1nforum.Web.Infrastructure.Exceptions;

namespace M1nforum.Web.Handlers
{
	public class Categories
	{
		public async Task Browse(HttpContext httpContext)
		{
			// model
			var domain = Program.Cache.Business.GetDomainFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.GetUserByClaims(domain.Id, httpContext.User);
			var categories = Program.Cache.Business.GetCategoriesByDomainId(domain.Id) ?? new List<Category>();
			var isAdmin = user?.IsAdmin == true && user?.IsBanned == false;

			// cache - do not cache if debugging is enabled - caching is difficult.  The data may not have changed but the css changed and this would cache that bad css. It is a trade off and inperfect.
			if (!Program.Cache.DebuggingEnabled && httpContext.CacheContent(categories.Max(c => c.UpdatedOn)))
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
					Header = domain.Title,
					IsAdmin = isAdmin, 
					SiteName = domain.Title,
					Subheader = domain.Description,
					Title = "Categories - " + domain.Title,
					User = user
				});

				// todo:  dont show archived?
				await body.WriteCategories(new
				{
					Categories = categories, 
					IsAdmin = isAdmin, 
					User = user,
				});

				await body.WriteDocumentFooter(new
				{
					domain.Title,
					GeneratedOn = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
				});
			}
		}

		public async Task Add(HttpContext httpContext)
		{

		}

		public async Task Add(HttpContext httpContext, Category category)
		{

		}
	}
}
