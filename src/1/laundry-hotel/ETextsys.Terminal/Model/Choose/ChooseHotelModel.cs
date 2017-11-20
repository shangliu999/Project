using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Model.Choose
{
    public class ChooseHotelModel : ViewModelBase
    {
        private ObservableCollection<ChooseModel> _ChooseList;
        public ObservableCollection<ChooseModel> ChooseList
        {
            get { return _ChooseList; }
            set
            {
                _ChooseList = value;
                this.RaisePropertyChanged("ChooseList");
            }
        }

        private ObservableCollection<ChooseModel> _AllChooseList;
        public ObservableCollection<ChooseModel> AllChooseList
        {
            get { return _AllChooseList; }
            set
            {
                _AllChooseList = value;
                this.RaisePropertyChanged("AllChooseList");
            }
        }

        private string _SearchText;
        public string SearchText
        {
            get { return _SearchText; }
            set
            {
                _SearchText = value;
                this.RaisePropertyChanged("SearchText");
            }
        }

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

        private bool _LeftEnabled;
        public bool LeftEnabled
        {
            get { return _LeftEnabled; }
            set
            {
                _LeftEnabled = value;
                this.RaisePropertyChanged("LeftEnabled");
            }
        }

        private bool _RightEnabled;
        public bool RightEnabled
        {
            get { return _RightEnabled; }
            set
            {
                _RightEnabled = value;
                this.RaisePropertyChanged("RightEnabled");
            }
        }

        private string _Page;
        public string Page
        {
            get { return _Page; }
            set
            {
                _Page = value;
                this.RaisePropertyChanged("Page");
            }
        }
    }
}
