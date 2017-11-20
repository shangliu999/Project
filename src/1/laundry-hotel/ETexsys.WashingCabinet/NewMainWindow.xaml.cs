using ETexsys.WashingCabinet.ViewModel;
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
using System.Windows.Threading;

namespace ETexsys.WashingCabinet
{
    /// <summary>
    /// NewMainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class NewMainWindow : Window
    {
        public NewMainWindow()
        {
            InitializeComponent();
            this.DataContext = new NewMainWindowViewModel();
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            (new DispatcherTimer(TimeSpan.FromMilliseconds(500), DispatcherPriority.Loaded, (o, ee) =>
            {
                lblDate.Text = DateTime.Now.ToString("yyyy-MM-dd");
                lblTimeH.Text = DateTime.Now.ToString("HH");
                lblTimeM.Text = DateTime.Now.ToString("mm");
                lblpx.Opacity = lblpx.Opacity == 1 ? 0 : 1;
            }, Dispatcher)).Start();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            txtCard.Focus();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

        }
    }
}
