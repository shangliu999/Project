using ETextsys.Terminal.ViewModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

namespace ETextsys.Terminal
{
    /// <summary>
    /// Login.xaml 的交互逻辑
    /// </summary>
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            this.DataContext = new LoginViewModel(password, this.Close);
        }

        private void password_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                Process[] process = Process.GetProcessesByName("SoftBoard");
                if (process.Length > 0)
                {

                }
                else
                {
                    System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "\\Resources\\SoftBoard.exe");
                }
            }
            catch { }
        }
    }
}
