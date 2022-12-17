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
			var user = Program.Cache.Business.GetUserByClaims(domain.Id, httpContext.User);
			var category = Program.Cache.Business.GetCategoryById(domain.Id, categoryId) ?? throw new PageNotFoundException("category");
			var topics = Program.Cache.Business.GetTopicsByCategoryId(domain.Id, category.Id) ?? new List<Topic>();
			var isAdmin = user?.IsAdmin == true && user?.IsBanned == false;

			// cache
			if (!Program.Cache.DebuggingEnabled && httpContext.CacheContent(topics.Max(t => t.UpdatedOn)))
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

				await body.WriteTopics(new
				{
					Category = category,
					IsAdmin = isAdmin, 
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
