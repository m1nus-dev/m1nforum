using M1nforum.Web.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using M1nforum.Web.Services.Entities;

namespace M1nforum.Web.Features.Topics
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

			public Category Category { get; set; }

			// todo:  paging
			public List<Topic> Topics { get; set; }
		}

		public async Task Get(HttpContext httpContext, ulong categoryId)
		{
			// context
			var domain = httpContext.Get<Domain>("Domain");
			// user, _ := r.Context().Value(CtxUserKey).(*models.User)

			// data
			var category = Program.Cache.Business.GetCategoryById(domain.Id, categoryId);
			var topics = Program.Cache.Business.GetTopicsByCategoryId(domain.Id, category.Id);

			// todo:  check for nulls

			//
			// caching is difficult.  The data may not have changed but the css changed and this would cache that bad css.
			// it is a trade off and inperfect.
			//
			if (!Program.Cache.DebuggingEnabled && httpContext.CacheContent(topics.Max(c => c.UpdatedOn)))
			{
				return;
			}

			// respond
			var viewModel = new ViewModel();
			viewModel.SiteName = domain.Title;
			viewModel.Title = "Topics - " + domain.Title;
			viewModel.Header = domain.Title + " - " + category.Name;
			viewModel.Subheader = category.Description;
			viewModel.Category = category;
			viewModel.Topics = topics;

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
				variables.Add("CategoryName", viewModel.Category.Name);
				variables.Add("CategoryId", viewModel.Category.Id.ToString());
				await template.Render(sw, "topics_header", variables);

				// todo:  dont show archived?

				variables.Clear();
				await template.RenderLoop(sw, "topics_row", viewModel.Topics, (topic) =>
				{
					return new Dictionary<string, string>()
					{
						{ "Id", topic.Id.ToString() },
						{ "CreatedBy", topic.CreatedBy },
						{ "CreatedOn", topic.CreatedOn.ToString("MM/dd/YYYY") },
						{ "UpdatedBy", topic.UpdatedBy },
						{ "UpdatedOn", topic.UpdatedOn.ToString("MM/dd/YYYY") },
						{ "DomainId", topic.DomainId.ToString() },
						{ "CategoryId", topic.Categoryid.ToString() },
						{ "UserId", topic.UserId.ToString() },
						{ "Title", topic.Title },
						{ "Content", topic.Content },
						{ "IsSticky", topic.IsSticky.ToString() },
						{ "IsReadOnly", topic.IsReadOnly.ToString() },
						{ "CommentCountCache", topic.CommentCountCache.ToString() },
						{ "ViewCountCache", topic.ViewCountCache.ToString() },
						{ "ActivityAt", topic.ActivityAt.ToString() },
						{ "ArchivedAt", topic.ArchivedAt.ToString() },
						{ "UserDisplayName", topic.UserDisplayName },
					};
				});

				variables.Clear();
				await template.Render(sw, "topics_footer", variables);

				variables.Clear();
				variables.Add("Title", viewModel.Title); // todo:  use the site title here
				variables.Add("GeneratedOn", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
				await template.Render(sw, "document_footer", variables);
			}
		}
	}
}
