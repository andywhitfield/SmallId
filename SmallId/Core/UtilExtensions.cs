using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SmallId.Core
{
    internal static class UtilExtensions
    {
        public static Uri GetAppPathRootedUri(this string value)
        {
            string appPath = HttpContext.Current.Request.ApplicationPath.ToLowerInvariant();
            if (!appPath.EndsWith("/"))
            {
                appPath += "/";
            }

            return new Uri(HttpContext.Current.Request.Url, appPath + value);
        }
    }
}