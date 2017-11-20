using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Model.Choose
{
    public class ExecutedModalModel : ViewModelBase
    {
        private string _title;
        public string Title
        {
            get { return _title; }
            set
            {
                _title = value;
                this.RaisePropertyChanged("Title");
            }
        }

        private ObservableCollection<ExecutedTableModel> _ExecutedTable;
        public ObservableCollection<ExecutedTableModel> ExecutedTable
        {
            get { return _ExecutedTable; }
            set
            {
                _ExecutedTable = value;
                this.RaisePropertyChanged("ExecutedTable");
            }
        }

        private int _type;
        public int Type
        {
            get { return _type; }
            set
            {
                _type = value;
                this.RaisePropertyChanged("Type");
            }
        }

        private List<ResponseRFIDTagModel> _TagList;
        public List<ResponseRFIDTagModel> TagList
        {
            get { return _TagList; }
            set
            {
                _TagList = value;
                this.RaisePropertyChanged("TagList");
            }
        }
    }

    public class ExecutedTableModel : ViewModelBase
    {
        private string brandName;
        public string BrandName
        {
            get { return brandName; }
            set
            {
                brandName = value;
                this.RaisePropertyChanged("BrandName");
            }
        }

        private string className;
        public string ClassName
        {
            get { return className; }
            set
            {
                className = value;
                this.RaisePropertyChanged("ClassName");
            }
        }

        private string sizeName;
        public string SizeName
        {
            get { return sizeName; }
            set
            {
                sizeName = value;
                this.RaisePropertyChanged("SizeName");
            }
        }

        private string _RFID;
        public string RFID
        {
            get { return _RFID; }
            set
            {
                _RFID = value;
                this.RaisePropertyChanged("RFID");
            }
        }

        private string _updateTime;
        public string UpdateTime
        {
            get { return _updateTime; }
            set
            {
                _updateTime = value;
                this.RaisePropertyChanged("UpdateTime");
            }
        }

        private string _RegionName;
        public string RegionName
        {
            get { return _RegionName; }
            set
            {
                _RegionName = value;
                this.RaisePropertyChanged("RegionName");
            }
        }

    }
}
