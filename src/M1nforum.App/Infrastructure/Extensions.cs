using M1nforum.Web.Services.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace M1nforum.Web.Infrastructure
{
	public static class DateTimeExtensions
	{
		public static DateTime Truncate(this DateTime date, long resolution)
		{
			return new DateTime(date.Ticks - (date.Ticks % resolution), date.Kind);
		}
	}

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

		//
		// note:  these messages (and most of these cookie helpers) would usually be handled with tempdata in razor pages.  We are pushing the messages to the client instead of session.
		//
		public static async Task WriteFlashMessage(this HttpContext httpContext, string type, string message)
		{
			await httpContext.WriteCookie("flash_message", type + "-" +message, false);
		}

		public static string ReadFlashMessage(this HttpContext httpContext)
		{
			var value = httpContext.ReadAndDeleteCookie("flash_message", false);
			return value;
		}

		// note:  compression on cookies is more about obfuscation than compression.  Most of the time, for short strings, the compressed version is longer.
		// Yes, it is possible to "decrypt" the message but this is not for security.  Don't pass anything sensitive.
		public static async Task WriteCookie(this HttpContext httpContext, string key, string value, bool compress = false)
		{
			if (compress)
			{
				value = await value.Compress();
			}

			httpContext.Response.Cookies.Append(key, value);
		}

		public static string ReadAndDeleteCookie(this HttpContext httpContext, string key, bool decompress = false)
		{
			var message = httpContext.Request.Cookies[key];
			httpContext.Response.Cookies.Delete(key);

			if (string.IsNullOrWhiteSpace(message))
			{
				return message ?? string.Empty;
			}

			if (decompress)
			{
				message = message.Decompress();
			}

			return message;
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

	public static class StringExtensions
	{
		public async static Task<string> Compress(this string buffer)
		{
			using (var uncompressedStream = new MemoryStream(Encoding.UTF8.GetBytes(buffer)))
			{
				using (var compressedStream = new MemoryStream())
				{
					using (var compressorStream = new DeflateStream(compressedStream, CompressionLevel.Fastest, true))
					{
						await uncompressedStream.CopyToAsync(compressorStream);
						await compressedStream.FlushAsync();
					}

					return Convert.ToBase64String(compressedStream.ToArray());
				}
			}
		}

		public static string Decompress(this string buffer)
		{
			using (var compressedStream = new MemoryStream(Convert.FromBase64String(buffer)))
			{
				using (var decompressorStream = new DeflateStream(compressedStream, CompressionMode.Decompress))
				{
					using (var decompressedStream = new MemoryStream())
					{
						decompressorStream.CopyTo(decompressedStream);

						return Encoding.UTF8.GetString(decompressedStream.ToArray());
					}
				}
			}
		}

		public static string Serialize(this object value)
		{
			return JsonSerializer.Serialize(value);
		}

		public static T Deserialize<T>(this string value)
		{
			if (string.IsNullOrEmpty(value))
			{
				return default(T);
			}

			return JsonSerializer.Deserialize<T>(value);
		}
	}

	public static class UserExtensions
	{
		public static User AsAdmin(this User user)
		{
			return Program.Cache.Business.UserIsAdmin(user) ? user : null;
		}
	}
}
