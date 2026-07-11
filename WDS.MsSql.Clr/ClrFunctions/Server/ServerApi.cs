using System;
using System.IO;
using System.Net;
using System.Text;

namespace WDS.MsSql.Clr.Server
{
    internal abstract class ServerApi
    {
        public static readonly Encoding Encoding = Encoding.UTF8;

        protected static HttpWebRequest BuildRequest(Uri uri, string method, string accept)
        {
            var request = WebRequest.CreateHttp(uri);
            request.Method = method;
            request.Accept = accept;
            request.Headers.Add(HttpRequestHeader.AcceptCharset, Encoding.HeaderName);
            request.Headers.Add("X-Tenant-Id", "default");
            return request;
        }

        protected static HttpWebRequest BuildRequest(Uri uri, string method, string contentType, string reqData)
        {
            var request = BuildRequest(uri, method, contentType);
            request.ContentType = $"{contentType}; charset={Encoding.HeaderName}";
            var bytes = Encoding.GetBytes(reqData);
            request.GetRequestStream().Write(bytes, 0, bytes.Length);
            return request;
        }

        protected static HttpWebResponse TryGetResponse(HttpWebRequest request, out string error)
        {
            while (true)
            {
                var response = (HttpWebResponse)request.GetResponse();
                switch (response.StatusCode)
                {
                    case HttpStatusCode.Accepted:
                        var location = response.Headers[HttpResponseHeader.Location];
                        if (string.IsNullOrWhiteSpace(location))
                        {
                            error = BuildApiRequestError(response.StatusCode, "The response did not include a polling location", response.Headers);
                            response.Dispose();
                            return null;
                        }

                        var responseUri = response.ResponseUri;
                        response.Dispose();
                        request = BuildRequest(new Uri(responseUri, location), WebRequestMethods.Http.Get, request.Accept);
                        continue;
                    case HttpStatusCode.OK:
                        error = null;
                        return response;
                    default:
                        error = BuildApiRequestError(response.StatusCode, GetResponseString(response), response.Headers);
                        return null;
                }
            }
        }

        protected static void HandleWebException(WebException e, out string error)
        {
            if (e.Response is HttpWebResponse webResp)
                error = BuildApiRequestError(webResp.StatusCode, GetResponseString(webResp), webResp.Headers);
            else
                error = BuildApiRequestError(null, e.Message, null);
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

        private static string BuildApiRequestError(HttpStatusCode? statusCode, string responseString, WebHeaderCollection responseHeaders)
        {
            var responseTraceId = responseHeaders?["X-Trace-Id"];
            var traceId = string.IsNullOrWhiteSpace(responseTraceId) ? "<MISSING>" : responseTraceId;
            var statusCodeStr = statusCode is null ? string.Empty : $"{(int)statusCode} ({statusCode}) - ";
            var reasonPhrase = responseString is null ? "<NULL>" : string.IsNullOrWhiteSpace(responseString) ? "<EMPTY>" : responseString;
            return $"REQUEST ERROR: {statusCodeStr}{reasonPhrase}; TraceId: {traceId}";
        }
    }
}