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
using MaterialDesignThemes.Wpf;
using MaterialDesignColors;

namespace Vlive_Downloader_Material
{
    /// <summary>
    /// Interaction logic for VideoItem.xaml
    /// </summary>
    public partial class VideoItem : UserControl
    {

        private string colour = "FF0000";
        private Dictionary<string, string> video;
        private Dictionary<string, string> subtitle;
        public VideoItem(BitmapImage img, string title, string colour, Dictionary<string, string> combos, Dictionary<string, string> subs)
        {
            InitializeComponent();
            _img.Source = img;
            _title.Text = title;
            this.colour = colour;
            var bc = new BrushConverter();
            _progress.Foreground = (Brush)bc.ConvertFrom("#FF" + colour);
            _progress.Background = (Brush)bc.ConvertFrom("#9A" + colour);
            _progress.BorderBrush = (Brush)bc.ConvertFrom("#FF" + colour);
            _title.BorderBrush = (Brush)bc.ConvertFrom("#69" + colour);
            _title.SelectionBrush = (Brush)bc.ConvertFrom("#69" + colour);
            TextFieldAssist.SetUnderlineBrush(_title, (Brush)bc.ConvertFrom("#69" + colour));
            
            _title.CaretBrush = (Brush)bc.ConvertFrom("#FF" + colour);
            foreach (string s in combos.Keys)
            {
                Button b = new Button();
                b.Content = s;
                b.Margin = new Thickness(10, 5, 0, 0);
                b.Background = (Brush)bc.ConvertFrom("#F5" + colour);
                b.BorderBrush = (Brush)bc.ConvertFrom("#F5" + colour);
                b.Click += Choose;
                _res.Children.Add(b);
            }
            foreach (string s in subs.Keys)
            {
                Button b = new Button();
                b.Content = s;
                b.Margin = new Thickness(10, 5, 0, 0);
                b.Background = (Brush)bc.ConvertFrom("#F5" + colour);
                b.BorderBrush = (Brush)bc.ConvertFrom("#F5" + colour);
                b.Click += Choose;
                _sub.Children.Add(b);
            }

            video = combos;
            subtitle = subs;

        }

        // Debugging constructor
        public VideoItem(string colour)
        {
            InitializeComponent();
            this.colour = colour;
        }

        private void Choose(object sender, RoutedEventArgs e)
        {
            //https://stackoverflow.com/questions/979876/set-background-color-of-wpf-textbox-in-c-sharp-code
            var bc = new BrushConverter();

            WrapPanel p = ((e.Source as Button).Parent as WrapPanel);
            foreach (Button b in p.Children)
            {
                b.Background = (Brush)bc.ConvertFrom("#69" + colour);
            }
            (e.Source as Button).Background = (Brush)bc.ConvertFrom("#FF" + colour);
        }

        public string DLLink()
        {
            string res = Get_Res();
            if (res == null)
            {
                return null;
            }
            return video[res];
        }

        public string SubLink()
        {
            string sub = Get_Sub();
            if (sub == null)
            {
                return null;
            }
            return subtitle[sub];
        }

        public string Get_Res()
        {
            foreach(Button b in _res.Children)
            {
                if (b.Background.ToString() == "#FF" + colour)
                {
                    return b.Content.ToString();
                }
            }
            // Should be impossible to reach here.
            return null;
        }

        public string Get_Sub()
        {
            foreach (Button b in _sub.Children)
            {
                if (b.Background.ToString() == "#FF" + colour)
                {
                    return b.Content.ToString();
                }
            }
            // Should be impossible to reach here.
            return null;
        }

        public void Set_Res(string res)
        {
            var bc = new BrushConverter();
            bool set = false;

            if (res == "None")
            {
                return;
            }
            while (true)
            {
                foreach (Button b in _res.Children)
                {
                    System.Diagnostics.Debug.Write(b.Content.ToString() + " " + res + " \n");
                    if (b.Content.ToString() == res)
                    {
                        
                        b.Background = (Brush)bc.ConvertFrom("#FF" + colour);
                        set = true;
                    }
                    else
                    {
                        b.Background = (Brush)bc.ConvertFrom("#69" + colour);
                    }
                }
                if (set)
                {
                    return;
                } else
                {
                    switch (res)
                    {
                        case "1080P":
                            res = "720P";
                            break;
                        case "720P":
                            res = "480P";
                            break;
                        case "480P":
                            res = "360P";
                            break;
                        case "360P":
                            res = "270P";
                            break;
                        case "270P":
                            res = "144P";
                            break;
                        case "144P":
                            return;
                    }
                }
                //System.Diagnostics.Debug.Write(res.ToString() + " uh \n");
            }
        }

        public void Set_Sub(string sub)
        {
            var bc = new BrushConverter();
            foreach (Button b in _sub.Children)
            {
                if (b.Content.ToString() == sub)
                {
                    b.Background = (Brush)bc.ConvertFrom("#FF" + colour);

                }
                else
                {
                    b.Background = (Brush)bc.ConvertFrom("#69" + colour);
                }
            }
            return;
        }

    }
}
