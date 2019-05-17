using System;
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
                    name = getName(line);
                
                }
            }
            handleRequest(cleanify(vid_id), cleanify(key), img, name);
        }
        
        // Removes unwanted characters from key and video id
        private string cleanify(string str)
        {
            string res = str;
            res = Regex.Replace(res, "\"", "");
            res = Regex.Replace(res, "source:", "");
            res = Regex.Replace(res, "name:", "");
            res = Regex.Replace(res, @"\s+", "");
            res = Regex.Replace(res, ",", "");

            return res;
        }

        private async void handleRequest(string vidId, string key, string img, string name)
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
            
            var json = JsonConvert.DeserializeObject<Dictionary<String, object>>(responseString);
            var list = JsonConvert.DeserializeObject<Dictionary<String, object>>(json["videos"].ToString());
            var tmp = list["list"].ToString();

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
            Video v = new Video(dic);
            videos.Add(v);
            createObject(names, img, name);
        }

        private void createObject(List<string> res, string img, string name)
        {
            Grid item = new Grid();
            TextBlock dur = new TextBlock();
            Image thumb = new Image();
            thumb.Source = new BitmapImage(new Uri(img));
            thumb.Height = 123;
            thumb.Margin = new Thickness(0, 0, 0, 0);
            thumb.HorizontalAlignment = HorizontalAlignment.Left;
            item.Children.Add(thumb);
            dur.Text = name;
            item.Children.Add(dur);
            ComboBox options = new ComboBox();
            options.HorizontalAlignment = HorizontalAlignment.Right;
            options.IsEditable = true;
            options.IsReadOnly = true;
            options.VerticalAlignment = VerticalAlignment.Top;
            options.Width = 100;
            options.Height = 20;
            options.Margin = new Thickness(650, 0, 0, 0);
            foreach(string str in res)
            {
                options.Items.Add(str);
            }
            options.Text = "--Resolution--";
            item.Children.Add(options);

            _videoList.Items.Add(item);
        }

        private string getImageURL(string r)
        {
            string cleaned = Regex.Replace(r, "<imgsrc=", "");
            cleaned = Regex.Replace(cleaned, @"class=.*", "");
            return cleaned;
        }

        private string getName(string r)
        {
            string cleaned = Regex.Replace(r, @".*alt=", "");
            cleaned = Regex.Replace(cleaned, @"style.*", "");

            return cleaned;
        }
    }
}
