using ETextsys.Terminal.ViewModel.Settings;
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

namespace ETextsys.Terminal.View.Settings
{
    /// <summary>
    /// BusinessSetting.xaml 的交互逻辑
    /// </summary>
    public partial class SettingMain : Window
    {
        public SettingMain()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string currenttag = contentPanel.Tag.ToString();
            string tag = (sender as Button).Tag.ToString();
            if (currenttag == tag)
                return;

            contentPanel.Children.Clear();
            switch (tag)
            {
                case "Business":
                    BusinessSetting bs = new Settings.BusinessSetting();
                    BusinessSettingViewModel bsvm = new ViewModel.Settings.BusinessSettingViewModel();
                    bs.DataContext = bsvm;
                    contentPanel.Children.Add(bs);
                    break;
                case "BG":
                    SystemBG bg = new Settings.SystemBG();
                    SystemBGViewModel bgvm = new ViewModel.Settings.SystemBGViewModel();
                    bg.DataContext = bgvm;
                    contentPanel.Children.Add(bg);
                    break;
                case "Reader":
                    ReaderSetting rs = new Settings.ReaderSetting();
                    ReaderSettingViewModel vm = new ReaderSettingViewModel();
                    rs.DataContext = vm;
                    contentPanel.Children.Add(rs);
                    break;
                case "System":
                    SystemSetting ss = new Settings.SystemSetting();
                    SystemSettingViewModel ssvm = new ViewModel.Settings.SystemSettingViewModel();
                    ss.DataContext = ssvm;
                    contentPanel.Children.Add(ss);
                    break;
            }
            contentPanel.Tag = tag;
        }
    }
}
