using NUnit.Framework;

namespace WDS.MsSql.Clr.Tests
{
    [TestFixture]
    public class DataContractsTests
    {
        [Test]
        public void JobConfigParseTest()
        {
            const string serverConnectionString = "wds://local.host:2807";
            const string serverUrl = "http://local.host:2807/";
            const string startUrl1 = "http://example.com";
            const string startUrl2 = "http://example2.com";
            const string startUrl3 = "http://example3.com";
            const string jobName = "jobName";

            var jobConfig = JobConfig.Parse($"JobName: {jobName}; Server: {serverConnectionString}; StartUrls: {startUrl1}, {startUrl2}");
            Assert.AreEqual(jobName, jobConfig.JobName);
            Assert.AreEqual(serverUrl, jobConfig.Server.Url);
            Assert.AreEqual(startUrl1, jobConfig.StartUrls[0]);
            Assert.AreEqual(startUrl2, jobConfig.StartUrls[1]);

            jobConfig.AddStartUrl(startUrl3);
            Assert.AreEqual(startUrl3, jobConfig.StartUrls[2]);
        }

        [Test]
        public void RequestDefaultsConfigAddHeaderTest()
        {
            const string header1Name = "header1";
            const string header1Value1 = "header1value1";
            const string header1Value2 = "header1value2";

            const string header2Name = "header2";
            const string header2Value1 = "header2value1";
            const string header2Value2 = "header2value2";

            var requestDefaultsConfig = HeadersConfig.Parse(string.Empty);
            requestDefaultsConfig.AppendHeader(header1Name, header1Value1);
            Assert.AreEqual(1, requestDefaultsConfig.DefaultRequestHeaders.Length);
            Assert.AreEqual(1, requestDefaultsConfig.DefaultRequestHeaders[0].Values.Length);
            Assert.AreEqual(header1Name, requestDefaultsConfig.DefaultRequestHeaders[0].Name);
            Assert.AreEqual(header1Value1, requestDefaultsConfig.DefaultRequestHeaders[0].Values[0]);

            requestDefaultsConfig.AppendHeader(header1Name, header1Value2);
            Assert.AreEqual(1, requestDefaultsConfig.DefaultRequestHeaders.Length);
            Assert.AreEqual(2, requestDefaultsConfig.DefaultRequestHeaders[0].Values.Length);
            Assert.AreEqual(header1Name, requestDefaultsConfig.DefaultRequestHeaders[0].Name);
            Assert.AreEqual(header1Value2, requestDefaultsConfig.DefaultRequestHeaders[0].Values[1]);

            var header2 = HttpHeader.Parse($"Name: {header2Name}; Values: {header2Value1}, {header2Value2}");
            requestDefaultsConfig.AddHeader(header2);
            Assert.AreEqual(2, requestDefaultsConfig.DefaultRequestHeaders.Length);
            Assert.AreEqual(2, requestDefaultsConfig.DefaultRequestHeaders[1].Values.Length);
            Assert.AreEqual(header2Name, requestDefaultsConfig.DefaultRequestHeaders[1].Name);
            Assert.AreEqual(header2Value1, requestDefaultsConfig.DefaultRequestHeaders[1].Values[0]);
            Assert.AreEqual(header2Value2, requestDefaultsConfig.DefaultRequestHeaders[1].Values[1]);
        }

        [Test]
        public void JobRestartConfigParseTest([Values] JobRestartModes mode)
        {
            var jobRestartConfig = JobRestartConfig.Parse(string.Empty);
            Assert.AreEqual(JobRestartModes.Continue, jobRestartConfig.RestartMode);

            jobRestartConfig = JobRestartConfig.Parse($"JobRestartMode: {mode}");
            Assert.AreEqual(mode, jobRestartConfig.RestartMode);
        }

        [Test]
        public void HttpsConfigParseTest()
        {
            var httpsConfig = HttpsConfig.Parse("SuppressHttpsCertificateValidation: true");
            Assert.IsTrue(httpsConfig.SuppressHttpsCertificateValidation);

            httpsConfig = HttpsConfig.Parse(string.Empty);
            Assert.IsFalse(httpsConfig.SuppressHttpsCertificateValidation);
        }

        [Test]
        public void CookiesConfigParseTest()
        {
            var cookiesConfig = CookiesConfig.Parse("UseCookies: true");
            Assert.IsTrue(cookiesConfig.UseCookies);

            cookiesConfig = CookiesConfig.Parse(string.Empty);
            Assert.IsFalse(cookiesConfig.UseCookies);
        }

        [Test]
        public void ProxiesConfigParseTest()
        {
            var proxiesConfig = ProxiesConfig.Parse(string.Empty);
            Assert.IsFalse(proxiesConfig.UseProxy);
            Assert.IsFalse(proxiesConfig.SendOvertRequestsOnProxiesFailure);
            Assert.IsNull(proxiesConfig.IterateProxyResponseCodes);

            const string iterateProxyResponseCodes = "200, 404";
            proxiesConfig = ProxiesConfig.Parse($"UseProxy: true; SendOvertRequestsOnProxiesFailure: true; IterateProxyResponseCodes: {iterateProxyResponseCodes}");
            Assert.IsTrue(proxiesConfig.UseProxy);
            Assert.IsTrue(proxiesConfig.SendOvertRequestsOnProxiesFailure);
            Assert.AreEqual(iterateProxyResponseCodes, proxiesConfig.IterateProxyResponseCodes);

            const string proxyProtocol1 = "socks5";
            const string proxyHost1 = "proxyHost1";
            const int proxyPort1 = 8080;
            const string proxyUserName1 = "proxyUserName1";
            const string proxyPassword1 = "proxyPassword1";
            const int proxyConnectionsLimit1 = 10;
            const string proxyAvailableHosts1 = "host1, host2";

            proxiesConfig = proxiesConfig.AddProxy(proxyProtocol1, proxyHost1, proxyPort1, proxyUserName1, proxyPassword1, proxyConnectionsLimit1, proxyAvailableHosts1);
            Assert.AreEqual(1, proxiesConfig.Proxies.Length);
            Assert.AreEqual(proxyProtocol1, proxiesConfig.Proxies[0].Protocol);
            Assert.AreEqual(proxyHost1, proxiesConfig.Proxies[0].Host);
            Assert.AreEqual(proxyPort1, proxiesConfig.Proxies[0].Port);
            Assert.AreEqual(proxyUserName1, proxiesConfig.Proxies[0].UserName);
            Assert.AreEqual(proxyPassword1, proxiesConfig.Proxies[0].Password);
            Assert.AreEqual(proxyConnectionsLimit1, proxiesConfig.Proxies[0].ConnectionsLimit);
            Assert.AreEqual(2, proxiesConfig.Proxies[0].AvailableHosts.Length);
            Assert.AreEqual("host1", proxiesConfig.Proxies[0].AvailableHosts[0]);
            Assert.AreEqual("host2", proxiesConfig.Proxies[0].AvailableHosts[1]);

            const string proxyProtocol2 = "https";
            const string proxyHost2 = "proxyHost2";
            const int proxyPort2 = 8081;
            const string proxyUserName2 = "proxyUserName2";
            const string proxyPassword2 = "proxyPassword2";
            const int proxyConnectionsLimit2 = 20;
            const string availableHost1 = "host3";
            const string availableHost2 = "host4";
            const string availableHost3 = "host5";

            var proxyConfig = ProxyConfig.Parse($"Protocol: {proxyProtocol2}; Host: {proxyHost2}; Port: {proxyPort2}; UserName: {proxyUserName2}; Password: {proxyPassword2}; ConnectionsLimit: {proxyConnectionsLimit2}; AvailableHosts: {availableHost1}, {availableHost2}, {availableHost3};");
            proxyConfig.AddAvailableHost(availableHost3);
            proxiesConfig = proxiesConfig.AddProxyConfig(proxyConfig);
            Assert.AreEqual(2, proxiesConfig.Proxies.Length);
            Assert.AreEqual(proxyProtocol2, proxiesConfig.Proxies[1].Protocol);
            Assert.AreEqual(proxyHost2, proxiesConfig.Proxies[1].Host);
            Assert.AreEqual(proxyPort2, proxiesConfig.Proxies[1].Port);
            Assert.AreEqual(proxyUserName2, proxiesConfig.Proxies[1].UserName);
            Assert.AreEqual(proxyPassword2, proxiesConfig.Proxies[1].Password);
            Assert.AreEqual(proxyConnectionsLimit2, proxiesConfig.Proxies[1].ConnectionsLimit);
            Assert.AreEqual(3, proxiesConfig.Proxies[1].AvailableHosts.Length);
            Assert.AreEqual(availableHost1, proxiesConfig.Proxies[1].AvailableHosts[0]);
            Assert.AreEqual(availableHost2, proxiesConfig.Proxies[1].AvailableHosts[1]);
            Assert.AreEqual(availableHost3, proxiesConfig.Proxies[1].AvailableHosts[2]);
        }

        [Test]
        public void DownloadErrorHandlingPolicyParseTest([Values] DownloadErrorHandlingPolicies policy)
        {
            var downloadErrorHandlingPolicy = DownloadErrorHandlingPolicy.Parse(string.Empty);
            Assert.AreEqual(DownloadErrorHandlingPolicies.Skip, downloadErrorHandlingPolicy.Policy);
            Assert.IsNull(downloadErrorHandlingPolicy.RetryPolicyParams);

            const int retryDelayMs = 1000;
            const int retriesLimit = 3;

            downloadErrorHandlingPolicy = DownloadErrorHandlingPolicy.Parse($"ErrorHandlingPolicy: {policy}");
            if (policy == DownloadErrorHandlingPolicies.Retry)
                downloadErrorHandlingPolicy.RetryPolicyParams = RetryPolicyParams.Parse($"RetryDelayMs: {retryDelayMs}; RetriesLimit: {retriesLimit}");

            Assert.AreEqual(policy, downloadErrorHandlingPolicy.Policy);
            if (policy == DownloadErrorHandlingPolicies.Retry)
            {
                Assert.AreEqual(retryDelayMs, downloadErrorHandlingPolicy.RetryPolicyParams.RetryDelayMs);
                Assert.AreEqual(retriesLimit, downloadErrorHandlingPolicy.RetryPolicyParams.RetriesLimit);
            }
        }

        [Test]
        public void CrawlersProtectionBypassParseTest()
        {
            var crawlersProtectionBypass = CrawlersProtectionBypass.Parse(string.Empty);
            Assert.IsNull(crawlersProtectionBypass.MaxResponseSizeKb);
            Assert.IsNull(crawlersProtectionBypass.MaxRedirectHops);
            Assert.IsNull(crawlersProtectionBypass.RequestTimeoutSec);
            Assert.IsNull(crawlersProtectionBypass.CrawlDelays);

            const int maxResponseSizeKb = 1000;
            const int maxRedirectHops = 3;
            const int requestTimeoutSec = 1;

            const string crawlDelayHost1 = "host1";
            const string crawlDelayDelay1 = "0";
            const string crawlDelayHost2 = "host2";
            const string crawlDelayDelay2 = "1-5";

            crawlersProtectionBypass = CrawlersProtectionBypass.Parse($"MaxResponseSizeKb: {maxResponseSizeKb}; MaxRedirectHops: {maxRedirectHops}; RequestTimeoutSec: {requestTimeoutSec}");
            var crawlDelay = CrawlDelay.Parse($"Host: {crawlDelayHost1}; Delay: {crawlDelayDelay1}");
            crawlersProtectionBypass.AddCrawlDelay(crawlDelay);
            crawlersProtectionBypass.AddDelay(crawlDelayHost2, crawlDelayDelay2);

            Assert.AreEqual(maxResponseSizeKb, crawlersProtectionBypass.MaxResponseSizeKb);
            Assert.AreEqual(maxRedirectHops, crawlersProtectionBypass.MaxRedirectHops);
            Assert.AreEqual(requestTimeoutSec, crawlersProtectionBypass.RequestTimeoutSec);
            Assert.AreEqual(2, crawlersProtectionBypass.CrawlDelays.Length);
            Assert.AreEqual(crawlDelayHost1, crawlersProtectionBypass.CrawlDelays[0].Host);
            Assert.AreEqual(crawlDelayDelay1, crawlersProtectionBypass.CrawlDelays[0].Delay);
            Assert.AreEqual(crawlDelayHost2, crawlersProtectionBypass.CrawlDelays[1].Host);
            Assert.AreEqual(crawlDelayDelay2, crawlersProtectionBypass.CrawlDelays[1].Delay);
        }
    }
}