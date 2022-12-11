using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using M1nforum.Web.Infrastructure;
using M1nforum.Web.Services;
using M1nforum.Web.Services.Entities;
using M1nforum.Web.Services.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using M1nforum.Web.Handlers;

namespace M1nforum.Web
{
	public class Program
	{
		public static Cache Cache { get; protected set; }

		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// todo:  this is some bs that I dont know why started.
			builder.Services.Configure<KestrelServerOptions>(options => options.AllowSynchronousIO = true);

			await Program.Seed();

			// setup services instances - this is similar to Services.AddTransient

			Cache = new Cache();

			var dataAccess = new DataAccess(
				new XmlRepository<Category>(),
				new XmlRepository<Comment>(),
				new XmlRepository<Domain>(),
				new XmlRepository<Topic>());
			Cache.Business = new Business(dataAccess);

			Cache.DebuggingEnabled = true;

			var app = builder.Build();

			app.UseResponseCaching();

			app.UseStaticFiles(new StaticFileOptions
			{
				OnPrepareResponse = httpContext =>
				{
					const int durationInSeconds = 60 * 60 * 24;
					httpContext.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
				}
			});

			// app.UseResponseCompression();

			// error handler
			app.Use(async (httpContext, next) =>
			{
				try
				{
					await next(httpContext);
				}
				catch (Exception exception)
				{
					if (Cache.DebuggingEnabled)
					{
						await httpContext.Response.WriteAsync(exception.Message);
						return;
					}

					await httpContext.Response.WriteAsync("Error");
				}
			});

			// request context
			app.Use(async (httpContext, next) =>
			{
				var domain = Cache.Business.GetDomainFromHttpContext(httpContext);

				if (domain == null)
				{
					throw new Exception($"Domain not found.");
				}

				httpContext.Items.Add("Domain", domain);

				await next(httpContext);
			});

			app.MapGet("/", async (httpContext) => await new Home().Get(httpContext));
			app.MapGet("/categories", async (httpContext) => await new Categories().Get(httpContext));
			app.MapGet("/categories/{categoryId}", async (HttpContext httpContext, ulong categoryId) => await new Topics().Get(httpContext, categoryId));
			app.MapGet("/categories/{categoryId}/topics/{topicId}", async (HttpContext httpContext, ulong categoryId, ulong topicId) => await new Comments().Get(httpContext, categoryId, topicId));

			app.Run();
		}

		private static string GetWords(int count)
		{
			var words = @"leverage agile frameworks to provide a robust synopsis for high level overviews iterative approaches to corporate strategy foster collaborative thinking to further the overall value proposition organically grow the holistic world view of disruptive innovation via workplace diversity and empowerment bring to the table win-win survival strategies to ensure proactive domination at the end of the day, going forward, a new normal that has evolved from generation X is on the runway heading towards a streamlined cloud solution user generated content in real-time will have multiple touchpoints for offshoring capitalize on low hanging fruit to identify a ballpark value added activity to beta test override the digital divide with additional clickthroughs from DevOps nanotechnology immersion along the information highway will close the loop on focusing solely on the bottom line"
				.Split(" ");

			var output  = new string[count];
			var random = new Random();

			for (var counter = 0; counter < count; counter++)
			{
				output[counter] = words[random.Next(words.Length)];
			}

			return string.Join(" ", output);
		}

		public static async Task Seed()
		{
			IRepository<Category> categoryRepository = new XmlRepository<Category>();
			IRepository<Comment> commentRepository = new XmlRepository<Comment>();
			IRepository<Domain> domainRepository = new XmlRepository<Domain>();
			IRepository<Topic> topicRepository = new XmlRepository<Topic>();
			var random = new Random();

			var domains = domainRepository.List();

			if (!domains.Any())
			{
				// create domains

				var domain = new Domain();

				domain.Id = IdGenerator.NewId();
				domain.CreatedBy = domain.UpdatedBy = "system";
				domain.CreatedOn = domain.UpdatedOn = DateTime.UtcNow;

				domain.Description = "Testing domain for the localhost.";
				domain.Name = "localhost";
				domain.Title = "The Host that is Local";

				domainRepository.Insert(domain);
			}

			domains = domainRepository.List();

			var categories = categoryRepository.List();
			if (!categories.Any())
			{
				foreach (var domain in domains)
				{
					// create categories
					for (var counter = 0; counter < 10; counter++)
					{
						var category = new Category();

						category.ArchivedAt = null;
						category.Description = "Category.Description - " + GetWords(random.Next(1, 5));
						category.HeaderMessage = "Category.HeaderMessage - " + GetWords(random.Next(1, 3));
						category.IsPrivate = counter % 3 == 0;
						category.IsReadOnly = false;
						category.IsRestricted = false;
						category.Name = "Category.Name - " + GetWords(random.Next(1, 3));
						category.TopicCountCache = 0;

						category.DomainId = domain.Id;

						category.Id = IdGenerator.NewId();
						category.CreatedBy = category.UpdatedBy = "system";
						category.CreatedOn = category.UpdatedOn = DateTime.UtcNow;

						categoryRepository.Insert(category);
					}
				}
			}

			categories = categoryRepository.List();

			var topics = topicRepository.List();

			if (!topics.Any())
			{
				foreach (var domain in domains)
				{
					foreach (var category in categories.Where(c => c.DomainId == domain.Id).ToList())
					{
						for (var counter = 0; counter < 10; counter++)
						{
							var topic = new Topic();

							topic.ActivityAt = DateTime.UtcNow;
							topic.IsSticky = new Random().Next(0, 1) == 0;
							topic.CommentCountCache = 0;
							topic.ArchivedAt = null;
							topic.Content = "Topic.Content - " + GetWords(random.Next(1, 40));
							topic.IsReadOnly = counter == 3;
							topic.IsSticky = false;
							topic.CommentCountCache = 0;
							topic.ViewCountCache = 0;
							topic.Title = "Topic.Title - " + GetWords(random.Next(1,3));
							topic.UserDisplayName = "Mr. " + GetWords(1);
							topic.UserId = (ulong)new Random().NextInt64();

							topic.DomainId = domain.Id;
							topic.Categoryid = category.Id;

							topic.Id = IdGenerator.NewId();
							topic.CreatedBy = topic.UpdatedBy = "system";
							topic.CreatedOn = topic.UpdatedOn = DateTime.UtcNow;

							category.TopicCountCache++;

							categoryRepository.Update(category);
							topicRepository.Insert(topic);
						}
					}
				}
			}

			topics = topicRepository.List();

			var comments = commentRepository.List();

			if (!comments.Any())
			{
				foreach (var domain in domains)
				{
					foreach (var category in categories.Where(c => c.DomainId == domain.Id))
					{
						foreach (var topic in topics.Where(t => t.DomainId == domain.Id && t.Categoryid == category.Id))
						{
							for (var counter = 0; counter < 10; counter++)
							{
								var comment = new Comment();

								comment.ArchivedAt = null;
								comment.CommentCountCache = 0; // todo:  ?
								comment.Content = "Comment.Content - " + GetWords(random.Next(1, 50));
								comment.IsSticky = false;
								comment.UserDisplayName = "Mrs. " + GetWords(1);
								comment.UserId = (ulong)new Random().NextInt64();

								comment.DomainId = domain.Id;
								comment.Categoryid = category.Id;
								comment.TopicId = topic.Id;

								comment.CreatedBy = topic.UpdatedBy = "system";
								comment.CreatedOn = topic.UpdatedOn = DateTime.UtcNow;
								comment.Id = IdGenerator.NewId();

								topic.CommentCountCache++;

								topicRepository.Update(topic);
								commentRepository.Insert(comment);
							}
						}
					}
				}
			}

			comments = commentRepository.List();
		}
	}
}











