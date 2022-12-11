using M1nforum.Web.Services.Entities;
using M1nforum.Web.Services.Repositories;
using System;
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

		public DataAccess(IRepository<Category> categoryRepository, IRepository<Comment> commentRepository, IRepository<Domain> domainRepository, IRepository<Topic> topicRepository)
		{
			_categoryRepository = categoryRepository;
			_commentRepository = commentRepository;
			_domainRepository = domainRepository;
			_topicRepository = topicRepository;
		}

		public Domain GetDomainByName(string name)
		{
			var domain = _domainRepository
				.List(d => d.Name.Equals(name))
				.FirstOrDefault();

			return domain;
		}

		public List<Category> GetCategoriesByDomainId(ulong domainId) 
		{
			var categories = _categoryRepository
				.List(c => c.DomainId.Equals(domainId))
				.OrderBy(c => c.Name)
				.ToList();

			return categories;
		}

		public Category GetCategoryById(ulong domainId, ulong categoryId)
		{
			var category = _categoryRepository
				.List(c => c.Id == categoryId && c.DomainId == domainId)
				.FirstOrDefault();

			return category;
		}

		public List<Topic> GetTopicsByCategoryId(ulong domainId, ulong categoryId)
		{
			var topics = _topicRepository
				.List(t => t.DomainId.Equals(domainId) && t.Categoryid == categoryId)
				.OrderByDescending(t => t.ActivityAt)
				.ToList();

			return topics;
		}

		internal Topic GetTopicById(ulong domainId, ulong categoryId, ulong topicId)
		{
			var topic = _topicRepository
				.List(t => t.DomainId.Equals(domainId) && t.Categoryid == categoryId && t.Id == topicId)
				.FirstOrDefault();

			return topic;
		}

		internal Topic GetTopicByIdUpdateViewCount(ulong domainId, ulong categoryId, ulong topicId)
		{
			var topic = GetTopicById(domainId, categoryId, topicId);

			if (topic != null)
			{
				topic.ViewCountCache++;
				_topicRepository.Update(topic);
			}

			return topic;
		}

		internal List<Comment> GetCommentsByTopicId(ulong domainId, ulong categoryId, ulong topicId)
		{
			var comments = _commentRepository
				.List(c => c.DomainId.Equals(domainId) && c.Categoryid == categoryId && c.TopicId == topicId)
				.OrderBy(c => c.CreatedOn)
				.ToList();

			return comments;
		}
	}
}
