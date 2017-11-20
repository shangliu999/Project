using ETextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Settings
{
    
    public class AboutViewModel
    {
        private Action windowsclose;
        public AboutViewModel(Action close)
        {
            windowsclose = close;
        }
        private RelayCommand<object> closewindows;
        public ICommand Closewindows
        {
            get
            {
                if (closewindows == null)
                {
                    closewindows = new RelayCommand<object>(CloseAction);
                }
                return closewindows;
            }
        }
        public void CloseAction(object parameter)
        {
            App.Current.Dispatcher.Invoke(() => { windowsclose.Invoke(); });
        }

    }
}
