using InstagramLibrary.InstTemplates;
using InstaLog;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
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
            for (int i = 0; i < 3; i++)
                try
                {
                    var bootstrapRequest = HttpRequestBuilder.Get("https://www.instagram.com/instagram/?hl=ru", _cookieContainer);
                    bootstrapRequest.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*;q=0.8";
                    bootstrapRequest.Headers["Upgrade-Insecure-Requests"] = "1";
                    using (var bootstrapResponse = await bootstrapRequest.GetResponseAsync() as HttpWebResponse)
                    {
                        if (bootstrapResponse.Cookies.Count == 0)
                            continue;
                        _cookieContainer.Add(bootstrapResponse.Cookies);
                        break;
                    }
                }
                catch (Exception bex)
                {
                    Debug.WriteLine("Bootstrap progress meet exception " + bex.Message);
                    throw bex; //Status==ConnectFailure
                }


            try
            {
                var request = HttpRequestBuilder.Get("https://www.instagram.com/instagram/?hl=ru", _cookieContainer);
                request.Referer = $"https://www.instagram.com/explore/{Hashtag}/find/?hl=ru";
                request.Headers["X-Requested-With"] = "XMLHttpRequest";
                request.AllowAutoRedirect = false;
                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    _cookieContainer.Add(response.Cookies);
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
                lock (LogIO.locker) logging.Invoke(LogIO.mainLog, new Log() { UserName = null, Date = DateTime.Now, Message = $"Exception! {ex.Message}", Method = "HttpAndroid.FindHashtagPhotos" });
                throw ex;
            }
        }
    }
}
