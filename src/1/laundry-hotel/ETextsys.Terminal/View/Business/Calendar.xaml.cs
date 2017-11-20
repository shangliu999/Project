using ETextsys.Terminal.ViewModel.Business;
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

namespace ETextsys.Terminal.View.Business
{
    /// <summary>
    /// Calendar.xaml 的交互逻辑
    /// </summary>
    public partial class Calendar : Window
    {
        public Calendar()
        {
            InitializeComponent();
            this.DataContext = new CalendarViewModel(this.Close, calendar);
        }
    }
}
