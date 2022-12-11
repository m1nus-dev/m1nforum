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
	public class Topics
    {
        public async Task Get(HttpContext httpContext, ulong categoryId)
        {
            // context
            var domain = httpContext.Get<Domain>("Domain");
            // user, _ := r.Context().Value(CtxUserKey).(*models.User)

            // model
            var category = Program.Cache.Business.GetCategoryById(domain.Id, categoryId);
            var topics = Program.Cache.Business.GetTopicsByCategoryId(domain.Id, category.Id);

			// todo:  check for nulls

			// cache
			if (!Program.Cache.DebuggingEnabled && httpContext.CacheContent(topics.Max(c => c.UpdatedOn)))
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

				await body.WriteTopicsHeader(new
				{
					Category = category, 
				});

                foreach (var topic in topics)
                {
					await body.WriteTopicsRow(new
					{
						Category = category,
						Topic = topic
					});
				}

				await body.WriteTopicsFooter();

				await body.WriteDocumentFooter(new
				{
					domain.Title,
					GeneratedOn = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
				});
			}
        }
    }
}
