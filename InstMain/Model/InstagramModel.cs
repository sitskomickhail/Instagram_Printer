using InstagramLibrary;
using PrinterLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace InstMain.Model
{
    public class InstagramModel
    {
        private object[] locker = new object[1];

        public List<Dictionary<string, string>> UsersInfo { get; set; }

        public string Hashtag_1 { get; internal set; }
        public string Hashtag_2 { get; internal set; }

        public bool IsWorking { get; internal set; }

        public string TemplateName { get; internal set; }
        public string LogoName { get; internal set; }

        public int SuccessPrint { get; internal set; }
        public int UnsuccessPrints { get; internal set; }
        public int WaitingPrint { get; internal set; }

        public InstagramModel()
        {
            UsersInfo = new List<Dictionary<string, string>>();
        }

        public async void Start()
        {
            await Task.Run(async () =>
            {
                DateTimeOffset tempTime = DateTime.Now;
                long timestamp_template = tempTime.ToUnixTimeSeconds();
                while (IsWorking)
                {
                    InstRequest instRequest = new InstRequest();
                    if (Hashtag_2.Length > 0)
                        instRequest.Hashtag = Hashtag_2;
                    else
                        instRequest.Hashtag = Hashtag_1;
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
                                            if (Int32.Parse(user[id]) >= 30)
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
                                            int parsed = Int32.Parse(user[id]);
                                            parsed++;
                                            user[$"{id}"] = parsed.ToString();
                                            notFound = false;
                                            break;
                                        }
                                    }

                                    if (notFound)
                                    {
                                        var res = new Dictionary<string, string>();
                                        res.Add(id, "1");
                                        res.Add("id", id);
                                        lock (locker)
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
            });
        }

        public async void CreatePhoto()
        {
            await Task.Run(() =>
            {
                List<Dictionary<string, PhotoCreatorLogic>> valuePairs = new List<Dictionary<string, PhotoCreatorLogic>>();
                int tempPos = 0;
                while (IsWorking)
                {
                    List<string[]> files_all = new List<string[]>();
                    List<string[]> files_two = new List<string[]>();
                    lock (locker)
                    {
                        foreach (var user in UsersInfo)
                        {
                            files_all.Add(Directory.GetFiles($@"Photos\{user["id"]}\"));
                            if (Int32.Parse(user[$"{user["id"]}"]) % 2 == 0)
                            {
                                files_two.Add(Directory.GetFiles($@"Photos\{user["id"]}\"));
                            }
                        }
                    }

                    foreach (var files in files_two)
                    {
                        if (files.Count() >= 2)
                        {
                            files[0] = files[0].Remove(0, 7);
                            files[1] = files[1].Remove(0, 7);
                            PhotoCreatorLogic creatorLogic = new PhotoCreatorLogic();

                            if (TemplateName != null)
                            {
                                string[] splitted = TemplateName.Split('\\');
                                TemplateName = splitted[splitted.Length - 1];
                                creatorLogic.TemplateName = TemplateName;
                            }
                            if (LogoName != null)
                            {
                                string[] splitted = LogoName.Split('\\');
                                LogoName = splitted[splitted.Length - 1];
                                creatorLogic.LogoName = LogoName;
                            }

                            string photoName = creatorLogic.CreatePhoto(files[0]);
                            creatorLogic.AddPhoto(photoName, files[1]);
                            creatorLogic.AddLogo(photoName);

                            File.Delete(@"Photos\" + files[0]);
                            File.Delete(@"Photos\" + files[1]);

                            var img = creatorLogic.ResizeImage(790, photoName);
                            string newPhotoName = photoName.Replace(".jpg", "sp.jpg");
                            img.Save(newPhotoName);
                            img.Dispose();
                            File.Delete(photoName);

                            PrintCl print = new PrintCl(Environment.CurrentDirectory + "\\" + newPhotoName);
                            try { print.PrintImage(); SuccessPrint++; }
                            catch (Exception ex)
                            { Debug.WriteLine(ex.Message + "\n" + ex.StackTrace); UnsuccessPrints++; }
                            File.Delete(newPhotoName);
                        }
                    }

                    if (tempPos == 1)
                    {
                        WaitingPrint = files_all.Count;
                        foreach (var files in files_all)
                        {
                            if (files.Count() == 1)
                            {
                                files[0] = files[0].Remove(0, 7);
                                PhotoCreatorLogic creatorLogic = new PhotoCreatorLogic();

                                if (TemplateName != null)
                                {
                                    string[] splitted = TemplateName.Split('\\');
                                    TemplateName = splitted[splitted.Length - 1];
                                    creatorLogic.TemplateName = TemplateName;
                                }
                                if (LogoName != null)
                                {
                                    string[] splitted = LogoName.Split('\\');
                                    LogoName = splitted[splitted.Length - 1];
                                    creatorLogic.LogoName = LogoName;
                                }

                                string photoName = creatorLogic.CreatePhoto(files[0]);
                                creatorLogic.AddLogo(photoName);

                                File.Delete(@"Photos\" + files[0]);

                                var img = creatorLogic.ResizeImage(790, photoName);
                                string newPhotoName = photoName.Replace(".jpg", "sp.jpg");
                                img.Save(newPhotoName);
                                img.Dispose();
                                File.Delete(photoName);

                                PrintCl print = new PrintCl(Environment.CurrentDirectory + "\\" + newPhotoName);
                                try { print.PrintImage(); SuccessPrint++; }
                                catch (Exception ex)
                                { Debug.WriteLine(ex.Message + "\n" + ex.StackTrace); UnsuccessPrints++; }
                                File.Delete(newPhotoName);
                            }
                            WaitingPrint--;
                        }
                        tempPos = -1;
                    }

                    tempPos++;
                    Thread.Sleep(30000);
                }
            });
        }
    }
}