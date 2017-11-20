using Etextsys.Terminal.Domain;
using Etextsys.Terminal.Model.Settings;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Settings;
using ETextsys.Terminal.View.Choose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Settings
{
    public class BusinessSettingViewModel : ViewModelBase
    {
        private BusinessSettingModel model;
        public BusinessSettingModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        public BusinessSettingViewModel()
        {
            BusinessSettingModel bsModel = ConfigController.GetBusinessConfig();
            if (bsModel != null)
            {
                model = bsModel;
            }
            else
            {
                model = new Terminal.Model.Settings.BusinessSettingModel();
            }
        }

        #region Action

        private void Save(string op)
        {
            ConfigController.SaveBusinessConfig(this.model);

            EtexsysMessageBox.Show("提示", "设置成功.", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        #endregion

        #region commond


        private RelayCommand<string> _SubmitCommand;
        public ICommand SubmitCommand
        {
            get
            {
                if (_SubmitCommand == null)
                {
                    _SubmitCommand = new RelayCommand<string>(Save);
                }
                return _SubmitCommand;
            }
        }



        #endregion
    }
}
