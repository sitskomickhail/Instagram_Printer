﻿using InstagramLibrary;
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

namespace InstMain.Model
{
    public class InstagramModel
    {
        private object[] locker = new object[1];

        public List<Dictionary<string, string>> UsersInfo { get; set; }

        public string Hashtag_1 { get; internal set; }
        public string Hashtag_2 { get; internal set; }

        public bool IsPrintAll { get; set; }
        public bool IsWorking { get; internal set; }
        public bool IsError_Need { get; set; }

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
                DateTimeOffset tempTime;
                if (IsError_Need && File.Exists("errorPostTime.sdv"))
                    tempTime = DateTime.Parse(File.ReadAllText("errorPostTime.sdv"));
                else
                {
                    tempTime = DateTime.Now;
                }

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
                                    string id = await instRequest.GetUserName(accounts[i].node.shortcode);
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
                    File.WriteAllText("errorPostTime.sdv", tempTime.ToString());
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
                while (IsWorking || IsPrintAll)
                {
                    List<List<string>> files_all = new List<List<string>>();
                    List<List<string>> files_two = new List<List<string>>();
                    lock (locker)
                    {
                        foreach (var user in UsersInfo)
                        {
                            var res = Directory.GetFiles($@"Photos\{user["id"]}\").ToList();
                            if (res.Count() >= 2)
                            {
                                files_two.Add(res);
                                res.Add(user["id"]);
                            }
                            else if (res.Count() > 0)
                            {
                                files_all.Add(res);
                                res.Add(user["id"]);
                            }
                        }
                    }

                    foreach (var files in files_two)
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

                        if (Hashtag_1.Length > 0)
                            creatorLogic.HashtagforDrawing1 = "#" + Hashtag_1;
                        if (Hashtag_2.Length > 0)
                            creatorLogic.HashtagforDrawing2 = "#" + Hashtag_2;

                        creatorLogic.Username = files[files.Count - 1];

                        var imgRs1 = creatorLogic.ResizeImage(640, Environment.CurrentDirectory + @"\Photos\" + files[0]);
                        var imgRs2 = creatorLogic.ResizeImage(640, Environment.CurrentDirectory + @"\Photos\" + files[1]);

                        string oldFile0 = files[0];
                        string oldFile1 = files[1];

                        files[0] = oldFile0.Replace(".jpg", "lp.jpg");
                        files[1] = oldFile1.Replace(".jpg", "lp.jpg");

                        imgRs1.Save(Environment.CurrentDirectory + @"\Photos\" + files[0]); imgRs1.Dispose();
                        imgRs2.Save(Environment.CurrentDirectory + @"\Photos\" + files[1]); imgRs2.Dispose();

                        File.Delete(@"Photos\" + oldFile0); File.Delete(@"Photos\" + oldFile1);

                        string photoName = creatorLogic.CreatePhoto(files[0]);
                        creatorLogic.AddPhoto(photoName, files[1]);
                        creatorLogic.AddLogo(photoName);

                        File.Delete(@"Photos\" + files[0]);
                        File.Delete(@"Photos\" + files[1]);

                        var img = creatorLogic.ResizeImage(610, photoName);
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

                    if (tempPos == 26 || IsPrintAll)
                    {
                        WaitingPrint = files_all.Count;
                        foreach (var files in files_all)
                        {
                            var strs = files[0].Split('\\');
                            var res = Directory.GetFiles(Environment.CurrentDirectory + @"\Photos\" + $@"{strs[strs.Count() - 2]}");
                            if (res.Count() % 2 == 1)
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

                                if (Hashtag_1.Length > 0)
                                    creatorLogic.HashtagforDrawing1 = "#" + Hashtag_1;
                                if (Hashtag_2.Length > 0)
                                    creatorLogic.HashtagforDrawing2 = "#" + Hashtag_2;

                                creatorLogic.Username = files[files.Count - 1];

                                var imgRs1 = creatorLogic.ResizeImage(640, Environment.CurrentDirectory + @"\Photos\" + files[0]);

                                string oldFile0 = files[0];

                                files[0] = oldFile0.Replace(".jpg", "lp.jpg");

                                imgRs1.Save(Environment.CurrentDirectory + @"\Photos\" + files[0]); imgRs1.Dispose();

                                File.Delete(@"Photos\" + oldFile0);

                                string photoName = creatorLogic.CreatePhoto(files[0]);
                                creatorLogic.AddLogo(photoName);

                                File.Delete(@"Photos\" + files[0]);

                                var img = creatorLogic.ResizeImage(610, photoName);
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
                    IsPrintAll = false;
                    tempPos++;
                    Thread.Sleep(30000);
                }
            });
        }
    }
}