using Etextsys.Terminal.Domain;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Settings;
using ETextsys.Terminal.View.Choose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Settings
{
    public class SystemBGViewModel : ViewModelBase
    {
        private SystemBGModel _model;
        public SystemBGModel Model
        {
            get { return _model; }
            set
            {
                _model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        public SystemBGViewModel()
        {
            this._model = new SystemBGModel();
            this._model.HomePageChoosed = true;
            this._model.HomeVisibility = System.Windows.Visibility.Visible;
            this._model.InPageVisibility = System.Windows.Visibility.Collapsed;


            switch (App.HomePageDictionary)
            {
                case "HomeTheme1":
                    _model.Home1Choosed = true;
                    break;
                case "HomeTheme2":
                    _model.Home2Choosed = true;
                    break;
                case "HomeTheme3":
                    _model.Home3Choosed = true;
                    break;
                case "HomeTheme4":
                    _model.Home4Choosed = true;
                    break;
                case "HomeTheme5":
                    _model.Home5Choosed = true;
                    break;
                case "HomeTheme6":
                    _model.Home6Choosed = true;
                    break;
                case "HomeTheme7":
                    _model.Home7Choosed = true;
                    break;
                case "HomeTheme8":
                    _model.Home8Choosed = true;
                    break;
            }
            switch (App.InPageDictionary)
            {
                case "InPageTheme1":
                    _model.InPage1Choosed = true;
                    break;
                case "InPageTheme2":
                    _model.InPage2Choosed = true;
                    break;
                case "InPageTheme3":
                    _model.InPage3Choosed = true;
                    break;
                case "InPageTheme4":
                    _model.InPage4Choosed = true;
                    break;
                case "InPageTheme5":
                    _model.InPage5Choosed = true;
                    break;
                case "InPageTheme6":
                    _model.InPage6Choosed = true;
                    break;
                case "InPageTheme7":
                    _model.InPage7Choosed = true;
                    break;
                case "InPageTheme8":
                    _model.InPage8Choosed = true;
                    break;
            }
        }

        #region Action

        private void SubmitAction(string sender)
        {
            string homePageValue = null;
            if (_model.Home1Choosed)
            {
                homePageValue = "HomeTheme1";
            }
            else if (_model.Home2Choosed)
            {
                homePageValue = "HomeTheme2";
            }
            else if (_model.Home3Choosed)
            {
                homePageValue = "HomeTheme3";
            }
            else if (_model.Home4Choosed)
            {
                homePageValue = "HomeTheme4";
            }
            else if (_model.Home5Choosed)
            {
                homePageValue = "HomeTheme5";
            }
            else if (_model.Home6Choosed)
            {
                homePageValue = "HomeTheme6";
            }
            else if (_model.Home7Choosed)
            {
                homePageValue = "HomeTheme7";
            }
            else if (_model.Home8Choosed)
            {
                homePageValue = "HomeTheme8";
            }

            string inPageValue = null;
            if (_model.InPage1Choosed)
            {
                inPageValue = "InPageTheme1";
            }
            else if (_model.InPage2Choosed)
            {
                inPageValue = "InPageTheme2";
            }
            else if (_model.InPage3Choosed)
            {
                inPageValue = "InPageTheme3";
            }
            else if (_model.InPage4Choosed)
            {
                inPageValue = "InPageTheme4";
            }
            else if (_model.InPage5Choosed)
            {
                inPageValue = "InPageTheme5";
            }
            else if (_model.InPage6Choosed)
            {
                inPageValue = "InPageTheme6";
            }
            else if (_model.InPage7Choosed)
            {
                inPageValue = "InPageTheme7";
            }
            else if (_model.InPage8Choosed)
            {
                inPageValue = "InPageTheme8";
            }
            ConfigController.SaveSystemDictionary(homePageValue, inPageValue);
            App.HomePageDictionary = homePageValue;
            App.InPageDictionary = inPageValue;

            if (!string.IsNullOrEmpty(homePageValue))
            {
                App.Current.Resources["MainWinBg"] = App.Current.Resources[homePageValue];
            }
            if (!string.IsNullOrEmpty(inPageValue))
            {
                App.Current.Resources["FuncWinBg"] = App.Current.Resources[inPageValue];
            }

            EtexsysMessageBox.Show("提示", "设置成功.", MessageBoxButton.OK, MessageBoxImage.Information);

        }
        private void InPageChooseAction(string sender)
        {
            if (sender == "InPageTheme1")
            {
                _model.InPage1Choosed = true;
                _model.InPage2Choosed = false;
                _model.InPage3Choosed = false;
                _model.InPage4Choosed = false;
                _model.InPage5Choosed = false;
                _model.InPage6Choosed = false;
                _model.InPage7Choosed = false;
                _model.InPage8Choosed = false;
            }
            else if (sender == "InPageTheme2")
            {
                _model.InPage1Choosed = false;
                _model.InPage2Choosed = true;
                _model.InPage3Choosed = false;
                _model.InPage4Choosed = false;
                _model.InPage5Choosed = false;
                _model.InPage6Choosed = false;
                _model.InPage7Choosed = false;
                _model.InPage8Choosed = false;
            }
            else if (sender == "InPageTheme3")
            {
                _model.InPage1Choosed = false;
                _model.InPage2Choosed = false;
                _model.InPage3Choosed = true;
                _model.InPage4Choosed = false;
                _model.InPage5Choosed = false;
                _model.InPage6Choosed = false;
                _model.InPage7Choosed = false;
                _model.InPage8Choosed = false;
            }
            else if (sender == "InPageTheme4")
            {
                _model.InPage1Choosed = false;
                _model.InPage2Choosed = false;
                _model.InPage3Choosed = false;
                _model.InPage4Choosed = true;
                _model.InPage5Choosed = false;
                _model.InPage6Choosed = false;
                _model.InPage7Choosed = false;
                _model.InPage8Choosed = false;
            }
            else if (sender == "InPageTheme5")
            {
                _model.InPage1Choosed = false;
                _model.InPage2Choosed = false;
                _model.InPage3Choosed = false;
                _model.InPage4Choosed = false;
                _model.InPage5Choosed = true;
                _model.InPage6Choosed = false;
                _model.InPage7Choosed = false;
                _model.InPage8Choosed = false;
            }
            else if (sender == "InPageTheme6")
            {
                _model.InPage1Choosed = false;
                _model.InPage2Choosed = false;
                _model.InPage3Choosed = false;
                _model.InPage4Choosed = false;
                _model.InPage5Choosed = false;
                _model.InPage6Choosed = true;
                _model.InPage7Choosed = false;
                _model.InPage8Choosed = false;
            }
            else if (sender == "InPageTheme7")
            {
                _model.InPage1Choosed = false;
                _model.InPage2Choosed = false;
                _model.InPage3Choosed = false;
                _model.InPage4Choosed = false;
                _model.InPage5Choosed = false;
                _model.InPage6Choosed = false;
                _model.InPage7Choosed = true;
                _model.InPage8Choosed = false;
            }
            else if (sender == "InPageTheme8")
            {
                _model.InPage1Choosed = false;
                _model.InPage2Choosed = false;
                _model.InPage3Choosed = false;
                _model.InPage4Choosed = false;
                _model.InPage5Choosed = false;
                _model.InPage6Choosed = false;
                _model.InPage7Choosed = false;
                _model.InPage8Choosed = true;
            }
        }

        private void HomeChooseAction(string sender)
        {
            if (sender == "HomeTheme1")
            {
                _model.Home1Choosed = true;
                _model.Home2Choosed = false;
                _model.Home3Choosed = false;
                _model.Home4Choosed = false;
                _model.Home5Choosed = false;
                _model.Home6Choosed = false;
                _model.Home7Choosed = false;
                _model.Home8Choosed = false;
            }
            else if (sender == "HomeTheme2")
            {
                _model.Home1Choosed = false;
                _model.Home2Choosed = true;
                _model.Home3Choosed = false;
                _model.Home4Choosed = false;
                _model.Home5Choosed = false;
                _model.Home6Choosed = false;
                _model.Home7Choosed = false;
                _model.Home8Choosed = false;
            }
            else if (sender == "HomeTheme3")
            {
                _model.Home1Choosed = false;
                _model.Home2Choosed = false;
                _model.Home3Choosed = true;
                _model.Home4Choosed = false;
                _model.Home5Choosed = false;
                _model.Home6Choosed = false;
                _model.Home7Choosed = false;
                _model.Home8Choosed = false;
            }
            else if (sender == "HomeTheme4")
            {
                _model.Home1Choosed = false;
                _model.Home2Choosed = false;
                _model.Home3Choosed = false;
                _model.Home4Choosed = true;
                _model.Home5Choosed = false;
                _model.Home6Choosed = false;
                _model.Home7Choosed = false;
                _model.Home8Choosed = false;
            }
            else if (sender == "HomeTheme5")
            {
                _model.Home1Choosed = false;
                _model.Home2Choosed = false;
                _model.Home3Choosed = false;
                _model.Home4Choosed = false;
                _model.Home5Choosed = true;
                _model.Home6Choosed = false;
                _model.Home7Choosed = false;
                _model.Home8Choosed = false;
            }
            else if (sender == "HomeTheme6")
            {
                _model.Home1Choosed = false;
                _model.Home2Choosed = false;
                _model.Home3Choosed = false;
                _model.Home4Choosed = false;
                _model.Home5Choosed = false;
                _model.Home6Choosed = true;
                _model.Home7Choosed = false;
                _model.Home8Choosed = false;
            }
            else if (sender == "HomeTheme7")
            {
                _model.Home1Choosed = false;
                _model.Home2Choosed = false;
                _model.Home3Choosed = false;
                _model.Home4Choosed = false;
                _model.Home5Choosed = false;
                _model.Home6Choosed = false;
                _model.Home7Choosed = true;
                _model.Home8Choosed = false;
            }
            else if (sender == "HomeTheme8")
            {
                _model.Home1Choosed = false;
                _model.Home2Choosed = false;
                _model.Home3Choosed = false;
                _model.Home4Choosed = false;
                _model.Home5Choosed = false;
                _model.Home6Choosed = false;
                _model.Home7Choosed = false;
                _model.Home8Choosed = true;
            }
        }

        private void TabAction(string sender)
        {
            if (sender == "HomePage")
            {
                this._model.HomePageChoosed = true;
                this._model.InPageChoosed = false;
                this._model.HomeVisibility = System.Windows.Visibility.Visible;
                this._model.InPageVisibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                this._model.HomePageChoosed = false;
                this._model.InPageChoosed = true;
                this._model.HomeVisibility = System.Windows.Visibility.Collapsed;
                this._model.InPageVisibility = System.Windows.Visibility.Visible;
            }
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
                    _SubmitCommand = new RelayCommand<string>(SubmitAction);
                }
                return _SubmitCommand;
            }
        }

        private RelayCommand<string> _InPageChooseCommand;
        public ICommand InPageChooseCommand
        {
            get
            {
                if (_InPageChooseCommand == null)
                {
                    _InPageChooseCommand = new RelayCommand<string>(InPageChooseAction);
                }
                return _InPageChooseCommand;
            }
        }

        private RelayCommand<string> _HomeChooseCommand;
        public ICommand HomeChooseCommand
        {
            get
            {
                if (_HomeChooseCommand == null)
                {
                    _HomeChooseCommand = new RelayCommand<string>(HomeChooseAction);
                }
                return _HomeChooseCommand;
            }
        }

        private RelayCommand<string> _TabCommand;
        public ICommand TabCommand
        {
            get
            {
                if (_TabCommand == null)
                {
                    _TabCommand = new RelayCommand<string>(TabAction);
                }
                return _TabCommand;
            }
        }

        #endregion
    }
}
