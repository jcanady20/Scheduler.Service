using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace Scheduler.Extensions
{
	public static class ObjectExtensions
	{
		/// <summary>
		/// Perform a deep Copy of the object.
		/// </summary>
		/// <typeparam name="T">The type of object being copied.</typeparam>
		/// <param name="source">The object instance to copy.</param>
		/// <returns>The copied object.</returns>
		public static T Clone<T>(this T source)
		{
			// Don't serialize a null object, simply return the default for that object
			if (Object.ReferenceEquals(source, null))
			{
				return default(T);
			}
			var serialized = JsonConvert.SerializeObject(source);
			return JsonConvert.DeserializeObject<T>(serialized);
		}

		public static string ToJson(this object obj, bool addFormating = false)
		{
			var serializer = new JsonStringify(obj);
			if (addFormating)
			{
				serializer.Formatting = Formatting.Indented;

			}
			return serializer.ToString();
		}

		internal class JsonStringify
		{
			private JsonStringify()
			{
				this.Formatting = Newtonsoft.Json.Formatting.None;
			}
			public JsonStringify(object data)
				: this()
			{
				this.Data = data;
			}
			private object Data { get; set; }
			public Formatting Formatting { get; set; }
			public override string ToString()
			{
				var objectString = string.Empty;
				if (Data != null)
				{
					var ms = new System.IO.MemoryStream();

					System.IO.TextWriter tw = new System.IO.StreamWriter(ms);

					using (var writer = new JsonTextWriter(tw) { Formatting = Formatting })
					{
						var serializer = JsonSerializer.Create();
						serializer.Serialize(writer, Data);
						writer.Flush();
					}

					objectString = ASCIIEncoding.ASCII.GetString(ms.ToArray());
				}
				return objectString;
			}
		}
	}
}
