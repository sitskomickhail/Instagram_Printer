using InstagramLibrary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InstMain.Model
{
    public class InstagramModel
    {
        public List<Dictionary<string, int>> UsersInfo { get; set; }

        public string Hashtag_1 { get; internal set; }
        public string Hashtag_2 { get; internal set; }

        public bool IsWorking { get; internal set; }
        public string TemplateName { get; internal set; }

        public InstagramModel()
        {
            UsersInfo = new List<Dictionary<string, int>>();
            IsWorking = true;
        }

        public async void Start()
        {
            DateTimeOffset tempTime = DateTime.Now;
            long timestamp_template = tempTime.ToUnixTimeSeconds();
            while (IsWorking)
            {
                InstRequest instRequest = new InstRequest();
                instRequest.Hashtag = Hashtag_2;

                var result = await instRequest.FindHashtagPhotos();
                if (result == null)
                {
                    Thread.Sleep(90000);
                    continue;
                }
                var accounts = result.graphql.hashtag.edge_hashtag_to_media.edges;
                for (int i = 0; i < accounts.Count; i++)
                {
                    long postTime = accounts[i].node.taken_at_timestamp;
                    if (DateTimeOffset.FromUnixTimeSeconds(postTime) > tempTime)
                    {
                        if (i == 0)
                            timestamp_template = postTime;

                        var edgesText = accounts[i].node.edge_media_to_caption.edges;
                        if (edgesText.Count > 0)
                            if (edgesText[0].node.text.Contains(Hashtag_1))
                            {
                                string id = accounts[i].node.owner.id;
                                bool tempExisting = false;
                                foreach (var user in UsersInfo)
                                {
                                    if (user.ContainsKey(id))
                                        if (user[id] >= 15)
                                        {
                                            tempExisting = true;
                                            break;
                                        }
                                }

                                if (tempExisting)
                                    continue;

                                bool notFound = true;
                                foreach (var user in UsersInfo)
                                {
                                    if (user.ContainsKey(id))
                                    {
                                        user[id]++;
                                        notFound = false;
                                        break;
                                    }
                                }

                                if (notFound)
                                {
                                    var res = new Dictionary<string, int>();
                                    res.Add(id, 1);
                                    UsersInfo.Add(res);
                                }

                                string url = accounts[i].node.display_url;

                                string directory = Environment.CurrentDirectory + $@"\Photos\{id}";
                                if (!Directory.Exists(directory))
                                    Directory.CreateDirectory(directory);
                                string localFilename = directory;

                                string fileName = Generator.GenerateName();
                                using (WebClient client = new WebClient())
                                {
                                    client.DownloadFile(url, $@"{directory}\{fileName}.jpg");
                                }
                            }
                    }
                    else
                    {
                        break;
                    }
                }

                tempTime = DateTimeOffset.FromUnixTimeSeconds(timestamp_template);
                Thread.Sleep(60000);
            }
        }

        public async void CreatePhoto()
        {

        }
    }
}
