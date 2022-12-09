using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace M1nforum.Web.Infrastructure
{
	public class TemplateEngine
	{
		public static Dictionary<string, CompiledTemplatePart[]> CompiledTemplatePartsCache = new Dictionary<string, CompiledTemplatePart[]>();

		public class CompiledTemplatePart
		{
			public string Key { get; set; }
			public string Value { get; set; }
		}

		public TemplateEngine ParseFile(string name, string filename)
		{
			if (CompiledTemplatePartsCache.ContainsKey(name))
			{
				// do not load the same template twice
				return this;
			}

			var buffer = File.ReadAllText(filename);

			return ParseHtml(name, buffer);
		}


		public TemplateEngine ParseHtml(string name, string buffer)
		{
			if (CompiledTemplatePartsCache.ContainsKey(name))
			{
				// do not load the same template twice
				return this;
			}

			var templateBody = buffer;
			bool templateFound;

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

					Compile(innerTemplateName, innerTemplate);

					templateFound = true;
				}
				else
				{
					templateFound = false;
				}
			} while (templateFound);

			Compile(name, templateBody);

			return this;
		}

		private void Compile(string name, string templateBody)
		{
			var compiledParts = new List<CompiledTemplatePart>();
			var regex = new Regex("<!-- \\[(.*?)\\] -->");

			var matches = regex.Split(templateBody);

			for (var counter = 0; counter < matches.Length; counter++)
			{
				var match = matches[counter];

				if (match.StartsWith("[") && match.EndsWith("]"))
				{
					// assume this is a variable
					compiledParts.Add(new CompiledTemplatePart() { Key = match.Substring(0, match.Length - 1).Substring(1) });
				}
				else
				{
					// assume this is not a variable
					compiledParts.Add(new CompiledTemplatePart() { Value = match });
				}
			}

			CompiledTemplatePartsCache[name] = compiledParts.ToArray();
		}

		public async Task Render(StreamWriter streamWriter, string templateName, IDictionary<string, string> variables)
		{
			try
			{
				var compiledTemplateParts = CompiledTemplatePartsCache[templateName];
				for (var counter = 0; counter < compiledTemplateParts.Length; counter++)
				{
					var compiledTemplatePart = compiledTemplateParts[counter];

					if (compiledTemplatePart.Key == null)
					{
						await streamWriter.WriteAsync(compiledTemplatePart.Value);
					}
					else
					{
						await streamWriter.WriteAsync(variables.FirstOrDefault(v => v.Key == compiledTemplatePart.Key).Value ?? string.Empty);
					}
				}
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task RenderLoop<T>(StreamWriter streamWriter, string templateName, List<T> datas, Func<T, IDictionary<string, string>> getData)
		{
			for (var counter = 0; counter < datas.Count; counter++)
			{
				var data = datas[counter];

				await Render(streamWriter, templateName, getData(data));
			}
		}
	}
}
