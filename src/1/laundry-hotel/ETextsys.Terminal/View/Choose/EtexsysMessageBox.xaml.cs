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
using System.Windows.Shapes;

namespace ETextsys.Terminal.View.Choose
{
    /// <summary>
    /// EtexsysMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class EtexsysMessageBox : Window
    {
        public EtexsysMessageBox()
        {
            InitializeComponent();
        }

        public new string Title
        {
            get { return txtTitle.Text; }
            set { this.txtTitle.Text = value; }
        }

        public string Message
        {
            get { return txtMessage.Text; }
            set { this.txtMessage.Text = value; }
        }

        public static bool? Show(string title, string message, MessageBoxButton messageBoxButton, MessageBoxImage messageBoxImage)
        {
            var msgBox = new EtexsysMessageBox();
            msgBox.Title = title;
            msgBox.Message = message;

            if (messageBoxButton == MessageBoxButton.OK)
            {
                msgBox.rightBtn.Content = "好";
                msgBox.rightBtn.Width = 360;
                msgBox.leftBtn.Visibility = Visibility.Collapsed;
                msgBox.rightBtn.Visibility = Visibility.Visible;
            }
            else if (messageBoxButton == MessageBoxButton.OKCancel)
            {
                msgBox.rightBtn.Content = "好";
                msgBox.leftBtn.Content = "取消";
                msgBox.leftBtn.Width = 170;
                msgBox.rightBtn.Width = 170;
                msgBox.leftBtn.Visibility = Visibility.Visible;
                msgBox.rightBtn.Visibility = Visibility.Visible;
            }
            else if (messageBoxButton == MessageBoxButton.YesNo)
            {
                msgBox.rightBtn.Content = "是";
                msgBox.leftBtn.Content = "否";
                msgBox.leftBtn.Width = 170;
                msgBox.rightBtn.Width = 170;
                msgBox.leftBtn.Visibility = Visibility.Visible;
                msgBox.rightBtn.Visibility = Visibility.Visible;
            }

            switch (messageBoxImage)
            {
                case MessageBoxImage.Information:
                    msgBox.imgIcon.Source = new BitmapImage(new Uri("../../Skins/Default/Images/img-succeed.png", UriKind.RelativeOrAbsolute));
                    msgBox.rightBtn.Background = new SolidColorBrush(Color.FromRgb(14, 154, 103));
                    msgBox.leftBtn.Background = new SolidColorBrush(Color.FromRgb(14, 154, 103));
                    msgBox.txtTitle.Foreground= new SolidColorBrush(Color.FromRgb(14, 154, 103));
                    break;
                case MessageBoxImage.Question:
                    msgBox.imgIcon.Source = new BitmapImage(new Uri("../../Skins/Default/Images/img-xuanze.png", UriKind.RelativeOrAbsolute));
                    msgBox.rightBtn.Background = new SolidColorBrush(Color.FromRgb(36, 179, 235));
                    msgBox.leftBtn.Background = new SolidColorBrush(Color.FromRgb(36, 179, 235));
                    msgBox.txtTitle.Foreground = new SolidColorBrush(Color.FromRgb(36, 179, 235));
                    break;
                case MessageBoxImage.Warning:
                    msgBox.imgIcon.Source = new BitmapImage(new Uri("../../Skins/Default/Images/img-set.png", UriKind.RelativeOrAbsolute));
                    msgBox.rightBtn.Background = new SolidColorBrush(Color.FromRgb(248, 180, 0));
                    msgBox.leftBtn.Background = new SolidColorBrush(Color.FromRgb(248, 180, 0));
                    msgBox.txtTitle.Foreground = new SolidColorBrush(Color.FromRgb(248, 180, 0));
                    break;
                case MessageBoxImage.Error:
                    msgBox.imgIcon.Source = new BitmapImage(new Uri("../../Skins/Default/Images/img-no.png", UriKind.RelativeOrAbsolute));
                    msgBox.rightBtn.Background = new SolidColorBrush(Color.FromRgb(253, 6, 6));
                    msgBox.leftBtn.Background = new SolidColorBrush(Color.FromRgb(253, 6, 6));
                    msgBox.txtTitle.Foreground = new SolidColorBrush(Color.FromRgb(253, 6, 6));
                    break;
            }

            return msgBox.ShowDialog();
        }

        private void leftBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void rightBtn_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
