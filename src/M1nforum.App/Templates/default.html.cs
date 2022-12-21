using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using M1nforum.Web.Infrastructure.Validation;

namespace M1nforum.Web.Templates
{ 
	public static partial class Views
	{

	public static async Task WriteDocumentHeader(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
<!doctype html>
<html lang=""en"">
<head>
	<meta charset=""utf-8"">
	<meta name=""viewport"" content=""width=device-width, initial-scale=1, shrink-to-fit=no"">
	<meta name=""description"" content=""M1nforum example site"">
	<title>");
await streamWriter.WriteAsync(viewModel.PageTitle);
await streamWriter.WriteAsync(@"</title>
	<link rel=""stylesheet"" href=""/css/");
await streamWriter.WriteAsync(Program.Cache.GetCSSFileUrl());
await streamWriter.WriteAsync(@""" />
</head>
<body>
	<nav class=""nav"" tabindex=""-1"" onclick=""this.focus()"">
		<div class=""container"">
			<a class=""pagename current"" href=""/"">");
await streamWriter.WriteAsync(viewModel.SiteName);
await streamWriter.WriteAsync(@"</a>
			<a href=""/"">Home</a>
			");
 if (viewModel.User == null) { 
await streamWriter.WriteAsync(@"
			<a href=""/login"">Login</a>
			");
 } else { 
await streamWriter.WriteAsync(@"
				");
 if (viewModel.IsAdmin == true) { 
await streamWriter.WriteAsync(@"
				<a href=""/domains"">Domains</a>
				");
 } 
await streamWriter.WriteAsync(@"
			<a href=""/logout"">Logout ");
await streamWriter.WriteAsync(viewModel.User.Username);
await streamWriter.WriteAsync(@"</a>
			");
 } 
await streamWriter.WriteAsync(@"
		</div>
	</nav>
	<div class=""container"">
		<h1 class=""mb0"">");
await streamWriter.WriteAsync(viewModel.PageHeader);
await streamWriter.WriteAsync(@"</h1>
		<p class=""mt0"">");
await streamWriter.WriteAsync(viewModel.PageSubheader);
await streamWriter.WriteAsync(@"</p>
	</div>

	");
 if (viewModel.FlashMessage != "") { var messageType = viewModel.FlashMessage.Split("-", 2)[0]; var messageBody = viewModel.FlashMessage.Split("-", 2)[1]; 
await streamWriter.WriteAsync(@"
	<div class=""container"">
		<div class=""bg-");
await streamWriter.WriteAsync(messageType);
await streamWriter.WriteAsync(@" white pv1 ph2 rounded"">
			");
await streamWriter.WriteAsync(messageBody);
await streamWriter.WriteAsync(@"
		</div>
	</div>
	");
 } 

await streamWriter.WriteAsync(@"
");
	} 

	public static async Task WriteDomains(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		");
 if (viewModel.IsAdmin == true) { 
await streamWriter.WriteAsync(@"
		<a href=""/domains/add"">New Domain</a>
		");
 } 
await streamWriter.WriteAsync(@"
		<table class=""table"">
			<thead>
				<tr>
					<th>Name</th>
					<th>Title</th>
					<th>Description</th>
					<th>Tasks</th>
				</tr>
			</thead>
			<tbody>
				");
 for (var counter = 0; counter < viewModel.Domains.Count; counter++) {
				var domain = viewModel.Domains[counter]; 
await streamWriter.WriteAsync(@"
				<tr>
					<td><a href=""/domains/read/");
await streamWriter.WriteAsync(domain.Id.ToString());
await streamWriter.WriteAsync(@""">");
await streamWriter.WriteAsync(domain.Name);
await streamWriter.WriteAsync(@"</a></td>
					<td>");
await streamWriter.WriteAsync(domain.Title);
await streamWriter.WriteAsync(@"</td>
					<td>");
await streamWriter.WriteAsync(domain.Description);
await streamWriter.WriteAsync(@"</td>
					<td><a href=""/domains/edit/");
await streamWriter.WriteAsync(domain.Id.ToString());
await streamWriter.WriteAsync(@""">Edit</a> <a class=""warning"" href=""/domains/delete/");
await streamWriter.WriteAsync(domain.Id.ToString());
await streamWriter.WriteAsync(@""">Delete</a> </td>
				</tr>
				");
 } 

await streamWriter.WriteAsync(@"
			</tbody>
		</table>
	</div>
");
	} 

	public static async Task WriteDomain(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		<!-- todo:  message -->

		<form action=""");
await streamWriter.WriteAsync(viewModel.Action);
await streamWriter.WriteAsync(@""" method=""POST"">
			<input type=""hidden"" name=""Id"" value=""");
await streamWriter.WriteAsync(viewModel.Domain.Id.ToString());
await streamWriter.WriteAsync(@""" />
			<fieldset class=""b0 m0 p0"">
				<!-- todo:  csrf -->
				<div class=""row"">
					<div class=""col 6"">
						");
 if (viewModel.IsReadonly) { 
await streamWriter.WriteAsync(@"
							<p><strong>Name:</strong>  ");
await streamWriter.WriteAsync(viewModel.Domain.Name);
await streamWriter.WriteAsync(@"</p>
						");
 } else { 
await streamWriter.WriteAsync(@"
							<label for=""Name"">Name:</label>
							<input class=""card w-100"" type=""text"" name=""Name"" placeholder=""Name"" value=""");
await streamWriter.WriteAsync(viewModel.Domain.Name);
await streamWriter.WriteAsync(@""" required />
							");
 foreach (var validationException in (viewModel.ValidationExceptions as List<ValidationException>).Where(ve => ve.parameterName == "Name")) { 
await streamWriter.WriteAsync(@"
							<p><b class=""error"">* ");
await streamWriter.WriteAsync(validationException.message);
await streamWriter.WriteAsync(@"</b></p>
							");
 } 
await streamWriter.WriteAsync(@"
						");
 } 
await streamWriter.WriteAsync(@"
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						");
 if (viewModel.IsReadonly) { 
await streamWriter.WriteAsync(@"
							<p><strong>Title:</strong>  ");
await streamWriter.WriteAsync(viewModel.Domain.Title);
await streamWriter.WriteAsync(@"</p>
						");
 } else { 
await streamWriter.WriteAsync(@"
							<label for=""Name"">Title:</label>
							<input class=""card w-100"" type=""text"" name=""Title"" placeholder=""Title"" value=""");
await streamWriter.WriteAsync(viewModel.Domain.Title);
await streamWriter.WriteAsync(@""" required />
							");
 foreach (var validationException in (viewModel.ValidationExceptions as List<ValidationException>).Where(ve => ve.parameterName == "Title")) { 
await streamWriter.WriteAsync(@"
							<p><b class=""error"">* ");
await streamWriter.WriteAsync(validationException.message);
await streamWriter.WriteAsync(@"</b></p>
							");
 } 
await streamWriter.WriteAsync(@"
						");
 } 
await streamWriter.WriteAsync(@"
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						");
 if (viewModel.IsReadonly) { 
await streamWriter.WriteAsync(@"
							<p><strong>Description:</strong>  ");
await streamWriter.WriteAsync(viewModel.Domain.Description);
await streamWriter.WriteAsync(@"</p>
						");
 } else { 
await streamWriter.WriteAsync(@"
							<label for=""Description"">Description:</label>
							<input class=""card w-100"" type=""text"" name=""Description"" placeholder=""Description"" value=""");
await streamWriter.WriteAsync(viewModel.Domain.Description);
await streamWriter.WriteAsync(@""" />
							");
 foreach (var validationException in (viewModel.ValidationExceptions as List<ValidationException>).Where(ve => ve.parameterName == "Description")) { 
await streamWriter.WriteAsync(@"
							<p><b class=""error"">* ");
await streamWriter.WriteAsync(validationException.message);
await streamWriter.WriteAsync(@"</b></p>
							");
 } 
await streamWriter.WriteAsync(@"
						");
 } 
await streamWriter.WriteAsync(@"
					</div>
					<div class=""col 6""></div>
				</div>

				<div class=""row"">
					<div class=""col 6"">
						");
 if (viewModel.ActionButton != "") { 
await streamWriter.WriteAsync(@"
						<input class=""btn primary"" type=""submit"" value=""");
await streamWriter.WriteAsync(viewModel.ActionButton);
await streamWriter.WriteAsync(@""" />
						<a href=""/domains"">Back To List</a>
						");
 } else { 
await streamWriter.WriteAsync(@"
						<a href=""/domains/edit/");
await streamWriter.WriteAsync(viewModel.Domain.Id.ToString());
await streamWriter.WriteAsync(@""">Edit</a> | <a href=""/domains"">Back To List</a>
						");
 }

await streamWriter.WriteAsync(@"
					</div>
					<div class=""col 6""></div>
				</div>
			</fieldset>
		</form>
	</div>
");
	} 

	public static async Task WriteCategories(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		");
 if (viewModel.IsAdmin == true) { 
await streamWriter.WriteAsync(@"
		<a href=""/category"">new category</a>
		");
 } 
await streamWriter.WriteAsync(@"
		<table class=""table"">
			<thead>
				<tr>
					<th>Name</th>
					<th>Topics</th>
					<th>Last Post</th>
				</tr>
			</thead>
			<tbody>
				");
 for (var counter = 0; counter < viewModel.Categories.Count; counter++) {
				var category = viewModel.Categories[counter]; 
await streamWriter.WriteAsync(@"
				<tr>
					<td><a href=""/categories/");
await streamWriter.WriteAsync(category.Id.ToString());
await streamWriter.WriteAsync(@""">");
await streamWriter.WriteAsync(category.Name);
await streamWriter.WriteAsync(@"</a><br />");
await streamWriter.WriteAsync(category.Description);
await streamWriter.WriteAsync(@"</td>
					<td>");
await streamWriter.WriteAsync(category.TopicCountCache.ToString());
await streamWriter.WriteAsync(@"</td>
					<td>todo:  last post	</td>
				</tr>
				");
 } 

await streamWriter.WriteAsync(@"
			</tbody>
		</table>
	</div>
");
	} 

	public static async Task WriteCategoryAdd(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		<h2>Add Category</h2>
		<form action=""/category/add"" method=""POST"">
			<fieldset class=""b0 m0 p0"">
				<input type=""hidden"" name=""csrf"" value=""");
await streamWriter.WriteAsync(viewModel.CSRF);
await streamWriter.WriteAsync(@""">
				<div class=""row"">
					<div class=""col 6"">
						<label for=""username"">Name:</label>
						<input class=""card w-100"" type=""text"" name=""username"" placeholder=""username"" required />
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						<label for=""password"">Description:</label>
						<input class=""card w-100"" type=""password"" name=""password"" placeholder=""password"" required />
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						<label for=""password"">Header Message:</label>
						<input class=""card w-100"" type=""password"" name=""password"" placeholder=""password"" required />
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						<label for=""password"">Is Private:</label>
						<input class=""card w-100"" type=""password"" name=""password"" placeholder=""password"" required />
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						<label for=""password"">Is Readonly:</label>
						<input class=""card w-100"" type=""password"" name=""password"" placeholder=""password"" required />
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						<label for=""password"">Is Restricted:</label>
						<input class=""card w-100"" type=""password"" name=""password"" placeholder=""password"" required />
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						<label for=""password"">Archived On:</label>
						<input class=""card w-100"" type=""password"" name=""password"" placeholder=""password"" required />
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						Don't have an account? <a href=""/signup?ReturnURL=");
await streamWriter.WriteAsync(viewModel.ReturnURL);

await streamWriter.WriteAsync(@""">Signup</a> : <a href=""/forgotpass"">Forgot password?</a>
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						<input class=""btn primary"" type=""submit"" value=""Save"" />
					</div>
					<div class=""col 6""></div>
				</div>
			</fieldset>
		</form>
	</div>
");
	} 

	public static async Task WriteTopics(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		<h2><a href=""/categories/"">");
await streamWriter.WriteAsync(viewModel.Category.Name);
await streamWriter.WriteAsync(@"</a> \ Topics</h2>
		<table class=""table"">
			<thead>
				<tr>
					<th>Topic</th>
					<th>Comments</th>
					<th>Views</th>
					<th>Last Post</th>
				</tr>
			</thead>
			<tbody>
				");
 for (var counter = 0; counter < viewModel.Topics.Count; counter++) {
				var topic = viewModel.Topics[counter]; 
await streamWriter.WriteAsync(@"
				<tr>
					<td><a href=""/categories/");
await streamWriter.WriteAsync(viewModel.Category.Id.ToString());
await streamWriter.WriteAsync(@"/topics/");
await streamWriter.WriteAsync(topic.Id.ToString());
await streamWriter.WriteAsync(@""">");
await streamWriter.WriteAsync(topic.Title);
await streamWriter.WriteAsync(@"</a><br />By ");
await streamWriter.WriteAsync(topic.UserDisplayName);
await streamWriter.WriteAsync(@" on ");
await streamWriter.WriteAsync(topic.CreatedOn.ToString("MM/dd/yyyy, hh:mm tt"));
await streamWriter.WriteAsync(@" </td>
					<td>");
await streamWriter.WriteAsync(topic.CommentCountCache.ToString());
await streamWriter.WriteAsync(@"</td>
					<td>");
await streamWriter.WriteAsync(topic.ViewCountCache.ToString());
await streamWriter.WriteAsync(@"</td>
					<td>todo:  last post</td>
				</tr>
				");
 } 

await streamWriter.WriteAsync(@"
			</tbody>
		</table>
	</div>
");
	} 

	public static async Task WriteComments(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		<h2><a href=""/categories/"">");
await streamWriter.WriteAsync(viewModel.Category.Name);
await streamWriter.WriteAsync(@"</a> \ <a href=""/categories/");
await streamWriter.WriteAsync(viewModel.Category.Id.ToString());
await streamWriter.WriteAsync(@""">");
await streamWriter.WriteAsync(viewModel.Topic.Title);
await streamWriter.WriteAsync(@"</a> \ Comments</h2>
		<p>
			<strong>");
await streamWriter.WriteAsync(viewModel.Topic.Title);
await streamWriter.WriteAsync(@"</strong><br />");
await streamWriter.WriteAsync(viewModel.Topic.CreatedOn.ToString("MM/dd/yyyy, hh:mm tt"));
await streamWriter.WriteAsync(@"
		</p>
		<p>
			");
await streamWriter.WriteAsync(viewModel.Topic.Content);
await streamWriter.WriteAsync(@"
		</p>
		<table class=""table"">
			<tbody>
				");
 for (var counter = 0; counter < viewModel.Comments.Count; counter++) {
				var comment = viewModel.Comments[counter]; 
await streamWriter.WriteAsync(@"
				<tr>
					<td>
						<p><strong>Comment By: ");
await streamWriter.WriteAsync(comment.UserDisplayName);
await streamWriter.WriteAsync(@"</strong> ");
await streamWriter.WriteAsync(comment.Content);
await streamWriter.WriteAsync(@"
					</td>
				</tr>
				");
 } 

await streamWriter.WriteAsync(@"
			</tbody>
		</table>
	</div>
");
	} 

	public static async Task WriteLogin(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		<form action=""/login"" method=""POST"">
			<fieldset class=""b0 m0 p0"">
				<input type=""hidden"" name=""csrf"" value=""");
await streamWriter.WriteAsync(viewModel.CSRF);
await streamWriter.WriteAsync(@""">
				<input type=""hidden"" name=""ReturnURL"" value=""");
await streamWriter.WriteAsync(viewModel.ReturnURL);
await streamWriter.WriteAsync(@""">
				<div class=""row"">
					<div class=""col 6"">
						<label for=""username"">Username:</label>
						<input class=""card w-100"" type=""text"" name=""username"" placeholder=""username"" required />
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						<label for=""password"">Password:</label>
						<input class=""card w-100"" type=""password"" name=""password"" placeholder=""password"" required />
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						Don't have an account? <a href=""/signup?ReturnURL=");
await streamWriter.WriteAsync(viewModel.ReturnURL);

await streamWriter.WriteAsync(@""">Signup</a> : <a href=""/forgotpass"">Forgot password?</a>
					</div>
					<div class=""col 6""></div>
				</div>
				<div class=""row"">
					<div class=""col 6"">
						<input class=""btn primary"" type=""submit"" value=""Login"" />
					</div>
					<div class=""col 6""></div>
				</div>
			</fieldset>
		</form>
	</div>
");
	} 

	public static async Task WriteDocumentFooter(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""mt1 bg-lightgray p2"">
		<div class=""container"">&copy; ");
await streamWriter.WriteAsync(DateTime.Now.Year.ToString());
await streamWriter.WriteAsync(@" - ");
await streamWriter.WriteAsync(viewModel.PageTitle);

await streamWriter.WriteAsync(@"</div>
	</div>
	<script>
	</script>
</body>
</html>
");
	} 

	}
}