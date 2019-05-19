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

namespace Vlive_downloader
{
    /// <summary>
    /// Interaction logic for VideoList.xaml
    /// </summary>
    public partial class VideoList : UserControl
    {
        public VideoList(BitmapImage img, string title, List<string> combos, List<string> subs)
        {
            InitializeComponent();
            _img.Source = img;
            _title.Text = title;
            foreach(string s in combos)
            {
                _res.Items.Add(s);
            }
            foreach(string s in subs)
            {
                _sub.Items.Add(s);
            }

        }

        // Slightly hacky...
        public void setIndex(string res)
        {
            string curr = res;
            while (true)
            {
                for (int i = 0; i < _res.Items.Count; i++)
                {
                    if (curr == _res.Items[i].ToString())
                    {
                        _res.SelectedIndex = i;
                        return;
                    }
                }
                if (curr == "144P")
                {
                    return;
                }
                curr = Downgrade(curr);
            }
        }

        // Yeah ew
        private string Downgrade(string res)
        {
            if (res == "1080P")
            {
                return "720P";
            } else if (res == "720P")
            {
                return "480P";
            } else if (res == "480P")
            {
                return "360P";
            } else if (res == "360P")
            {
                return "270P";
            }
            return "144P";
        }
    }
}
