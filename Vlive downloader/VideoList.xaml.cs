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
        public VideoList(BitmapImage img, string title, List<string> combos)
        {
            InitializeComponent();
            _img.Source = img;
            _title.Text = title;
            foreach(string s in combos)
            {
                _res.Items.Add(s);
            }
        }
    }
}
