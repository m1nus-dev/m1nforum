using Microsoft.AspNetCore.Http;
using M1nforum.Web.Services.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using M1nforum.Web.Infrastructure;
using System.Security.Claims;
using M1nforum.Web.Infrastructure.Validation;

namespace M1nforum.Web.Services
{
	public class Business
	{
		private readonly DataAccess _dataAccess;

		internal Business(DataAccess dataAccess)
		{
			_dataAccess = dataAccess;
		}

		//
		// todo:  all of these methods need validation
		//

		internal Domain DomainGetFromHttpContext(HttpContext httpContext)
		{
			var host = httpContext
				.Request
				.Host
				.Value
				.ToLower()
				.Split(":")
				.First();

			return _dataAccess.DomainGetByName(host);
		}

		internal List<Domain> DomainsList(User user)
		{
			return UserIsAdmin(user) ?
				_dataAccess.DomainsList() :
				null;
		}

		internal List<Category> CategoriesGetByDomainId(ulong domainId)
		{
			return _dataAccess.CategoriesGetByDomainId(domainId);
		}

		internal Category CategoryGetById(ulong domainId, ulong categoryId)
		{
			return _dataAccess.CategoryGetById(domainId, categoryId);
		}

		internal List<Topic> TopicsGetByCategoryId(ulong domainId, ulong categoryId)
		{
			return _dataAccess.TopicsGetByCategoryId(domainId, categoryId);
		}

		internal Topic TopicGetById(ulong domainId, ulong categoryId, ulong topicId)
		{
			return _dataAccess.TopicGetById(domainId, categoryId, topicId);
		}

		internal Topic TopicGetByIdUpdateViewCount(ulong domainId, ulong categoryId, ulong topicId)
		{
			return _dataAccess.TopicGetByIdUpdateViewCount(domainId, categoryId, topicId);
		}

		internal List<Comment> CommentsGetByTopicId(ulong domainId, ulong categoryId, ulong topicId)
		{
			return _dataAccess.CommentsGetByTopicId(domainId, categoryId, topicId);
		}

		internal User LoginUser(ulong domainId, string username, string password)
		{
			username = username.ToLower();

			var user = _dataAccess.UserGetByUsername(domainId, username);

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
				user = _dataAccess.UserUpdate(domainId, user);
				return user;
			}
			else
			{
				user.PasswordFailedCount++;
				if (user.PasswordFailedCount >= 5)
				{
					user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
				}
				_dataAccess.UserUpdate(domainId, user);
				return null;
			}
		}

		internal User UserGetByClaims(ulong domainId, ClaimsPrincipal claimsPrincipal)
		{
			if (ulong.TryParse(claimsPrincipal.Claims
				.Where(c => c.Type == "Id")
				.Select(c => c.Value)
				.FirstOrDefault(), out var userId))
			{
				return UserGetByid(domainId, userId);
			}
			else
			{
				return null;
			}
		}

		internal User UserGetByid(ulong domainId, ulong userId)
		{
			return _dataAccess.UserGetById(domainId, userId);
		}

		internal void DomainInsert(User user, Domain domain)
		{
			if (!UserIsAdmin(user))
			{
				throw new Exception("User is not an admin"); // todo:  better
			}

			domain.CreatedOn = domain.UpdatedOn = DateTime.UtcNow;
			domain.CreatedBy = domain.UpdatedBy = user.Id.ToString(); // todo:  should this be a name?

			_dataAccess.DomainInsert(domain);
		}

		internal Domain DomainGetById(User user, ulong domainId)
		{
			return UserIsAdmin(user) ?
				_dataAccess.DomainGetByDomainId(domainId) :
				null;
		}

		internal bool UserIsAdmin(User user)
		{
			return user != null && !user.IsBanned && user.IsAdmin;
		}

		internal void DomainUpdate(User user, Domain domain)
		{
			if (!UserIsAdmin(user))
			{
				throw new Exception("User is not an admin"); // todo:  better
			}

			var oldDomain = DomainGetById(user, domain.Id);

			if (oldDomain == null)
			{
				throw new Exception("Domain does not exist."); // todo:  better
			}

			oldDomain.UpdatedBy = user.Id.ToString(); // todo:  should this be a name?
			oldDomain.UpdatedOn= DateTime.UtcNow;
			oldDomain.Description = domain.Description;
			oldDomain.Name = domain.Name;
			oldDomain.Title = domain.Title;

			_dataAccess.DomainUpdate(domain);
		}

		internal void DomainDelete(User user, Domain domain)
		{
			if (!UserIsAdmin(user))
			{
				throw new Exception("User is not an admin"); // todo:  better
			}

			_dataAccess.DomainDelete(domain.Id);
		}

		internal List<ValidationException> DomainGetValidationExceptions(Domain domain)
		{
			var exceptions = new Validation()
				.IsNotNull(domain.Title, nameof(domain.Title))
				.IsLengthInRange(domain.Title, nameof(domain.Title), 1, 64)
				.IsNotNull(domain.Name, nameof(domain.Name))
				.IsLengthInRange(domain.Name, nameof(domain.Name), 5, 64)
				.IsNotNull(domain.Description, nameof(domain.Description))
				.IsLengthInRange(domain.Description, nameof(domain.Description), 0, 128)
				.GetValidationExceptions();

			return exceptions;
		}
	}
}
