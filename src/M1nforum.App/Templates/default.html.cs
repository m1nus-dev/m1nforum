using System;
using System.IO;
using System.Threading.Tasks;

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
await streamWriter.WriteAsync(viewModel.Title);
await streamWriter.WriteAsync(@"</title>
	<link rel=""stylesheet"" href=""/css/");
await streamWriter.WriteAsync(viewModel.CSSFilename);
await streamWriter.WriteAsync(@""" />
</head>
<body>
	<nav class=""nav"" tabindex=""-1"" onclick=""this.focus()"">
		<div class=""container"">
			<a class=""pagename current"" href=""/"">");
await streamWriter.WriteAsync(viewModel.SiteName);

await streamWriter.WriteAsync(@"</a>
			<a href=""/"">Home</a>
		</div>
	</nav>
");
	} 

	public static async Task WritePageHeader(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		<h1 class=""mb0"">");
await streamWriter.WriteAsync(viewModel.Header);
await streamWriter.WriteAsync(@"</h1>
		<p class=""mt0"">");
await streamWriter.WriteAsync(viewModel.Subheader);

await streamWriter.WriteAsync(@"</p>
	</div>
");
	} 

	public static async Task WriteCategories(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		<h2>Categories</h2>
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
		<div class=""container"">
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
		</div>
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

	public static async Task WriteDocumentFooter(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""mt1 bg-gray p1"">
		<div class=""container"">");
await streamWriter.WriteAsync(viewModel.Title);
await streamWriter.WriteAsync(@" | generated on ");
await streamWriter.WriteAsync(viewModel.GeneratedOn);

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