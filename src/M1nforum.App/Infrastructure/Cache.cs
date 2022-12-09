using M1nforum.Web.Services;

namespace M1nforum.Web.Infrastructure
{
	public class Cache
	{
		public Business Business { get; set; }

		public string CSSTimestamp { get; set; }

		public bool DebuggingEnabled { get; set; } = false;
	}
}
