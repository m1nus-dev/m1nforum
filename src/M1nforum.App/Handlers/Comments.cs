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
    public class Comments
    {
        public async Task Get(HttpContext httpContext, ulong categoryId, ulong topicId)
        {
			// data
			var domain = Program.Cache.Business.GetDomainFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.GetuserByClaims(httpContext.User);
			var category = Program.Cache.Business.GetCategoryById(domain.Id, categoryId) ?? throw new PageNotFoundException("category");
			var topic = Program.Cache.Business.GetTopicByIdUpdateViewCount(domain.Id, category.Id, topicId) ?? throw new PageNotFoundException("topic");
			var comments = Program.Cache.Business.GetCommentsByTopicId(domain.Id, category.Id, topic.Id) ?? new List<Comment>();

			// cache
			if (!Program.Cache.DebuggingEnabled && httpContext.CacheContent(comments.Max(c => c.UpdatedOn))) // todo:  this is probably the wrong sort
            {
                return;
            }

			// response
			await using (var body = await httpContext.StartHtmlResponse())
			{
				await body.WriteDocumentHeader(new
                {
					User = user,
					SiteName = domain.Title,
                    Title = "Categories - " + domain.Title,
                    CSSFilename = Program.Cache.DebuggingEnabled ?
                        "app.css?v=" + "wwwroot/css/app.css".GetCSSFileTimestamp() :
                        "app.min.css?v=" + "wwwroot/css/app.min.css".GetCSSFileTimestamp(), 
                    Header = domain.Title,
                    Subheader = domain.Description,
					FlashMessage = httpContext.ReadFlashMessage()
				});

                await body.WriteComments(new
                {
					Category = category,
                    Topic = topic,
                    Comments = comments
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
