﻿using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace M1nforum.Web.Infrastructure
{
	public static class HttpExtensions
	{
		// todo:  this helper automatically respond 200 - make generic
		public async static Task<StreamWriter> StartResponse(this HttpContext httpContext, string contentType)
		{
			var streamWriter = new StreamWriter(httpContext.Response.Body);

			// await streamWriter.WriteAsync("content-type", contentType);
			httpContext.Response.StatusCode = 200;
			httpContext.Response.Headers.Append("Content-Type", contentType);

			return streamWriter;
		}

		public static async Task<StreamWriter> StartHtmlResponse(this HttpContext httpContext)
		{
			return await httpContext.StartResponse("text/html");
		}

		public static void WriteFlashMessage(this HttpContext httpContext, string type, string message)
		{
			httpContext.Response.Cookies.Append("flash_message", type + "-" +message);
		}

		public static string ReadFlashMessage(this HttpContext httpContext)
		{
			var message = httpContext.Request.Cookies["flash_message"];
			httpContext.Response.Cookies.Delete("flash_message");

			return message ?? string.Empty;
		}
	}

	public static class DateTimeExtensions
	{
		public static DateTime Truncate(this DateTime date, long resolution)
		{
			return new DateTime(date.Ticks - (date.Ticks % resolution), date.Kind);
		}

		// todo:  make this generic
		public static string GetCSSFileTimestamp(this string path)
		{
			var timestamp = Program.Cache.CSSTimestamp ?? (Program.Cache.CSSTimestamp = File.GetLastWriteTimeUtc(path).ToString("yyyyMMddHHmmss"));
			return timestamp;
		}
	}

	public static class IdGenerator
	{
		public static ulong NewId()
		{
			return ulong.Parse(DateTime.UtcNow.ToString("yyMMddHHmmssfffffff")); // y2k all over again!  :O
		}

		// for later if the other id doesnt work out
		public static Guid GenerateComb()
		{
			byte[] guidArray = Guid.NewGuid().ToByteArray();

			DateTime baseDate = new DateTime(1900, 1, 1);
			DateTime now = DateTime.UtcNow;

			// Get the days and milliseconds which will be used to build the byte string 
			TimeSpan days = new TimeSpan(now.Ticks - baseDate.Ticks);
			TimeSpan msecs = now.TimeOfDay;

			// Convert to a byte array 
			// Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
			byte[] daysArray = BitConverter.GetBytes(days.Days);
			byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

			// Reverse the bytes to match SQL Servers ordering 
			Array.Reverse(daysArray);
			Array.Reverse(msecsArray);

			// Copy the bytes into the guid 
			Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
			Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

			return new Guid(guidArray);
		}
	}
}
