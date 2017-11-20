using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETextsys.Terminal.Model.QuerySummary
{
    public class TextileDetailModel : ViewModelBase
    {
        private Visibility _textileAreaVisibiled;
        public Visibility TextileAreaVisibiled
        {
            get { return _textileAreaVisibiled; }
            set
            {
                _textileAreaVisibiled = value;
                this.RaisePropertyChanged("TextileAreaVisibiled");
            }
        }

        private Visibility _textileSingleVisibiled;
        public Visibility TextileSingleVisibiled
        {
            get { return _textileSingleVisibiled; }
            set
            {
                _textileSingleVisibiled = value;
                this.RaisePropertyChanged("TextileSingleVisibiled");
            }
        }

        private bool _singleBtnChoosed;
        public bool SingleBtnChoosed
        {
            get { return _singleBtnChoosed; }
            set
            {
                _singleBtnChoosed = value;
                this.RaisePropertyChanged("SingleBtnChoosed");
            }
        }

        private bool _areaBtnChoosed;
        public bool AreaBtnChoosed
        {
            get { return _areaBtnChoosed; }
            set
            {
                _areaBtnChoosed = value;
                this.RaisePropertyChanged("AreaBtnChoosed");
            }
        }

        private int _textileId;
        public int TextileId
        {
            get { return _textileId; }
            set
            {
                _textileId = value;
                this.RaisePropertyChanged("TextileId");
            }
        }

        private string _className;
        public string ClassName
        {
            get { return _className; }
            set
            {
                _className = value;
                this.RaisePropertyChanged("ClassName");
            }
        }

        private string _sizeName;
        public string SizeName
        {
            get { return _sizeName; }
            set
            {
                _sizeName = value;
                this.RaisePropertyChanged("SizeName");
            }
        }

        private string _costTime;
        public string CostTime
        {
            get { return _costTime; }
            set
            {
                _costTime = value;
                this.RaisePropertyChanged("CostTime");
            }
        }

        private int _textileWashtime;
        public int TextileWashtime
        {
            get { return _textileWashtime; }
            set
            {
                _textileWashtime = value;
                this.RaisePropertyChanged("TextileWashtime");
            }
        }

        private string _RFIDTagNo;
        public string RFIDTagNo
        {
            get { return _RFIDTagNo; }
            set
            {
                _RFIDTagNo = value;
                this.RaisePropertyChanged("RFIDTagNo");
            }
        }

        private int _RFIDWashtime;
        public int RFIDWashtime
        {
            get { return _RFIDWashtime; }
            set
            {
                _RFIDWashtime = value;
                this.RaisePropertyChanged("RFIDWashtime");
            }
        }

        private ObservableCollection<TextileFlowTableModel> _FlowTable;
        public ObservableCollection<TextileFlowTableModel> FlowTable
        {
            get { return _FlowTable; }
            set
            {
                _FlowTable = value;
                this.RaisePropertyChanged("FlowTable");
            }
        }
    }

    public class TextileFlowTableModel : ViewModelBase
    {
        private string _PositionName;
        public string PositionName
        {
            get { return _PositionName; }
            set
            {
                _PositionName = value;
                this.RaisePropertyChanged("PositionName");
            }
        }

        private string _FlowName;
        public string FlowName
        {
            get { return _FlowName; }
            set
            {
                _FlowName = value;
                this.RaisePropertyChanged("FlowName");
            }
        }

        private string _OperationUser;
        public string OperationUser
        {
            get { return _OperationUser; }
            set
            {
                _OperationUser = value;
                this.RaisePropertyChanged("OperationUser");
            }
        }

        private string _OperationTime;
        public string OperationTime
        {
            get { return _OperationTime; }
            set
            {
                _OperationTime = value;
                this.RaisePropertyChanged("OperationTime");
            }
        }
    }
}
