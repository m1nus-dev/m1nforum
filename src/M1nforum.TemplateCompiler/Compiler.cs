using System.IO;
using System.Text.RegularExpressions;

namespace M1nforum.TemplateCompiler
{
	public class Compiler
	{
		public string ParseFile(string name, string filename)
		{
			var buffer = File.ReadAllText(filename);

			return ParseHtml(name, buffer);
		}

		public string ParseHtml(string name, string buffer)
		{
			var templateBody = buffer;
			bool templateFound;
			var csOutput = "";

			do
			{
				var startPosition = templateBody.IndexOf("<!-- template");

				if (startPosition > -1)
				{
					var endPosition = templateBody.IndexOf("<!-- /template", startPosition);
					endPosition = templateBody.IndexOf("-->", endPosition) + 3;

					var innerTemplate = templateBody.Substring(startPosition, endPosition - startPosition);

					var tempBuffer = templateBody.Substring(0, startPosition);
					templateBody = tempBuffer + templateBody.Substring(endPosition);

					var innerTemplateName = innerTemplate.Substring("<!-- template_".Length, innerTemplate.IndexOf(" ", "<!-- template_".Length) - "<!-- template_".Length);

					// trim the <!-- template_name --> stuff off.  There is probabaly a more efficient way to do this.
					innerTemplate = innerTemplate.Substring(innerTemplate.IndexOf("-->") + 3);
					innerTemplate = innerTemplate.Substring(0, innerTemplate.LastIndexOf("<!--"));

					csOutput += parseTemplateToCs(innerTemplateName, innerTemplate);

					templateFound = true;
				}
				else
				{
					templateFound = false;
				}
			} while (templateFound);


			// todo:  make this configurable
			// right now, this is created for this project only
			// dynamic using statements
			return @"using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using M1nforum.Web.Infrastructure.Validation;

namespace M1nforum.Web.Templates
{ 
	public static partial class Views
	{
" + csOutput + @"
	}
}";
		}

		// idea completely taken from John Resig - https://johnresig.com/blog/javascript-micro-templating/
		// probably a buggy implementation in c#

		public string parseTemplateToCs(string templateName, string template)
		{
			// replace the first html string, if it exists.
			string buffer = Regex.Replace(template, @"\A(?<expr>.*?)<%", html =>
			{
				string expr = html.Groups["expr"].Value;
				string cleanExpr = expr.Replace("\"", "\"\"");
				cleanExpr = "await streamWriter.WriteAsync(@\"" + cleanExpr + "\");\r\n<%";
				return cleanExpr;
			}, RegexOptions.Singleline);

			var delimiterPosition = buffer.LastIndexOf("%>");

			if (delimiterPosition == -1)
			{
				// there is no c# in this template;
				// just write it all out
				buffer = "await streamWriter.WriteAsync(@\"" + buffer.Replace("\"", "\"\"") + "\");";
			}
			else
			{
				buffer = buffer.Substring(0, delimiterPosition) + "%>\r\nawait streamWriter.WriteAsync(@\"" + buffer.Substring(delimiterPosition + 2).Replace("\"", "\"\"") + "\");";

				// replace html (the code between ending %> and starting <%)
				buffer = Regex.Replace(buffer, @"%>(?<expr>.*?)<%", html =>
				{
					string expr = html.Groups["expr"].Value;
					string cleanExpr = expr.Replace("\"", "\"\"");
					cleanExpr = "%>await streamWriter.WriteAsync(@\"" + cleanExpr + "\");\r\n<%";
					return cleanExpr;
				}, RegexOptions.Singleline);

				// replace c# expressions:  <%=
				buffer = Regex.Replace(buffer, @"<%[=:](?<expr>.*?)%>", m =>
				{
					string expr = m.Groups["expr"].Value.Trim();
					string cleanExpr = "await streamWriter.WriteAsync(" + expr + ");\r\n";
					return cleanExpr;
				}, RegexOptions.Singleline);

				// replace c# code:  <%
				buffer = Regex.Replace(buffer, @"<%(?<expr>.*?)%>", m =>
				{
					string expr = m.Groups["expr"].Value;
					string cleanExpr = expr + "\r\n";
					return cleanExpr;
				}, RegexOptions.Singleline);
			}

			return @"
	public static async Task Write" + templateName + @"(this StreamWriter streamWriter, dynamic viewModel = null) 
	{
		" + buffer + @"
	} 
";
		}
	}
}
