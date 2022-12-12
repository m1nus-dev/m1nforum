using M1nforum.Web.Infrastructure;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using M1nforum.Web.Services.Entities;
using M1nforum.Web.Templates;
using System.Collections.Generic;
using M1nforum.Web.Infrastructure.Exceptions;

namespace M1nforum.Web.Handlers
{
	public class Topics
    {
        public async Task Get(HttpContext httpContext, ulong categoryId)
        {
			// model
			var domain = Program.Cache.Business.GetDomainFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var category = Program.Cache.Business.GetCategoryById(domain.Id, categoryId) ?? throw new PageNotFoundException("category");
			var topics = Program.Cache.Business.GetTopicsByCategoryId(domain.Id, category.Id) ?? new List<Topic>();

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
						"app.min.css?v=" + "wwwroot/css/app.min.css".GetCSSFileTimestamp(), 
					Header = domain.Title,
					Subheader = domain.Description,
					FlashMessage = httpContext.ReadFlashMessage()
				});

				await body.WriteTopics(new
				{
					Category = category, 
					Topics = topics
				});

				await body.WriteDocumentFooter(new
				{
					domain.Title,
					GeneratedOn = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
				});
			}
        }
    }
}
