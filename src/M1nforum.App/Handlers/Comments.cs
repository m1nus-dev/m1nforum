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
			var domain = Program.Cache.Business.DomainGetFromHttpContext(httpContext) ?? throw new PageNotFoundException("domain");
			var user = Program.Cache.Business.UserGetByClaims(domain.Id, httpContext.User);
			var category = Program.Cache.Business.CategoryGetById(domain.Id, categoryId) ?? throw new PageNotFoundException("category");
			var topic = Program.Cache.Business.TopicGetByIdUpdateViewCount(domain.Id, category.Id, topicId) ?? throw new PageNotFoundException("topic");
			var comments = Program.Cache.Business.CommentsGetByTopicId(domain.Id, category.Id, topic.Id) ?? new List<Comment>();
			var isAdmin = user?.IsAdmin == true && user?.IsBanned == false;

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
					GetCSSFileUrl = Program.Cache.GetCSSFileUrl(),
					FlashMessage = httpContext.ReadFlashMessage(), 
					IsAdmin = isAdmin, 
					PageHeader = domain.Title,
					SiteName = domain.Title,
					PageSubheader = domain.Description,
					PageTitle = "Categories - " + domain.Title,
					User = user
				});

                await body.WriteComments(new
                {
					Category = category,
                    Topic = topic,
					isAdmin= isAdmin,
                    Comments = comments
                });

                await body.WriteDocumentFooter(new
				{
					PageTitle = domain.Title,
					GeneratedOn = DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss")
				});
            }
		}
    }
}
