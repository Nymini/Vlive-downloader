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
        public VideoItem(BitmapImage img, string title, List<string> combos, List<string> subs, string colour)
        {
            InitializeComponent();
            _img.Source = img;
            _title.Text = title;
            this.colour = colour;
            var bc = new BrushConverter();
            _progress.Foreground = (Brush)bc.ConvertFrom("#FF" + colour);
            _progress.Background = (Brush)bc.ConvertFrom("#9A" + colour);
            _progress.BorderBrush = (Brush)bc.ConvertFrom("#FF" + colour);
            _remove.Background = (Brush)bc.ConvertFrom("#FF" + colour);
            _remove.BorderBrush = (Brush)bc.ConvertFrom("#FF" + colour);
            _title.BorderBrush = (Brush)bc.ConvertFrom("#69" + colour);
            _title.SelectionBrush = (Brush)bc.ConvertFrom("#69" + colour);
            TextFieldAssist.SetUnderlineBrush(_title, (Brush)bc.ConvertFrom("#69" + colour));
            
            _title.CaretBrush = (Brush)bc.ConvertFrom("#FF" + colour);
            foreach (string s in combos)
            {
                Button b = new Button();
                b.Content = s;
                b.Margin = new Thickness(10, 5, 0, 0);
                b.Background = (Brush)bc.ConvertFrom("#F5" + colour);
                b.BorderBrush = (Brush)bc.ConvertFrom("#F5" + colour);
                b.Click += Choose;
                _res.Children.Add(b);
            }
            foreach (string s in subs)
            {
                Button b = new Button();
                b.Content = s;
                b.Margin = new Thickness(10, 5, 0, 0);
                b.Background = (Brush)bc.ConvertFrom("#F5" + colour);
                b.BorderBrush = (Brush)bc.ConvertFrom("#F5" + colour);
                b.Click += Choose;
                _sub.Children.Add(b);
            }

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

        private void Remove(object sender, RoutedEventArgs e)
        {
            
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

    }
}
