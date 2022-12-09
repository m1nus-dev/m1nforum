using Microsoft.AspNetCore.Http;
using System;
using System.IO;

namespace M1nforum.Web.Infrastructure
{
	public static class Extensions
	{
		public static T Get<T>(this HttpContext httpContext, string key)
		{
			return (T)httpContext.Items[key];
		}
	}

	public static class DateTimeExtensions
	{
		public static DateTime Truncate(this DateTime date, long resolution)
		{
			return new DateTime(date.Ticks - (date.Ticks % resolution), date.Kind);
		}

		public static string GetFileTimestamp(this string path)
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
