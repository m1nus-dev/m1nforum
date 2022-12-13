using System;

namespace M1nforum.Web.Services.Entities
{
	public interface IEntity
	{
		ulong Id { get; set; }
		DateTime CreatedOn { get; set; }
		string CreatedBy { get; set; }
		DateTime UpdatedOn { get; set; }
		string UpdatedBy { get; set; }
	}

	public class Entity : IEntity
	{
		public ulong Id { get; set; }
		public DateTime CreatedOn { get; set; }
		public DateTime UpdatedOn { get; set; }
		public string CreatedBy { get; set; }
		public string UpdatedBy { get; set; }
	}

	public class Domain : Entity
	{
		public string Name { get; set; }
		public string Title { get; set; }
		public string Description { get; set; }
	}

	public class Category : Entity
	{
		public ulong DomainId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public string HeaderMessage { get; set; }
		public int TopicCountCache { get; set; }
		public bool IsPrivate { get; set; }
		public bool IsReadOnly { get; set; }
		public bool IsRestricted { get; set; }
		public DateTime? ArchivedOn { get; set; }
	}

	public class Topic : Entity
	{
		public ulong DomainId { get; set; }
		public ulong Categoryid { get; set; }
		public ulong UserId { get; set; }
		public string Title { get; set; }
		public string Content { get; set; }
		public bool IsSticky { get; set; }
		public bool IsReadOnly { get; set; }
		public int CommentCountCache { get; set; }
		public int ViewCountCache { get; set; }
		public DateTime LastActivityOn { get; set; }
		public DateTime? ArchivedOn { get; set; }
		public string UserDisplayName { get; set; }
	}

	public class Comment : Entity
	{
		public ulong DomainId { get; set; }
		public ulong Categoryid { get; set; }
		public ulong TopicId { get; set; }
		public ulong UserId { get; set; }
		public string UserDisplayName { get; set; }
		public int CommentCountCache { get; set; } // todo:  ?
		public string Content { get; set; }
		public bool IsSticky { get; set; }
		public DateTime? ArchivedOn { get; set; }
	}

	public class User : Entity
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string About { get; set; }
		public string ResetToken { get; set; }
		public string ResetTokenCreatedOn { get; set; }
		public int PasswordFailedCount { get; set; }
		public DateTime? LockedUntil { get; set; }
		public bool IsAdmin { get; set; }
		public bool IsBanned { get; set; }
	}
}
