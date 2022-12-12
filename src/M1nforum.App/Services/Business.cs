using Microsoft.AspNetCore.Http;
using M1nforum.Web.Services.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using M1nforum.Web.Infrastructure;

namespace M1nforum.Web.Services
{
    public class Business
	{
		private readonly DataAccess _dataAccess;

		internal Business(DataAccess dataAccess)
		{
			_dataAccess = dataAccess;
		}

		internal Domain GetDomainFromHttpContext(HttpContext httpContext)
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

		internal List<Category> GetCategoriesByDomainId(ulong domainId)
		{
			return _dataAccess.GetCategoriesByDomainId(domainId);
		}

		internal Category GetCategoryById(ulong domainId, ulong categoryId)
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

		internal Topic GetTopicByIdUpdateViewCount(ulong domainId, ulong categoryId, ulong topicId)
		{
			return _dataAccess.GetTopicByIdUpdateViewCount(domainId, categoryId, topicId);
		}

		internal List<Comment> GetCommentsByTopicId(ulong domainId, ulong categoryId, ulong topicId)
		{
			return _dataAccess.GetCommentsByTopicId(domainId, categoryId, topicId);
		}

		internal User Login(string username, string password)
		{
			username = username.ToLower();

			var user = _dataAccess.GetUserByUsername(username);

			if (user == null)
			{
				return null;
			}

			if (user.LockedUntil.HasValue && user.LockedUntil.Value > DateTime.UtcNow)
			{
				return null;
			}

			var passwordsMatch = Security.VerifyPassword(user.Password, password);

			if (passwordsMatch)
			{
				user.PasswordFailedCount = 0;
				user = _dataAccess.UpdateUser(user);
				return user;
			}
			else
			{
				user.PasswordFailedCount++;
				if (user.PasswordFailedCount >= 5) 
				{
					user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
				}
				_dataAccess.UpdateUser(user);
				return null;
			}
		}
	}
}
