using M1nforum.Web.Infrastructure;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using M1nforum.Web.Services.Entities;

namespace M1nforum.Web.Features.Comments
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
			public Topic Topic { get; set; }
			public List<Comment> Comments { get; set; }
		}

		public async Task Get(HttpContext httpContext, ulong categoryId, ulong topicId)
		{
			// context
			var domain = httpContext.Get<Domain>("Domain");
			// user, _ := r.Context().Value(CtxUserKey).(*models.User)

			// data
			var category = Program.Cache.Business.GetCategoryById(domain.Id, categoryId);
			var topic = Program.Cache.Business.GetTopicById(domain.Id, category.Id, topicId);
			var comments = Program.Cache.Business.GetCommentsByTopicId(domain.Id, category.Id, topic.Id);

			// todo:  check for nulls

			//
			// caching is difficult.  The data may not have changed but the css changed and this would cache that bad css.
			// it is a trade off and inperfect.
			//
			if (!Program.Cache.DebuggingEnabled && httpContext.CacheContent(comments.Max(c => c.UpdatedOn))) // todo:  this is probably the wrong sort
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
			viewModel.Topic = topic;
			viewModel.Comments = comments;

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
				variables.Add("Category.Name", viewModel.Category.Name);
				variables.Add("Category.Id", viewModel.Category.Id.ToString());
				variables.Add("Topic.Id", viewModel.Topic.Id.ToString());
				variables.Add("Topic.Title", viewModel.Topic.Title);
				variables.Add("Topic.Content", viewModel.Topic.Content);
				await template.Render(sw, "comments_header", variables);

				// todo:  dont show archived?

				variables.Clear();
				await template.RenderLoop(sw, "comments_row", viewModel.Comments, (comment) =>
				{
					return new Dictionary<string, string>()
					{
						{ "Id", comment.Id.ToString() },
						{ "CreatedBy", comment.CreatedBy },
						{ "CreatedOn", comment.CreatedOn.ToString("MM/dd/YYYY") },
						{ "UpdatedBy", comment.UpdatedBy },
						{ "UpdatedOn", comment.UpdatedOn.ToString("MM/dd/YYYY") },
						{ "DomainId", comment.DomainId.ToString() },
						{ "CategoryId", comment.Categoryid.ToString() },
						{ "UserId", comment.UserId.ToString() },
						{ "TopicId", comment.TopicId.ToString() },
						{ "UserDisplayName", comment.UserDisplayName },
						{ "CommentCountCache", comment.CommentCountCache.ToString() },
						{ "IsSticky", comment.IsSticky.ToString() }, 
						{ "Content", comment.Content },
						{ "ArchivedAt", comment.ArchivedAt.ToString() }
					};
				});

				variables.Clear();
				await template.Render(sw, "comments_footer", variables);

				variables.Clear();
				variables.Add("Title", viewModel.Title); // todo:  use the site title here
				variables.Add("GeneratedOn", DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss"));
				await template.Render(sw, "document_footer", variables);
			}
		}
	}
}
