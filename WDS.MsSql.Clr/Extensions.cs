using System;
using System.Text;

namespace WDS.MsSql.Clr
{
    public static class Extensions
    {
        public static StringBuilder Append(this Uri baseUri, string path) => new StringBuilder(baseUri.AbsoluteUri.TrimEnd('/')).Append(path);
    }
}