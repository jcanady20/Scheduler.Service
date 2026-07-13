using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

namespace Scheduler.Extensions;

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
    var serialized = JsonSerializer.Serialize(source);
    return JsonSerializer.Deserialize<T>(serialized);
  }

  public static string ToXml<T>(this T obj)
  {
    var xml = String.Empty;
    if (obj == null)
    {
      throw new ArgumentNullException("obj");
    }
    var serializer = new XmlSerializer(typeof(T));

    using (var textWriter = new StringWriter())
    {
      using (var xmlWriter = XmlWriter.Create(textWriter))
      {
        serializer.Serialize(xmlWriter, obj);
      }
      xml = textWriter.ToString();
    }

    return xml;
  }

  public static string ToJson(this object obj, bool addFormatting = false)
  {
    var opts = JsonSerializerOptions.Default;
    opts.WriteIndented = addFormatting;
    opts.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    opts.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    return JsonSerializer.Serialize(obj, opts);
  }
}
