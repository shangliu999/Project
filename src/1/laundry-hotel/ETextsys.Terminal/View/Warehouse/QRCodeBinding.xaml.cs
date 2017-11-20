﻿using System;
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
    /// QRCodeBinding.xaml 的交互逻辑
    /// </summary>
    public partial class QRCodeBinding : Window
    {
        public QRCodeBinding()
        {
            InitializeComponent();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            txtQRCode.Focus();
        }
    }
}
