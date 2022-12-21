using M1nforum.Web.Services;
using System.IO;

namespace M1nforum.Web.Infrastructure
{
	public class Cache
	{
		private string _cssFileLink = null;

		public Business Business { get; set; }

		public string GetCSSFileUrl()
		{
			if (_cssFileLink == null)
			{
				var filename = DebuggingEnabled ? "wwwroot/css/app.min.css" : "wwwroot/css/app.css";
				var timestamp = File.GetLastWriteTimeUtc(filename).ToString("yyyyMMddHHmmss");
				_cssFileLink = DebuggingEnabled ? "app.css?v=" + timestamp : "app.min.css?v=" + timestamp;
			}

			return _cssFileLink;
		}

		public bool DebuggingEnabled { get; set; } = false;
	}
}
