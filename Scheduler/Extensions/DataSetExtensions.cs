using System.Data;
using System.IO;
using System.IO.Compression;

namespace Scheduler.Extensions
{
	public static class DataSetExtensions
	{
		public static void WriteToCompressedFile(this DataSet ds, string filePath)
		{
			using (var dsStream = new MemoryStream())
			{
				ds.WriteXml(dsStream, XmlWriteMode.WriteSchema);

				dsStream.Seek(0, SeekOrigin.Begin);

				using (var compressedFileStream = File.Create(filePath))
				{
					using (var gzStream = new GZipStream(compressedFileStream, CompressionMode.Compress))
					{
						dsStream.CopyTo(gzStream);
					}
				}
			}
		}

		public static void LoadFromCompressedFile(this DataSet ds, string filePath)
		{
			using (var inFile = new FileInfo(filePath).OpenRead())
			{
				using (var ms = new MemoryStream())
				{
					using (var gzStream = new GZipStream(inFile, CompressionMode.Decompress))
					{
						gzStream.CopyTo(ms);
					}
					ms.Seek(0, SeekOrigin.Begin);

					ds.ReadXml(ms);
				}
			}
		}
	}
}
