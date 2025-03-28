using System;
using System.IO;
using System.Net;
using System.Text;

namespace WDS.MsSql.Clr.Server
{
    internal abstract class ServerApi
    {
        public static readonly Encoding Encoding = Encoding.UTF8;

        protected static HttpWebRequest BuildRequest(Uri serverUri, string pathAndQuery, string method, string accept)
        {
            var uri = new Uri(serverUri, pathAndQuery);
            var request = WebRequest.CreateHttp(uri);
            request.Method = method;
            request.Accept = accept;
            request.Headers.Add(HttpRequestHeader.AcceptCharset, Encoding.HeaderName);
            request.Headers.Add("X-Tenant-Id", "default");
            return request;
        }

        protected static HttpWebRequest BuildRequest(Uri authority, string pathAndQuery, string method, string contentType, string reqData)
        {
            var request = BuildRequest(authority, pathAndQuery, method, contentType);
            request.ContentType = $"{contentType}; charset={Encoding.HeaderName}";
            var bytes = Encoding.GetBytes(reqData);
            request.GetRequestStream().Write(bytes, 0, bytes.Length);
            return request;
        }

        protected static HttpWebResponse TryGetResponse(HttpWebRequest request, out string error)
        {
            var retries = 10;
            while (true)
            {
                var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.Accepted) // Waiting for a result
                    continue;
                if (response.StatusCode == HttpStatusCode.InternalServerError && retries-- > 0) // Retrying on an internal service error
                    continue;
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    error = null;
                    return response;
                }

                error = BuildApiRequestError(response.StatusCode, GetResponseString(response));
                return null;
            }
        }

        protected static void HandleWebException(WebException e, out string error)
        {
            if (e.Response is HttpWebResponse webResp)
                error = BuildApiRequestError(webResp.StatusCode, GetResponseString(webResp));
            else
                error = BuildApiRequestError(null, e.Message);
        }

        protected static string GetResponseString(HttpWebResponse webResp)
        {
            using var webRespStream = webResp.GetResponseStream();
            if (webRespStream == null || webRespStream == Stream.Null)
                return string.Empty;
            using var streamReader = new StreamReader(webRespStream);
            var response = streamReader.ReadToEnd();
            return string.IsNullOrWhiteSpace(response) ? string.Empty : response;
        }

        private static string BuildApiRequestError(HttpStatusCode? statusCode, string responseString)
        {
            var statusCodeStr = statusCode is null ? string.Empty : $"{(int)statusCode} ({statusCode}) - ";
            var reasonPhrase = responseString is null ? "<NULL>" : string.IsNullOrWhiteSpace(responseString) ? "<EMPTY>" : responseString;
            return $"REQUEST ERROR: {statusCodeStr}{reasonPhrase}";
        }
    }
}