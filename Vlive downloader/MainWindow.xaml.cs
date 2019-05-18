﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Net;
using System.Text.RegularExpressions;
using System.Net.Http;
using Newtonsoft.Json;
using System.ComponentModel;

namespace Vlive_downloader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Handle POST/GET requests
        private static readonly HttpClient client = new HttpClient();
        private List<Video> videos = new List<Video>();
        private List<Subtitle> subs = new List<Subtitle>();
        private int curr = 0;
        public MainWindow()
        {
            InitializeComponent();
            _url.Text = "https://www.vlive.tv/video/127002?channelCode=F5F127";


        }

        private void _urlInsert_Click(object sender, RoutedEventArgs e)
        {
            string url = _url.Text;

            string html;
            using (WebClient client = new WebClient())
            {
                //https://www.vlive.tv/video/127002?channelCode=F5F127
                html = client.DownloadString(url);
                
            }

            int n_match = 0;
            string vid_id = null;
            string key = null;
            string img = null;
            string name = null;
            // Look through each line in html finding a regex match.
            // #TODO: There are better methods than iterating through html.
            foreach (var line in html.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                Match m = Regex.Match(line, @"video.init");
                if (n_match == 2)
                {
                    key = line;
                    n_match++;
                }
                if (n_match == 1)
                {
                    vid_id = line;
                    n_match++;
                }
                if (m.Success && n_match == 0)
                {
                    n_match++;
                }
                Match n = Regex.Match(line, "_video_thumb");
                if (n.Success)
                {
                    //We use regex for now.
                    img = getImageURL(cleanify(line));
                
                }
            }
            handleRequest(cleanify(vid_id), cleanify(key), img);
        }
        
        // Removes unwanted characters from key and video id
        private string cleanify(string str)
        {
            string res = str;
            res = Regex.Replace(res, "\"", "");
            res = Regex.Replace(res, "source:", "");
            res = Regex.Replace(res, "name:", "");
            res = Regex.Replace(res, "label:", "");
            res = Regex.Replace(res, @"\s+", "");
            res = Regex.Replace(res, ",", "");

            return res;
        }

        private async void handleRequest(string vidId, string key, string img)
        {
            string url = "http://global.apis.naver.com/rmcnmv/rmcnmv/vod_play_videoInfo.json";
            var param = new Dictionary<string, string>
            {
                { "videoId" , vidId },
                { "key" , key },
                { "ptc" , "http" },
                { "doct" , "json" },
                { "cpt" , "vtt" }
            };
            var content = new FormUrlEncodedContent(param);

            var response = await client.PostAsync(url, content);

            var responseString = await response.Content.ReadAsStringAsync();
            System.Diagnostics.Debug.Write(responseString);
            var json = JsonConvert.DeserializeObject<Dictionary<String, object>>(responseString);
            var list = JsonConvert.DeserializeObject<Dictionary<String, object>>(json["videos"].ToString());
            var meta = JsonConvert.DeserializeObject<Dictionary<String, object>>(json["meta"].ToString());
            var sub = JsonConvert.DeserializeObject<Dictionary<String, object>>(json["captions"].ToString());
            string name = meta["subject"].ToString();
            var tmp = list["list"].ToString();

            List<string> subLabels = new List<string>();
            subLabels = retrieveSubs(sub["list"].ToString());
            List<string> videoLinks = new List<string>();
            List<string> names = new List<string>();
            foreach (var line in tmp.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                
                Match m = Regex.Match(line, @"source");
                if (m.Success)
                {
                    videoLinks.Add(cleanify(line));
                }
                Match n = Regex.Match(line, @"name");
                if (n.Success)
                {
                    names.Add(cleanify(line));
                }
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            for (int i = 0; i < names.Count; i++)
            {
                dic.Add(names[i], videoLinks[i]);
            }
            Video v = new Video(dic, name);
            videos.Add(v);
            createObject(names, img, name, subLabels);
        }

        private void createObject(List<string> res, string img, string name, List<string> labels)
        {

            VideoList v = new VideoList(new BitmapImage(new Uri(img)), name, res, labels);
            v.Width = _main.Width - 27;
            _videoList.Items.Add(v);
        }

        private string getImageURL(string r)
        {
            string cleaned = Regex.Replace(r, "<imgsrc=", "");
            cleaned = Regex.Replace(cleaned, @"class=.*", "");
            return cleaned;
        }

        private async void _dl_Click(object sender, RoutedEventArgs e)
        {

            foreach(VideoList g in _videoList.Items)
            {
                ComboBox tmp = g._res;
                if (tmp.Text == "--Resolutions--")
                {
                    //System.Diagnostics.Debug.Write("oh no\n");
                    System.Windows.MessageBox.Show("One or more videos does not have a chosen resolution.");
                    return;
                }
            }
            downloadFile();
            
        }
        
        private void downloadFile()
        {

            if (curr >= _videoList.Items.Count)
            {
                return;
            }
            VideoList g = _videoList.Items[curr] as VideoList;

            ComboBox res = g._res;
            ComboBox label = g._sub;
            Video v = videos[curr];
            string dlLink = v.getLink(res.Text);
            if (label.Text != "--Subtitles--")
            {
                Subtitle s = subs[curr];
                string subLink = s.getSub(label.Text);
                using (WebClient client = new WebClient())
                {
                    client.DownloadFileAsync(new Uri(subLink), v.getName() + "[" + res.Text + "]" + ".vtt");
                }
            }
           
            using (WebClient client = new WebClient())
            {
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadComplete);
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(updateProgress);
                client.DownloadFileAsync(new Uri(dlLink), v.getName() + "[" + res.Text + "]" + ".mp4");
            }
        }

        private void updateProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            (_videoList.Items[curr] as VideoList)._progress.Value = e.ProgressPercentage;
        }

        private void downloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            curr++;
            downloadFile();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            foreach(VideoList g in _videoList.Items)
            {
                g.Width = _main.Width - 27;
            }
        }

        private List<string> retrieveSubs(string src)
        {
            
            List<string> subLinks = new List<string>();
            List<string> label = new List<string>();
            foreach (var line in src.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {

                Match m = Regex.Match(line, @"source");
                if (m.Success)
                {
                    subLinks.Add(cleanify(line));
                }
                Match n = Regex.Match(line, @"label");
                if (n.Success)
                {
                    label.Add(cleanify(line));
                }
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            for (int i = 0; i < label.Count; i++)
            {
                dic.Add(label[i], subLinks[i]);
            }
            Subtitle s = new Subtitle(dic);
            subs.Add(s);

            return label;
        }
    }
}
