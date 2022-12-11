using System;
using System.IO;
using System.Threading.Tasks;

namespace M1nforum.Web.Templates
{ 
	public static class Views
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

	public static async Task WriteCategoriesHeader(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		<h2>Categories</h2>
		<table class=""table"">
			<thead>
				<tr>
					<th>Name</th>
					<th>Description</th>
					<th>Topics</th>
				</tr>
			</thead>
			<tbody>
");
	} 

	public static async Task WriteCategoriesRow(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
				<tr>
					<td><a href=""/categories/");
await streamWriter.WriteAsync(viewModel.Category.Id.ToString());
await streamWriter.WriteAsync(@""">");
await streamWriter.WriteAsync(viewModel.Category.Name);
await streamWriter.WriteAsync(@"</a></td>
					<td>");
await streamWriter.WriteAsync(viewModel.Category.Description);
await streamWriter.WriteAsync(@"</td>
					<td>");
await streamWriter.WriteAsync(viewModel.Category.TopicCountCache.ToString());

await streamWriter.WriteAsync(@"</td>
				</tr>
");
	} 

	public static async Task WriteCategoriesFooter(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
			</tbody>
		</table>
	</div>
");
	} 

	public static async Task WriteTopicsHeader(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		<h2><a href=""/categories/"">");
await streamWriter.WriteAsync(viewModel.Category.Name);

await streamWriter.WriteAsync(@"</a> \ Topics</h2>
		<table class=""table"">
			<thead>
				<tr>
					<th>Title</th>
					<th>Content</th>
					<th>Comment Count</th>
					<th>View Count</th>
				</tr>
			</thead>
			<tbody>
");
	} 

	public static async Task WriteTopicsRow(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
				<tr>
					<td><a href=""/categories/");
await streamWriter.WriteAsync(viewModel.Category.Id.ToString());
await streamWriter.WriteAsync(@"/topics/");
await streamWriter.WriteAsync(viewModel.Topic.Id.ToString());
await streamWriter.WriteAsync(@""">");
await streamWriter.WriteAsync(viewModel.Topic.Title);
await streamWriter.WriteAsync(@"</a></td>
					<td>");
await streamWriter.WriteAsync(viewModel.Topic.Content);
await streamWriter.WriteAsync(@"</td>
					<td>");
await streamWriter.WriteAsync(viewModel.Topic.CommentCountCache.ToString());
await streamWriter.WriteAsync(@"</td>
					<td>");
await streamWriter.WriteAsync(viewModel.Topic.ViewCountCache.ToString());

await streamWriter.WriteAsync(@"</td>
				</tr>
");
	} 

	public static async Task WriteTopicsFooter(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
			</tbody>
		</table>
	</div>
");
	} 

	public static async Task WriteCommentsHeader(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
	<div class=""container"">
		<h2><a href=""/categories/"">");
await streamWriter.WriteAsync(viewModel.Category.Name);
await streamWriter.WriteAsync(@"</a> \ <a href=""/categories/");
await streamWriter.WriteAsync(viewModel.Category_Id);
await streamWriter.WriteAsync(@""">");
await streamWriter.WriteAsync(viewModel.Topic_Title);
await streamWriter.WriteAsync(@"</a> \ Comments</h2>
		<div class=""container"">
			<p>
				<strong>Topic Title:  </strong>");
await streamWriter.WriteAsync(viewModel.Topic_Title);
await streamWriter.WriteAsync(@"
			</p>
			<p>
				<strong>Topic Content:  </strong>");
await streamWriter.WriteAsync(viewModel.Topic_Content);

await streamWriter.WriteAsync(@"
			</p>
		</div>
		<table class=""table"">
			<tbody>
");
	} 

	public static async Task WriteCommentsRow(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		await streamWriter.WriteAsync(@"
				<tr>
					<td>
						<p><strong>Comment By: ");
await streamWriter.WriteAsync(viewModel.Comment.UserDisplayName);
await streamWriter.WriteAsync(@"</strong> ");
await streamWriter.WriteAsync(viewModel.Comment.Content);

await streamWriter.WriteAsync(@"
					</td>
				</tr>
");
	} 

	public static async Task WriteCommentsFooter(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
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