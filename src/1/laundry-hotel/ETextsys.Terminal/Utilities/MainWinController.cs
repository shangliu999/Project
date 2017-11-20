using ETexsys.APIRequestModel.Response;
using ETextsys.Terminal.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Utilities
{
    public class MainWindowController
    {
        private static readonly object obj = new object();

        private static MainWindow instance;

        public static MainWindow Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (obj)
                    {
                        if (null == instance)
                        {
                            instance = new MainWindow();
                            MainWindowViewModel mwvm = new MainWindowViewModel();
                            instance.Closing += mwvm.RequestClose;
                            instance.DataContext = mwvm;
                        }
                    }
                }
                return instance;
            }
        }

        private static List<ResponseSysRightModel> sysRights;
        public static List<ResponseSysRightModel> SysRights
        {
            get { return sysRights; }
            set { sysRights = value; }
        }
        
        private static ResponseSysCustomerModel sysCustomer;
        public static ResponseSysCustomerModel SysCustomer
        {
            get { return sysCustomer; }

            set { sysCustomer = value; }
        }
    }
}
