using Microsoft.AspNetCore.Http;
using M1nforum.Web.Services.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace M1nforum.Web.Services
{
    public class Business
	{
		private readonly DataAccess _dataAccess;

		public Business(DataAccess dataAccess)
		{
			_dataAccess = dataAccess;
		}

		public Domain GetDomainFromHttpContext(HttpContext httpContext)
		{
			var host = httpContext
				.Request
				.Host
				.Value
				.ToLower()
				.Split(":")
				.First();

			return _dataAccess.GetDomainByName(host);
		}

		public List<Category> GetCategoriesByDomainId(ulong domainId)
		{
			return _dataAccess.GetCategoriesByDomainId(domainId);
		}

		public Category GetCategoryById(ulong domainId, ulong categoryId)
		{
			return _dataAccess.GetCategoryById(domainId, categoryId);
		}

		internal List<Topic> GetTopicsByCategoryId(ulong domainId, ulong categoryId)
		{
			return _dataAccess.GetTopicsByCategoryId(domainId, categoryId);
		}

		internal Topic GetTopicById(ulong domainId, ulong categoryId, ulong topicId)
		{
			return _dataAccess.GetTopicById(domainId, categoryId, topicId);
		}

		internal List<Comment> GetCommentsByTopicId(ulong domainId, ulong categoryId, ulong topicId)
		{
			return _dataAccess.GetCommentsByTopicId(domainId, categoryId, topicId);
		}
	}
}
