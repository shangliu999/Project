﻿using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Settings;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.View.Choose;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Settings
{
    public class SystemSettingViewModel : ViewModelBase
    {
        private SystemSettingModel _model;
        public SystemSettingModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        public SystemSettingViewModel()
        {
            _model = new SystemSettingModel();
            _model.PrintPaperType = new ObservableCollection<PrintPaperModel>();
            _model.PrintPaperType.Add(new PrintPaperModel { Name = "58", Type = 1 });
            _model.PrintPaperType.Add(new PrintPaperModel { Name = "80", Type = 2 });
            _model.PrintPaperType.Add(new PrintPaperModel { Name = "240", Type = 3 });

            ObservableCollection<string> coms = new ObservableCollection<string>();
            RegistryKey keyCom = Registry.LocalMachine.OpenSubKey(@"Hardware\DeviceMap\SerialComm");
            if (keyCom != null)
            {
                string[] sSubKeys = keyCom.GetValueNames();
                foreach (string sName in sSubKeys)
                {
                    string sValue = (string)keyCom.GetValue(sName);
                    coms.Add(sValue);
                }
            }

            _model.Coms = coms;

            SystemSettingModel settingModel = ConfigController.GetSystemConfig();
            _model.SelectPrintPaper = settingModel.SelectPrintPaper - 1;
            _model.OpenMoveAntenna = settingModel.OpenMoveAntenna;
            _model.MoveAntennaCom = settingModel.MoveAntennaCom;
            _model.TruckIsRequired = settingModel.TruckIsRequired;
            _model.InFactoryPercentage = settingModel.InFactoryPercentage;
        }

        #region Action

        private void Save(string op)
        {
            ConfigController.SaveSystemConfig(this._model);

            if (ConfigController.SystemSettingConfig != null)
            {
                if (ConfigController.SystemSettingConfig.OpenMoveAntenna && ConfigController.SystemSettingConfig.MoveAntennaCom != null
                    && ConfigController.SystemSettingConfig.MoveAntennaCom != "")
                {
                    ComUitilities com = new ComUitilities();
                    App.TurnSP = com.Init(ConfigController.SystemSettingConfig.MoveAntennaCom);
                }
            }

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
