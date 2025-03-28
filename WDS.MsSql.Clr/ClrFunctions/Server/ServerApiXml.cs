using System;
using System.Net;
using System.Xml.Serialization;

namespace WDS.MsSql.Clr.Server
{
    internal class ServerApiXml : ServerApi
    {
        private const string _contentType = "application/xml";

        public static bool TryGet<T>(Uri serverUri, string pathAndQuery, XmlSerializer serializer, out T response, out string error)
            where T : class
        {
            var request = BuildRequest(serverUri, pathAndQuery, WebRequestMethods.Http.Get, _contentType);
            return TryRequest(request, serializer, out response, out error);
        }

        public static bool TryPost<T>(Uri serverUri, string pathAndQuery, string reqData, XmlSerializer serializer, out T response, out string error)
            where T : class
        {
            var request = BuildRequest(serverUri, pathAndQuery, WebRequestMethods.Http.Post, _contentType, reqData);
            return TryRequest(request, serializer, out response, out error);
        }

        private static bool TryRequest<T>(HttpWebRequest request, XmlSerializer serializer, out T response, out string error)
            where T : class
        {
            try
            {
                using var httpWebResponse = TryGetResponse(request, out error);
                using var responseStream = httpWebResponse?.GetResponseStream();
                if (responseStream != null)
                {
                    response = (T)serializer.Deserialize(responseStream);
                    return true;
                }

                response = null;
                return false;
            }
            catch (WebException e)
            {
                response = null;
                ServerApi.HandleWebException(e, out error);
                return false;
            }
        }
    }
}