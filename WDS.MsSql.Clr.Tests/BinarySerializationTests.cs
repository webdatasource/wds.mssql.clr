using System;
using System.IO;
using System.Reflection;
using System.Text;
using NUnit.Framework;

namespace WDS.MsSql.Clr.Tests
{
    [TestFixture]
    public class BinarySerializationTests
    {
        [Test]
        public void TryReadStringReadsValueAndFallsBackAtEndOfStream()
        {
            const string value = "serialized value";
            const string defaultValue = "default value";

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                    writer.Write(value);

                stream.Position = 0;
                using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    Assert.AreEqual(value, TryReadString(reader, defaultValue));
                    Assert.AreEqual(defaultValue, TryReadString(reader, defaultValue));
                }
            }
        }

        [Test]
        public void ScrapeMultipleParams()
        {
            var server = ServerConfig.Parse("wds://localhost:2807");
            var downloadTask = new DownloadTask
            {
                Server = server,
                Id = Guid.NewGuid().ToString(),
                Url = "https://example.com"
            };
            var serialized = new ScrapeMultipleParams().BindDownloadTask(downloadTask)
                .AddScrapeParams("name", "selector", "attributeName");

            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream, Encoding.UTF8, true))
                    serialized.Write(writer);

                var deserialized = new ScrapeMultipleParams();
                stream.Position = 0;
                using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
                    deserialized.Read(reader);

                deserialized.Validate();
            }
        }

        private static string TryReadString(BinaryReader reader, string defaultValue)
        {
            var extensionsType = typeof(JobConfig).Assembly.GetType("WDS.MsSql.Clr.Serialization.BinarySerializationExtensions");
            Assert.That(extensionsType, Is.Not.Null);
            var tryReadString = extensionsType.GetMethod("TryReadString", BindingFlags.Public | BindingFlags.Static);
            Assert.That(tryReadString, Is.Not.Null);
            return (string)tryReadString.Invoke(null, new object[] { reader, defaultValue });
        }
    }
}
