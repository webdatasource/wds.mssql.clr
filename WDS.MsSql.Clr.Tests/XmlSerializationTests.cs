using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;

namespace WDS.MsSql.Clr.Tests
{
    [TestFixture]
    public class XmlSerializationTests
    {
        [Test]
        [TestCase("192.168.1.10:8081")]
        [TestCase("server.url")]
        public void JobConfigXmlSerializeTest(string serverAuthority)
        {
            var serverConnectionString = $"wds://{serverAuthority}";
            var serverUrl = $"http://{serverAuthority}/";
            var startUrls = new[] { "http://start1.url/", "http://start2.url/" };

            var jobConfig = JobConfig.Parse($"Server: {serverConnectionString}; StartUrls: {string.Join(", ", startUrls)}");
            Assert.AreEqual(serverUrl, jobConfig.Server.Url);
            Assert.AreEqual(startUrls.Length, jobConfig.StartUrls.Intersect(startUrls).Count());
            var xml = jobConfig.ToString();

            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var declaration = doc.ChildNodes.OfType<XmlDeclaration>().FirstOrDefault();
            Assert.AreEqual(Encoding.UTF8.HeaderName, declaration?.Encoding);
        }
    }
}