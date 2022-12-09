﻿using Microsoft.Extensions.Primitives;
using System.Net;
using System;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace M1nforum.Web.Infrastructure
{
	public static class Caching
	{
		private static void AddLastModifiedHeader(this HttpContext httpContext, DateTime lastModifiedUTC)
		{
			httpContext.Response.Headers.CacheControl = "must-revalidate, private";
			httpContext.Response.Headers.Expires = "-1";
			httpContext.Response.Headers.Add("Last-Modified", lastModifiedUTC.ToString("R"));
		}

		private static bool IsClientCached(this HttpContext httpContext, DateTime contentLastModified)
		{
			string header = httpContext.Request.Headers["If-Modified-Since"];

			if (header != null)
			{
				DateTime headerLastModified;

				if (DateTime.TryParse(header, out headerLastModified))
				{
					headerLastModified = headerLastModified.ToUniversalTime().Truncate(TimeSpan.TicksPerSecond);
					contentLastModified = contentLastModified.Truncate(TimeSpan.TicksPerSecond);

					return headerLastModified <= contentLastModified;
				}
			}

			return false;
		}

		private static void AddCachePageHeader(this HttpContext httpContext)
		{
			httpContext.Response.Headers.Add(HttpStatusCode.NotModified.ToString(), "Page not modified.");
		}

		public static bool CacheContent(this HttpContext httpContext, DateTime contentLastModified)
		{
			if (httpContext.IsClientCached(contentLastModified))
			{
				httpContext.AddCachePageHeader();
				httpContext.Response.StatusCode = 304;
				return true;
			}
			else
			{
				httpContext.AddLastModifiedHeader(contentLastModified);
				return false;
			}
		}
	}
}