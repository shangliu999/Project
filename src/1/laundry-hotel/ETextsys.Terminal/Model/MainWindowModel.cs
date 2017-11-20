using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETextsys.Terminal.Model
{
    public class MainWindowModel : ViewModelBase
    {
        public MainWindowModel()
        {

        }

        private Visibility _LVisibility;
        public Visibility LVisibility
        {
            get { return _LVisibility; }
            set {
                _LVisibility = value;
                RaisePropertyChanged("LVisibility");
            }
        }

        private Visibility _RVisibility;
        public Visibility RVisibility
        {
            get { return _RVisibility; }
            set
            {
                _RVisibility = value;
                RaisePropertyChanged("RVisibility");
            }
        }

        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                _Title = value;
                RaisePropertyChanged("Title");
            }
        }
        
        private long process;
        public long Process
        {
            get { return process; }
            set
            {
                process = value;
                this.RaisePropertyChanged("Process");
            }
        }

        /// <summary>
        /// 左上
        /// </summary>
        private ObservableCollection<FuncModel> funcInFactory;
        /// <summary>
        /// 左上
        /// </summary>
        public ObservableCollection<FuncModel> FuncInFactory
        {
            get { return funcInFactory; }
            set
            {
                funcInFactory = value;
                this.RaisePropertyChanged("FuncInFactory");
            }
        }

        /// <summary>
        /// 右
        /// </summary>
        private ObservableCollection<FuncModel> funcInC;
        /// <summary>
        /// 右
        /// </summary>
        public ObservableCollection<FuncModel> FuncInC
        {
            get { return funcInC; }
            set
            {
                funcInC = value;
                this.RaisePropertyChanged("FuncInC");
            }
        }

        /// <summary>
        /// 查询
        /// </summary>
        private ObservableCollection<FuncModel> funcQ;
        /// <summary>
        /// 查询
        /// </summary>
        public ObservableCollection<FuncModel> FuncQ
        {
            get { return funcQ; }
            set
            {
                funcQ = value;
                this.RaisePropertyChanged("FuncQ");
            }
        }

        /// <summary>
        /// 快捷菜单
        /// </summary>
        private ObservableCollection<FuncModel> funcM;
        /// <summary>
        /// 快捷菜单
        /// </summary>
        public ObservableCollection<FuncModel> FuncM
        {
            get { return funcM; }
            set
            {
                funcM = value;
                this.RaisePropertyChanged("FuncM");
            }
        }

        private int _QTop;
        public int QTop
        {
            get { return _QTop; }
            set
            {
                _QTop = value;
                RaisePropertyChanged("QTop");
            }
        }

        private Visibility _WaitVisibled;
        public Visibility WaitVisibled
        {
            get { return _WaitVisibled; }
            set
            {
                _WaitVisibled = value;
                this.RaisePropertyChanged("WaitVisibled");
            }
        }

        private string _WaitContent;
        public string WaitContent
        {
            get { return _WaitContent; }
            set
            {
                _WaitContent = value;
                this.RaisePropertyChanged("WaitContent");
            }
        }
    }

    public class FuncModel
    {
        public string ID { get; set; }

        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public bool IsEnabled { get; set; }
    }
}
