using System;

namespace M1nforum.Web.Infrastructure.Exceptions
{
	// todo:  is there a better way to do this?
	public class PageNotFoundException : Exception 
	{
		public PageNotFoundException(string name) : base(name) { }
	}
}
