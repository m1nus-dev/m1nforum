using M1nforum.Web.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using M1nforum.Web.Services.Entities;

namespace M1nforum.Web.Features.Categories
{
	public class Browse
	{
		public class ViewModel
		{
			// todo
			public string CSRF { get; set; }
			public string PasePathField { get; set; }
			public string User { get; set; }



			public string SiteName { get; set; }
			public string Title { get; set; }
			public string Header { get; set; }
			public string Subheader { get; set; }

			public List<Category> Categories { get; set; }
		}

		public async Task Get(HttpContext httpContext)
		{
			// context
			var domain = httpContext.Get<Domain>("Domain");
			// user, _ := r.Context().Value(CtxUserKey).(*models.User)

			// data
			var categories = Program.Cache.Business.GetCategoriesByDomainId(domain.Id);

			// todo:  check for nulls

			//
			// caching is difficult.  The data may not have changed but the css changed and this would cache that bad css.
			// it is a trade off and inperfect.
			//
			// cache - do not cache if debugging is enabled
			if (!Program.Cache.DebuggingEnabled && httpContext.CacheContent(categories.Max(c => c.UpdatedOn)))
			{
				return;
			}

			// respond
			var viewModel = new ViewModel(); // todo:  site info
			viewModel.SiteName = domain.Title;
			viewModel.Title = "Categories - " + domain.Title;
			viewModel.Header = domain.Title;
			viewModel.Subheader = domain.Description;
			viewModel.Categories = categories;

			await Render(httpContext.Response.Body, viewModel);
		}

		public async Task Render(Stream stream, ViewModel viewModel)
		{
			using (var sw = new StreamWriter(stream))
			{
				var variables = new Dictionary<string, string>();
				var template = new TemplateEngine();
				template.ParseFile("template", "Templates\\default.html");

				variables.Add("SiteName", viewModel.SiteName);
				variables.Add("Title", viewModel.Title);
				if (Program.Cache.DebuggingEnabled)
				{
					variables.Add("CSSFilename", "app.css?v=" + "wwwroot/css/app.css".GetFileTimestamp());
				}
				else
				{
					variables.Add("CSSFilename", "app.min.css?v=" + "wwwroot/css/app.min.css".GetFileTimestamp());
				}
				await template.Render(sw, "document_header", variables);

				variables.Clear();
				variables.Add("Title", viewModel.Title);
				variables.Add("Header", viewModel.Header);
				variables.Add("Subheader", viewModel.Subheader);
				await template.Render(sw, "page_header", variables);

				variables.Clear();
				await template.Render(sw, "categories_header", variables);

				// todo:  dont show archived?

				variables.Clear();
				await template.RenderLoop(sw, "categories_row", viewModel.Categories, (category) =>
				{
					return new Dictionary<string, string>()
					{
						{ "Id", category.Id.ToString() },
						{ "CreatedBy", category.CreatedBy },
						{ "CreatedOn", category.CreatedOn.ToString("MM/dd/YYYY") },
						{ "UpdatedBy", category.UpdatedBy },
						{ "UpdatedOn", category.UpdatedOn.ToString("MM/dd/YYYY") },
						{ "ArchivedAt", category.ArchivedAt.ToString() },
						{ "Description", category.Description },
						{ "DomainId", category.DomainId.ToString() },
						{ "HeaderMsg", category.HeaderMessage },
						{ "IsPrivate", category.IsPrivate.ToString() },
						{ "IsReadOnly", category.IsReadOnly.ToString() },
						{ "IsRestricted", category.IsRestricted.ToString() },
						{ "Name", category.Name },
						{ "TopicCountCache", category.TopicCountCache.ToString() }
					};
				});

				variables.Clear();
				await template.Render(sw, "categories_footer", variables);

				variables.Clear();
				variables.Add("Title", viewModel.SiteName);
				variables.Add("GeneratedOn", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
				await template.Render(sw, "document_footer", variables);
			}
		}
	}
}
