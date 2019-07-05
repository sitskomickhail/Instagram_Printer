using InstagramLibrary.InstTemplates;
using InstaLog;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading.Tasks;

namespace InstagramLibrary
{
    public class InstRequest
    {
        private CookieContainer _cookieContainer;
        private LogIO.Logging logging = new LogIO.Logging(LogIO.WriteLog);

        public string Hashtag { get; set; }

        public InstRequest()
        {
            _cookieContainer = new CookieContainer();
        }

        public async Task<HashResult> FindHashtagPhotos()
        {
            try
            {
                var request = HttpRequestBuilder.Get($"https://www.instagram.com/explore/tags/{Hashtag}/?__a=1", _cookieContainer);
                request.Referer = $"https://www.instagram.com/explore/tags/{Hashtag}/";
                request.Headers["X-Requested-With"] = "XMLHttpRequest";
                request.Headers["X-IG-App-ID"] = "936619743392459";
                request.Headers["X-Instagram-GIS"] = "07872401bf58d36857235616ae5cc596";
                request.AllowAutoRedirect = false;
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    _cookieContainer.Add(response.Cookies); // may be exep
                    using (var responseStream = response.GetResponseStream())
                    using (var gzipStream = new GZipStream(responseStream, CompressionMode.Decompress))
                    using (var streamReader = new StreamReader(gzipStream))
                    {
                        var data = streamReader.ReadToEnd();
                        return JsonConvert.DeserializeObject<HashResult>(data);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("GetProfile progress occur exception " + ex.Message);
                if (ex.Message.Contains("404"))
                    return null;

                logging.Invoke(LogIO.mainLog, new Log() { Date = DateTime.Now, Message = ex.Message, Method = "FindHashtagPhotos" });
                throw ex;
            }
        }
    }
}
