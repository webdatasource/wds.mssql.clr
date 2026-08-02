using System;
using System.Text;
using System.Net;
using System.Xml.Serialization;
using WDS.MsSql.Clr.Serialization;

namespace WDS.MsSql.Clr.Server
{
    internal abstract class ServerApiXml : ServerApi
    {
        private const string _contentType = "application/xml";

        public static bool TryGet<TResponse>(Uri uri, XmlSerializer serializer, out TResponse response, out string error)
            where TResponse : class
        {
            var request = BuildRequest(uri, WebRequestMethods.Http.Get, _contentType);
            return TryRequest(request, serializer, out response, out error);
        }

        public static bool TryPost<TRequest, TResponse>(Uri uri, TRequest reqData, XmlSerializer requestSerializer, XmlSerializer responseSerializer, out TResponse response, out string error)
            where TResponse : class
        {
            var stringBuilder = new StringBuilder();
            using (var writer = new XmlStringWriter(stringBuilder, Encoding))
                requestSerializer.Serialize(writer, reqData);
            return TryPost(uri, stringBuilder.ToString(), responseSerializer, out response, out error);
        }

        private static bool TryPost<TResponse>(Uri uri, string reqData, XmlSerializer serializer, out TResponse response, out string error)
            where TResponse : class
        {
            var request = BuildRequest(uri, WebRequestMethods.Http.Post, _contentType, reqData);
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
                HandleWebException(e, out error);
                return false;
            }
        }
    }
}
