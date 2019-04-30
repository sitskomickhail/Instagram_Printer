using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace InstagramLibrary
{
    internal class HttpRequestBuilder
    {
        static HttpRequestBuilder()
        {
            System.Net.ServicePointManager.Expect100Continue = false;
        }

        private static HttpWebRequest CreateRequest(Uri uri, CookieContainer cookies = null)
        {
            var request = WebRequest.Create(uri) as HttpWebRequest;
            request.Timeout = 10000;
            request.Host = uri.Host;
            request.Accept = "*/*";
            request.KeepAlive = true;

            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.Headers.Add("Accept-Language", "ru-RU,ru;q=0.8,en-US;q=0.5,en;q=0.3");
            request.UserAgent = "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/73.0.3683.103 Safari/537.36";
            if (cookies != null) request.CookieContainer = cookies;
            return request;
        }

        public static HttpWebRequest Post(string url, CookieContainer cookies = null)
        {
            var request = CreateRequest(new Uri(url), cookies);
            request.Method = WebRequestMethods.Http.Post;
            request.ContentType = "application/x-www-form-urlencoded";
            return request;
        }

        public static HttpWebRequest Get(string url, CookieContainer cookies = null)
        {
            var request = CreateRequest(new Uri(url), cookies);
            request.Method = WebRequestMethods.Http.Get;
            return request;
        }
    }
}
