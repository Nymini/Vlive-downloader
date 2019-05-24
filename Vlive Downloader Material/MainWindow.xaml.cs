using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
using Newtonsoft.Json;
using MaterialDesignThemes.Wpf;
using MaterialDesignColors;
using System.Configuration;
using System.Collections.Specialized;

namespace Vlive_Downloader_Material
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Handle POST/GET requests
        private static readonly HttpClient client = new HttpClient();
        private List<string> seenSubs = new List<string>();
        private int curr = 0;
        private int col = 0;
        private List<string> colours = new List<string>();

        public MainWindow()
        {
            InitializeComponent();
            _prefRes.Items.Add("None");
            _prefRes.Items.Add("1080P");
            _prefRes.Items.Add("720P");
            _prefRes.Items.Add("480P");
            _prefRes.Items.Add("360P");
            _prefRes.Items.Add("270P");
            _prefRes.Items.Add("144P");
            _prefSub.Items.Add("None");
            _prefSub.Items.Add("English");

            _prefRes.SelectedItem = ConfigurationManager.AppSettings.Get("prefRes");
            _prefSub.SelectedItem = ConfigurationManager.AppSettings.Get("prefSub");
        }

        // Retrieves URL
        private async void Add_Url(object sender, RoutedEventArgs e)
        {
            string url = _url.Text;
            url = "https://www.vlive.tv/video/127002?channelCode=F5F127";
            string html;

            if (url == null)
            {
                _snackbar.MessageQueue.Enqueue("Empty URL", "OOF", () => Foo());
                _url.Clear();
                return;
            }
            Match v = Regex.Match(url, @"vlive.tv/video/");
            if (!v.Success)
            {
                _snackbar.MessageQueue.Enqueue("Invalid URL", "OOF", () => Foo());
                _url.Clear();
                return;
            }

            // Retrieve html to find internal video ID + video Key
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
            // Make controls visible/invisible now that we have a video
            _download.Visibility = Visibility.Visible;
            _hasContent.Visibility = Visibility.Hidden;


        }

        // Create a POST request to retrieve JSON content
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
            // Hacky way to retrieve JSON content
            var json = JsonConvert.DeserializeObject<Dictionary<String, object>>(responseString);
            var list = JsonConvert.DeserializeObject<Dictionary<String, object>>(json["videos"].ToString());
            var meta = JsonConvert.DeserializeObject<Dictionary<String, object>>(json["meta"].ToString());
            var sub = JsonConvert.DeserializeObject<Dictionary<String, object>>(json["captions"].ToString());
            string name = meta["subject"].ToString();
            var tmp = list["list"].ToString();

            Dictionary<string, string> subLabels = new Dictionary<string, string>();
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
            Dictionary<string, string> dicNames = new Dictionary<string, string>();
            for (int i = 0; i < names.Count; i++)
            {
                dicNames.Add(names[i], videoLinks[i]);
            }

            createObject(dicNames, img, name, subLabels);
        }

        private string getImageURL(string r)
        {
            string cleaned = Regex.Replace(r, "<imgsrc=", "");
            cleaned = Regex.Replace(cleaned, @"class=.*", "");
            return cleaned;
        }

        private async void createObject(Dictionary<string, string> res, string img, string name, Dictionary<string, string> labels)
        {
            _snackbar.MessageQueue.Enqueue("Added video to queue", "Nice", () => Foo());
            string colour = Colourize();
            VideoItem v = new VideoItem(new BitmapImage(new Uri(img)), name, colour, res, labels );
            v.Margin = new Thickness(5, 10, 5, 0);
            Grid g = new Grid();

            if (_prefRes.SelectedItem.ToString() != "None")
            {
                v.Set_Res(_prefRes.SelectedItem.ToString());
            }
            if (_prefSub.SelectedItem.ToString() != "None")
            {
                v.Set_Sub(_prefSub.SelectedItem.ToString());
            }
            g.Children.Add(v);
            var bc = new BrushConverter();

            // Create a remove button to add to grid
            Button b = new Button();
            b.Content = "Remove";
            b.VerticalAlignment = VerticalAlignment.Top;
            b.HorizontalAlignment = HorizontalAlignment.Right;
            b.Height = 25;
            b.Margin = new Thickness(0, 5, 0, 0);
            b.FontSize = 11;
            b.Background = (Brush)bc.ConvertFrom("#FF" + colour);
            b.BorderBrush = (Brush)bc.ConvertFrom("#FF" + colour);
            b.Click += Remove;
            g.Children.Add(b);
            g.Margin = new Thickness(20, 10, 20, 10);
            _videoList.Items.Add(g);
            _url.Clear();
        }

        private async void Download(object sender, RoutedEventArgs e)
        {

            foreach (Grid g in _videoList.Items)
            {
                VideoItem v = g.Children[0] as VideoItem;
                if (v.Get_Res() == null)
                {
                    //System.Diagnostics.Debug.Write("oh no\n");
                    _snackbar.MessageQueue.Enqueue("One or more videos does not have a chosen resolution");
                    return;
                }
            }
            _download.Visibility = Visibility.Hidden;
            _add.Visibility = Visibility.Hidden;
            downloadFile();

        }

        private void downloadFile()
        {

            if (curr >= _videoList.Items.Count)
            {
                _fin.IsActive = true;
                SnackbarMessage m = new SnackbarMessage();
                m.Content = "All files finished downloading";
                m.ActionContent = "Remove all completed";
                m.ActionClick += Remove_All;
                _fin.Message = m;
                _add.Visibility = Visibility.Hidden;
  
                return;
            }
            Grid g = _videoList.Items[curr] as Grid;
            VideoItem v = g.Children[0] as VideoItem;

            string name = v._title.Text;
            string dlLink = v.DLLink();
            string locale = v.SubLink();
            string res = v.Get_Res();
            if (locale != null)
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadFileAsync(new Uri(locale), name + "[" + res + "]" + ".vtt");
                }
            }

            using (WebClient client = new WebClient())
            {
                client.DownloadFileCompleted += new AsyncCompletedEventHandler(downloadComplete);
                client.DownloadProgressChanged += new DownloadProgressChangedEventHandler(updateProgress);
                client.DownloadFileAsync(new Uri(dlLink), name + "[" + res + "]" + ".mp4");
            }
        }

        private void updateProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            Grid g = _videoList.Items[curr] as Grid;
            VideoItem v = g.Children[0] as VideoItem;
            v._progress.Value = e.ProgressPercentage;
        }

        private void downloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            _snackbar.MessageQueue.Enqueue("File " + curr.ToString() + " finished downloading", "Nice", () => Foo());
            curr++;
            downloadFile();
        }

        private Dictionary<string, string> retrieveSubs(string src)
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
                    string tmp = cleanify(line);
                    label.Add(tmp);
                    bool seen = false;
                    for (int i = 0; i < seenSubs.Count; i++)
                    {
                        if (seenSubs[i] == tmp)
                        {
                            seen = true;
                        }
                    }
                    if (!seen)
                    {
                        seenSubs.Add(tmp);
                        System.Diagnostics.Debug.Write(tmp + "\n");
                    }
                }
            }
            Dictionary<string, string> dic = new Dictionary<string, string>();
            for (int i = 0; i < label.Count; i++)
            {
                if (!dic.ContainsKey(label[i]))
                {
                    dic.Add(label[i], subLinks[i]);
                }
            }
            return dic;
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

        private string Colourize()
        {
            if (colours.Count == 0)
            {
                colours.Add("F92672");
                colours.Add("66D9EF");
                colours.Add("A6E22E");
                colours.Add("FD971F");

            }

            string colour = colours[col];
            col++;
            if (col >= colours.Count)
            {
                col = 0;
            }
            return colour;
        }

        private async void Remove(object sender, RoutedEventArgs e)
        {
            Grid g = ((e.Source as Button).Parent as Grid);
            foreach (Grid i in _videoList.Items)
            {
                if (i == g)
                {
                    _videoList.Items.Remove(g);
                    if (_videoList.Items.Count == 0)
                    {
                        _download.Visibility = Visibility.Hidden;
                        _hasContent.Visibility = Visibility.Visible;
                    }
                    return;
                }
            }
        }

        public void Foo()
        {

        }

        private async void Remove_All(object sender, RoutedEventArgs e)
        {
            _videoList.Items.Clear();
            _download.Visibility = Visibility.Hidden;
            _hasContent.Visibility = Visibility.Visible;
            _fin.IsActive = false;
            _add.Visibility = Visibility.Visible;
            curr = 0;
        }

        private void Set_Pref_Res(object sender, SelectionChangedEventArgs e)
        {
            //https://stackoverflow.com/questions/305529/how-to-update-appsettings-in-a-wpf-app
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["prefRes"].Value = _prefRes.SelectedItem.ToString();
            config.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }

        private void Set_Pref_Sub(object sender, SelectionChangedEventArgs e)
        {
            //https://stackoverflow.com/questions/305529/how-to-update-appsettings-in-a-wpf-app
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            config.AppSettings.Settings["prefSub"].Value = _prefSub.SelectedItem.ToString();
            config.Save(ConfigurationSaveMode.Full);
            ConfigurationManager.RefreshSection("appSettings");
        }
    }
}
