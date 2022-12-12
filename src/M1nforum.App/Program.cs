using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;
using M1nforum.Web.Infrastructure;
using M1nforum.Web.Services;
using M1nforum.Web.Services.Entities;
using M1nforum.Web.Services.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using M1nforum.Web.Handlers;
using M1nforum.Web.Infrastructure.Exceptions;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace M1nforum.Web
{
	public class Program
	{
		public static Cache Cache { get; protected set; }

		public static async Task Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			await Program.Seed();

			// auth
			builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
				.AddCookie(options =>
				{
					options.LoginPath = new PathString("/login");
				});
			builder.Services.AddAuthorization();

			// setup services instances - this is similar to Services.AddTransient

			Cache = new Cache();

			var dataAccess = new DataAccess(
				new XmlRepository<Category>(),
				new XmlRepository<Comment>(),
				new XmlRepository<Domain>(),
				new XmlRepository<Topic>(), 
				new XmlRepository<User>());
			Cache.Business = new Business(dataAccess);

			// todo:  move this to environment variable
			Cache.DebuggingEnabled = true;

			var app = builder.Build();

			// auth
			app.UseAuthentication();
			app.UseAuthorization();

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
				catch (PageNotFoundException pnfe)
				{
					// todo:  since we are writing directly to the body stream, there is no way to do good error pages once the views start, right?

					if (Cache.DebuggingEnabled)
					{
						await httpContext.Response.WriteAsync("Page not found - " + pnfe.Message);
						return;
					}

					await httpContext.Response.WriteAsync("Page Not Found");

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

			app.MapGet("/", async (httpContext) => await new Home().Get(httpContext));
			app.MapGet("/categories", async (httpContext) => await new Categories().Get(httpContext));
			app.MapGet("/categories/{categoryId}", async (HttpContext httpContext, ulong categoryId) => await new Topics().Get(httpContext, categoryId));
			app.MapGet("/categories/{categoryId}/topics/{topicId}", async (HttpContext httpContext, ulong categoryId, ulong topicId) => await new Comments().Get(httpContext, categoryId, topicId));
			app.MapGet("/login", async (HttpContext httpContext) => await new Login().Get(httpContext));
			app.MapPost("/login", async (HttpContext httpContext) => await new Login().Post(httpContext));
			app.Map("/logout", async (httpContext) => await new Logout().Get(httpContext));

			app.Run();
		}

		private static string GetWords(int count)
		{
			var words = @"leverage agile frameworks to provide a robust synopsis for high level overviews iterative approaches to corporate strategy foster collaborative thinking to further the overall value proposition organically grow the holistic world view of disruptive innovation via workplace diversity and empowerment bring to the table win-win survival strategies to ensure proactive domination at the end of the day, going forward, a new normal that has evolved from generation X is on the runway heading towards a streamlined cloud solution user generated content in real-time will have multiple touchpoints for offshoring capitalize on low hanging fruit to identify a ballpark value added activity to beta test override the digital divide with additional clickthroughs from DevOps nanotechnology immersion along the information highway will close the loop on focusing solely on the bottom line"
				.Split(" ");

			var output = new string[count];
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
			IRepository<User> userRepository = new XmlRepository<User>();

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
							topic.Title = "Topic.Title - " + GetWords(random.Next(1, 3));
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

			var users = userRepository.List();

			if (!users.Any())
			{
				var user = new User();
				user.CreatedBy = user.UpdatedBy = "system";
				user.CreatedOn = user.UpdatedOn = DateTime.UtcNow;
				user.Id = IdGenerator.NewId();

				user.About = GetWords(100);
				user.Email = GetWords(1) + "@" + GetWords(1) + ".com";
				user.IsAdmin = true;
				user.IsBanned = false;
				user.Password = Security.GenerateHashPassword("Password1");
				user.Username = "admin";

				userRepository.Insert(user);

				user = new User();
				user.CreatedBy = user.UpdatedBy = "system";
				user.CreatedOn = user.UpdatedOn = DateTime.UtcNow;
				user.Id = IdGenerator.NewId();

				user.About = GetWords(100);
				user.Email = GetWords(1) + "@" + GetWords(1) + ".com";
				user.IsAdmin = false;
				user.IsBanned = false;
				user.Password = Security.GenerateHashPassword("Password1");
				user.Username = "user";

				userRepository.Insert(user);

				user = new User();
				user.CreatedBy = user.UpdatedBy = "system";
				user.CreatedOn = user.UpdatedOn = DateTime.UtcNow;
				user.Id = IdGenerator.NewId();

				user.About = GetWords(100);
				user.Email = GetWords(1) + "@" + GetWords(1) + ".com";
				user.IsAdmin = false;
				user.IsBanned = true;
				user.Password = Security.GenerateHashPassword("Password1");
				user.Username = "banned";

				userRepository.Insert(user);
			}
		}
	}
}












