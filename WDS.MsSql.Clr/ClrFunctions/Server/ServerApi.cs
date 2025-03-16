using System;
using System.IO;
using System.Net;
using System.Text;

namespace WDS.MsSql.Clr.Server
{
    internal abstract class ServerApi
    {
        public static readonly Encoding Encoding = Encoding.UTF8;

        private readonly string _contentType;

        protected ServerApi(string contentType)
        {
            _contentType = contentType;
        }

        public bool TryGet(Uri serverUri, string pathAndQuery, out HttpStatusCode? statusCode, out string responseString)
        {
            try
            {
                var request = BuildRequest(serverUri, pathAndQuery, WebRequestMethods.Http.Get, _contentType);
                TryGetResponse(request, out statusCode, out responseString);
                return true;
            }
            catch (WebException e)
            {
                HandleWebException(e, out statusCode, out responseString);
                return false;
            }
        }

        public bool TryPost(Uri serverUri, string pathAndQuery, string reqData, out HttpStatusCode? statusCode, out string responseString)
        {
            try
            {
                var request = BuildRequest(serverUri, pathAndQuery, WebRequestMethods.Http.Post, _contentType, reqData);
                TryGetResponse(request, out statusCode, out responseString);
                return true;
            }
            catch (WebException e)
            {
                HandleWebException(e, out statusCode, out responseString);
                return false;
            }
        }

        public static string BuildApiRequestError(HttpStatusCode? statusCode, string responseString)
        {
            var statusCodeStr = statusCode is null ? string.Empty : $"{(int)statusCode} ({statusCode}) - ";
            var reasonPhrase = responseString is null ? "<NULL>" : string.IsNullOrWhiteSpace(responseString) ? "<EMPTY>" : responseString;
            return $"REQUEST ERROR: {statusCodeStr}{reasonPhrase}";
        }

        private static HttpWebRequest BuildRequest(Uri serverUri, string pathAndQuery, string method, string accept)
        {
            var uri = new Uri(serverUri, pathAndQuery);
            var request = WebRequest.CreateHttp(uri);
            request.Method = method;
            request.Accept = accept;
            request.Headers.Add(HttpRequestHeader.AcceptCharset, Encoding.HeaderName);
            request.Headers.Add("X-Tenant-Id", "default");
            return request;
        }

        private static HttpWebRequest BuildRequest(Uri authority, string pathAndQuery, string method, string contentType, string reqData)
        {
            var request = BuildRequest(authority, pathAndQuery, method, contentType);
            request.ContentType = $"{contentType}; charset={Encoding.HeaderName}";
            var bytes = Encoding.GetBytes(reqData);
            request.GetRequestStream().Write(bytes, 0, bytes.Length);
            return request;
        }

        private static void TryGetResponse(HttpWebRequest request, out HttpStatusCode? statusCode, out string responseString)
        {
            var retries = 10;
            while (true)
            {
                using var response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.Accepted) // Waiting for a result
                    continue;
                if (response.StatusCode == HttpStatusCode.InternalServerError && retries-- > 0) // Retrying on an internal service error
                    continue;
                statusCode = response.StatusCode;
                responseString = GetResponseString(response);
                return;
            }
        }

        private static void HandleWebException(WebException e, out HttpStatusCode? statusCode, out string responseString)
        {
            if (e.Response is HttpWebResponse webResp)
            {
                statusCode = webResp.StatusCode;
                responseString = GetResponseString(webResp);
            }
            else
            {
                statusCode = null;
                responseString = e.Message;
            }
        }

        private static string GetResponseString(HttpWebResponse webResp)
        {
            using var webRespStream = webResp.GetResponseStream();
            if (webRespStream == null || webRespStream == Stream.Null)
                return string.Empty;
            using var streamReader = new StreamReader(webRespStream);
            var response = streamReader.ReadToEnd();
            return string.IsNullOrWhiteSpace(response) ? string.Empty : response;
        }
    }
}