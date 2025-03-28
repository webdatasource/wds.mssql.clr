using System;
using System.Net;

namespace WDS.MsSql.Clr.Server
{
    internal class ServerApiTxt : ServerApi
    {
        private const string _contentType = "text/plain";

        public static bool TryGet(Uri serverUri, string pathAndQuery, out string response, out string error)
        {
            var request = BuildRequest(serverUri, pathAndQuery, WebRequestMethods.Http.Get, _contentType);
            return TryRequest(request, out response, out error);
        }
        
        private static bool TryRequest(HttpWebRequest request, out string response, out string error)
        {
            try
            {
                using var httpWebResponse = TryGetResponse(request, out error);
                if (httpWebResponse != null)
                {
                    response = GetResponseString(httpWebResponse);
                    return true;
                }

                response = null;
                return false;
            }
            catch (WebException e)
            {
                response = null;
                HandleWebException(e, out error);
                return false;
            }
        }
    }
}