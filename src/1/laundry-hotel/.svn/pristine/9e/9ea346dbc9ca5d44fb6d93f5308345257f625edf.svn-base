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
    public class TruckModalModel : ViewModelBase
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

        private ObservableCollection<TruckTableModel> _TruckTable;
        public ObservableCollection<TruckTableModel> TruckTable
        {
            get { return _TruckTable; }
            set
            {
                _TruckTable = value;
                this.RaisePropertyChanged("TruckTable");
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
    public class TruckTableModel : ViewModelBase
    {
        private string truckRFID;
        public string TruckRFID
        {
            get { return truckRFID; }
            set
            {
                truckRFID = value;
                this.RaisePropertyChanged("TruckRFID");
            }
        }

        private int textileCount;
        public int TextileCount
        {
            get { return textileCount; }
            set
            {
                textileCount = value;
                this.RaisePropertyChanged("TextileCount");
            }
        }
    }
}
