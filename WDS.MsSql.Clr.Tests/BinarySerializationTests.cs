using System;
using System.IO;
using System.Text;
using NUnit.Framework;

namespace WDS.MsSql.Clr.Tests
{
    [TestFixture]
    public class BinarySerializationTests
    {
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
    }
}