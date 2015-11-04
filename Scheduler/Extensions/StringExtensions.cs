using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Net;
using System.Globalization;
using System.Threading;
using System.Security.Cryptography;

namespace Scheduler.Extensions
{
	public static class StringExtensions
	{
		public static bool IsJson(this string input)
		{
			if (String.IsNullOrEmpty(input)) return false;
			input = input.Trim();
			return (input.StartsWith("{") && input.EndsWith("}")) || (input.StartsWith("[") && input.EndsWith("]"));
		}

		/// <summary>
		///	Convert a String to a Stream Object
		/// </summary>
		/// <param name="str"></param>
		/// <returns></returns>
		public static Stream ToStream(this string str)
		{
			var bytes = Encoding.UTF8.GetBytes(str ?? "");
			return new MemoryStream(bytes);
		}

		public static String Enquote(this string str)
		{
			return "\"" + str + "\"";
		}

		public static String RemoveFormatting(this string str)
		{
			if (String.IsNullOrEmpty(str))
			{
				return str;
			}
			var rgx = new Regex("[^a-zA-Z0-9]");
			return rgx.Replace(str, "");
		}

		/// <summary>
		/// true, if is valid email address
		/// from http://www.davidhayden.com/blog/dave/
		/// archive/2006/11/30/ExtensionMethodsCSharp.aspx
		/// </summary>
		/// <param name="s">email address to test</param>
		/// <returns>true, if is valid email address</returns>
		public static bool IsValidEmailAddress(this string s)
		{
			return new Regex(@"^[\w-\.]+@([\w-]+\.)+[\w-]{2,6}$").IsMatch(s);
		}

		/// <summary>
		/// Checks if url is valid. 
		/// from http://www.osix.net/modules/article/?id=586
		/// and changed to match http://localhost
		/// 
		/// complete (not only http) url regex can be found 
		/// at http://internet.ls-la.net/folklore/url-regexpr.html
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static bool IsValidUrl(this string url)
		{
			string strRegex = "^(https?://)"
		+ "?(([0-9a-z_!~*'().&=+$%-]+: )?[0-9a-z_!~*'().&=+$%-]+@)?" //user@
		+ @"(([0-9]{1,3}\.){3}[0-9]{1,3}" // IP- 199.194.52.184
		+ "|" // allows either IP or domain
		+ @"([0-9a-z_!~*'()-]+\.)*" // tertiary domain(s)- www.
		+ @"([0-9a-z][0-9a-z-]{0,61})?[0-9a-z]" // second level domain
		+ @"(\.[a-z]{2,6})?)" // first level domain- .com or .museum is optional
		+ "(:[0-9]{1,5})?" // port number- :80
		+ "((/?)|" // a slash isn't required if there is no file name
		+ "(/[0-9a-z_!~*'().;?:@&=+$,%#-]+)+/?)$";
			return new Regex(strRegex).IsMatch(url);
		}

		/// <summary>
		/// Check if url (http) is available.
		/// </summary>
		/// <param name="httpUri">url to check</param>
		/// <example>
		/// string url = "www.codeproject.com;
		/// if( !url.UrlAvailable())
		///     ...codeproject is not available
		/// </example>
		/// <returns>true if available</returns>
		public static bool UrlAvailable(this string httpUrl)
		{
			if (!httpUrl.StartsWith("http://") || !httpUrl.StartsWith("https://"))
				httpUrl = "http://" + httpUrl;
			try
			{
				HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(httpUrl);
				myRequest.Method = "GET";
				myRequest.ContentType = "application/x-www-form-urlencoded";
				HttpWebResponse myHttpWebResponse =
				   (HttpWebResponse)myRequest.GetResponse();
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Reverse the string
		/// from http://en.wikipedia.org/wiki/Extension_method
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public static string Reverse(this string input)
		{
			char[] chars = input.ToCharArray();
			Array.Reverse(chars);
			return new String(chars);
		}

		/// <summary>
		/// remove white space, not line end
		/// Useful when parsing user input such phone,
		/// price int.Parse("1 000 000".RemoveSpaces(),.....
		/// </summary>
		/// <param name="s"></param>
		/// <param name="value">string without spaces</param>
		public static string RemoveSpaces(this string s)
		{
			return s.Replace(" ", "");
		}

		/// <summary>
		/// true, if the string can be parse as Double respective Int32
		/// Spaces are not considered.
		/// </summary>
		/// <param name="s">input string</param>

		/// <param name="floatpoint">true, if Double is considered,
		/// otherwise Int32 is considered.</param>
		/// <returns>true, if the string contains only digits or float-point</returns>
		public static bool IsNumber(this string s, bool floatpoint)
		{
			int i;
			double d;
			string withoutWhiteSpace = s.RemoveSpaces();
			if (floatpoint)
			{
				return double.TryParse(withoutWhiteSpace, NumberStyles.Any, Thread.CurrentThread.CurrentUICulture, out d);
			}
			else
			{
				return int.TryParse(withoutWhiteSpace, out i);
			}
		}

		/// <summary>
		/// true, if the string contains only digits or float-point.
		/// Spaces are not considered.
		/// </summary>
		/// <param name="s">input string</param>
		/// <param name="floatpoint">true, if float-point is considered</param>
		/// <returns>true, if the string contains only digits or float-point</returns>
		public static bool IsNumberOnly(this string s, bool floatpoint)
		{
			s = s.Trim();
			if (s.Length == 0)
				return false;
			foreach (char c in s)
			{
				if (!char.IsDigit(c))
				{
					if (floatpoint && (c == '.' || c == ','))
						continue;
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Replace \r\n or \n by <br />
		/// from http://weblogs.asp.net/gunnarpeipman/archive/2007/11/18/c-extension-methods.aspx
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string Nl2Br(this string s)
		{
			return s.Replace("\r\n", "<br />").Replace("\n", "<br />");
		}

		/// <summary>
		/// from http://weblogs.asp.net/gunnarpeipman/archive/2007/11/18/c-extension-methods.aspx
		/// </summary>
		/// <param name="s"></param>
		/// <returns></returns>
		public static string MD5(this string s)
		{
			MD5CryptoServiceProvider s_md5 = new MD5CryptoServiceProvider();
			Byte[] newdata = Encoding.Default.GetBytes(s);
			Byte[] encrypted = s_md5.ComputeHash(newdata);
			return BitConverter.ToString(encrypted).Replace("-", "").ToLower();
		}

		public static string SHA256(this string s)
		{
			byte[] bytes = null;
			using (var sha = HMACSHA256.Create())
			{
				var encoding = Encoding.UTF8;
				bytes = sha.ComputeHash(encoding.GetBytes(s));
			}

			return Convert.ToBase64String(bytes);
		}

		public static IEnumerable<String> Chunk(this string str, int chunkSize)
		{
			for (int i = 0; i < str.Length; i += chunkSize)
			{
				yield return str.Substring(i, chunkSize);
			}
		}
	}
}
