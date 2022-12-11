using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Linq;
using M1nforum.Web.Infrastructure;
using M1nforum.Web.Services.Entities;
using M1nforum.Web.Templates;

namespace M1nforum.Web.Handlers
{
	public class Categories
	{
		public async Task Get(HttpContext httpContext)
		{
			// context
			var domain = httpContext.Get<Domain>("Domain");
			// user, _ := r.Context().Value(CtxUserKey).(*models.User)

			// model
			var categories = Program.Cache.Business.GetCategoriesByDomainId(domain.Id);

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
					SiteName = domain.Title,
					Title = "Categories - " + domain.Title,
					CSSFilename = Program.Cache.DebuggingEnabled ?
						"app.css?v=" + "wwwroot/css/app.css".GetCSSFileTimestamp() :
						"app.min.css?v=" + "wwwroot/css/app.min.css".GetCSSFileTimestamp()
				});

				await body.WritePageHeader(new
				{
					Title = "Categories - " + domain.Title,
					Header = domain.Title,
					Subheader = domain.Description
				});

				await body.WriteCategoriesHeader();

				// todo:  dont show archived?

				foreach (var category in categories)
				{
					await body.WriteCategoriesRow(new
					{
						Category = category
					});
				}

				await body.WriteCategoriesFooter();

				await body.WriteDocumentFooter(new
				{
					domain.Title,
					GeneratedOn = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
				});
			}


			//await using (var body = await httpContext.StartHtmlResponse())
			//{
			//	await body.WriteDocumentHeader(new
			//	{
			//		SiteName = domain.Title,
			//		Title = "Categories - " + domain.Title,
			//		CSSFilename = Program.Cache.DebuggingEnabled ?
			//			"app.css?v=" + "wwwroot/css/app.css".GetCSSFileTimestamp() :
			//			"app.min.css?v=" + "wwwroot/css/app.min.css".GetCSSFileTimestamp()
			//	});

			//	await body.WritePageHeader(new
			//	{
			//		Title = "Categories - " + domain.Title,
			//		Header = domain.Title,
			//		Subheader = domain.Description
			//	});

			//	await body.WriteCategoriesHeader();

			//	// todo:  dont show archived?

			//	foreach (var category in categories)
			//	{
			//		await body.WriteCategoriesRow(new
			//		{
			//			Category = category
			//		});
			//	}

			//	await body.WriteCategoriesFooter();

			//	await body.WriteDocumentFooter(new
			//	{
			//		domain.Title,
			//		GeneratedOn = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
			//	});
			//}
		}
	}
}
