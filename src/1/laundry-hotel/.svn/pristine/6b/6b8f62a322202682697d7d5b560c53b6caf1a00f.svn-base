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

namespace ETextsys.Terminal.View.Warehouse
{
    /// <summary>
    /// QRCodePrint.xaml 的交互逻辑
    /// </summary>
    public partial class QRCodePrint : Window
    {
        public QRCodePrint()
        {
            InitializeComponent();
        }

        private void PrintCount_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            bool shiftKey = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;//判断shifu键是否按下
            if (shiftKey == true)                  //当按下shift
            {
                e.Handled = true;//不可输入
            }
            if (!((e.Key >= Key.D0 && e.Key <= Key.D9) || (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9) || e.Key == Key.Delete || e.Key == Key.Back || e.Key == Key.Tab || e.Key == Key.Enter))
            {
                e.Handled = true;//不可输入
            }
        }
    }
}
