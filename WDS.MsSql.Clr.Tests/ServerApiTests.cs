using System.Net;
using System.Reflection;
using NUnit.Framework;

namespace WDS.MsSql.Clr.Tests
{
    [TestFixture]
    public class ServerApiTests
    {
        private const string _traceIdHeaderName = "X-Trace-Id";

        [Test]
        public void ApiErrorIncludesResponseTraceId()
        {
            const string traceId = "88b3f3cf-3f24-41f3-b417-46013f6feea8";
            var responseHeaders = new WebHeaderCollection
            {
                { _traceIdHeaderName, traceId }
            };

            var error = BuildApiRequestError(HttpStatusCode.BadRequest, "Request details", responseHeaders);

            StringAssert.Contains($"TraceId: {traceId}", error);
            StringAssert.Contains("400 (BadRequest) - Request details", error);
        }

        [Test]
        public void ApiErrorMarksMissingResponseTraceId()
        {
            var error = BuildApiRequestError(null, "Transport error", null);

            StringAssert.Contains("TraceId: <MISSING>", error);
            StringAssert.Contains("Transport error", error);
        }

        private static string BuildApiRequestError(HttpStatusCode? statusCode, string responseString, WebHeaderCollection responseHeaders)
        {
            var serverApiType = typeof(JobConfig).Assembly.GetType("WDS.MsSql.Clr.Server.ServerApi");
            Assert.That(serverApiType, Is.Not.Null);
            var buildApiRequestError = serverApiType.GetMethod("BuildApiRequestError", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(buildApiRequestError, Is.Not.Null);
            return (string)buildApiRequestError.Invoke(null, new object[] { statusCode, responseString, responseHeaders });
        }
    }
}
