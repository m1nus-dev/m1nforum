using M1nforum.Web.Services.Entities;
using M1nforum.Web.Services.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace M1nforum.Web.Services
{
	public class DataAccess
	{
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<Comment> _commentRepository;
		private readonly IRepository<Domain> _domainRepository;
		private readonly IRepository<Topic> _topicRepository;
		private readonly IRepository<User> _userRepository;

		public DataAccess(IRepository<Category> categoryRepository, IRepository<Comment> commentRepository, IRepository<Domain> domainRepository, IRepository<Topic> topicRepository, IRepository<User> userRepository)
		{
			_categoryRepository = categoryRepository;
			_commentRepository = commentRepository;
			_domainRepository = domainRepository;
			_topicRepository = topicRepository;
			_userRepository = userRepository;
		}

		public Domain DomainGetByName(string name)
		{
			var domain = _domainRepository
				.List(d => d.Name == name)
				.FirstOrDefault();

			return domain;
		}

		internal List<Domain> DomainsList()
		{
			return _domainRepository
				.List();
		}


		public List<Category> CategoriesGetByDomainId(ulong domainId) 
		{
			var categories = _categoryRepository
				.List(c => c.DomainId == domainId)
				.OrderBy(c => c.Name)
				.ToList();

			return categories;
		}

		public Category CategoryGetById(ulong domainId, ulong categoryId)
		{
			var category = _categoryRepository
				.List(c => c.Id == categoryId && c.DomainId == domainId)
				.FirstOrDefault();

			return category;
		}

		public List<Topic> TopicsGetByCategoryId(ulong domainId, ulong categoryId)
		{
			var topics = _topicRepository
				.List(t => t.DomainId == domainId && t.Categoryid == categoryId)
				.OrderByDescending(t => t.LastActivityOn)
				.ToList();

			return topics;
		}

		internal Topic TopicGetById(ulong domainId, ulong categoryId, ulong topicId)
		{
			var topic = _topicRepository
				.List(t => t.DomainId == domainId && t.Categoryid == categoryId && t.Id == topicId)
				.FirstOrDefault();

			return topic;
		}

		internal Topic TopicGetByIdUpdateViewCount(ulong domainId, ulong categoryId, ulong topicId)
		{
			var topic = TopicGetById(domainId, categoryId, topicId);

			if (topic != null)
			{
				topic.ViewCountCache++;
				_topicRepository.Update(topic);
			}

			return topic;
		}

		internal List<Comment> CommentsGetByTopicId(ulong domainId, ulong categoryId, ulong topicId)
		{
			var comments = _commentRepository
				.List(c => c.DomainId == domainId && c.Categoryid == categoryId && c.TopicId == topicId)
				.OrderBy(c => c.CreatedOn)
				.ToList();

			return comments;
		}

		internal User UserGetByUsername(ulong domainId, string username)
		{
			var user = _userRepository
				.List(u => u.DomainId == domainId && u.Username == username)
				.FirstOrDefault();

			return user;
		}

		internal User UserUpdate(ulong domainid, User user)
		{
			_userRepository.Update(user);

			return _userRepository.List(u => u.DomainId == domainid && u.Id == user.Id)
				.FirstOrDefault();
		}

		internal User UserGetById(ulong domainId, ulong userId)
		{
			return _userRepository
				.List(u => u.DomainId == domainId && u.Id == userId)
				.FirstOrDefault();
		}

		internal void DomainInsert(Domain domain)
		{
			_domainRepository.Insert(domain);
		}

		internal Domain DomainGetByDomainId(ulong domainId)
		{
			return _domainRepository.List(d => d.Id == domainId)
				.FirstOrDefault();
		}

		internal void DomainUpdate(Domain domain)
		{
			_domainRepository.Update(domain);
		}

		internal void DomainDelete(ulong domainId)
		{
			_domainRepository.Delete(d => d.Id == domainId);
		}
	}
}
