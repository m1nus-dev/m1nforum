using M1nforum.Web.Infrastructure;
using System.IO;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using M1nforum.Web.Services.Entities;
using M1nforum.Web.Templates;

namespace M1nforum.Web.Handlers
{
    public class Comments
    {
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

            // cache
            if (!Program.Cache.DebuggingEnabled && httpContext.CacheContent(comments.Max(c => c.UpdatedOn))) // todo:  this is probably the wrong sort
            {
                return;
            }

            // response
            using (var body = new StreamWriter(httpContext.Response.Body))
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

                await body.WriteCommentsHeader(new
                {
					//	variables.Add("Category.Name", viewModel.Category.Name);
					Category = category,
                    Category_Id =  category.Id.ToString(), 
                    Topic_Id = topic.Id.ToString(),
                    Topic_Title = topic.Title,
                    Topic_Content = topic.Content
                });

                foreach (var comment in comments)
                {
                    await body.WriteCommentsRow(new
                    {
                        Comment = comment
					});
                }

                await body.WriteCommentsFooter();

                await body.WriteDocumentFooter(new
				{
					domain.Title,
					GeneratedOn = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
				});
            }
		}
    }
}
