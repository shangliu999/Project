using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Model.Settings
{
    public class SystemSettingModel : ViewModelBase
    {
        private ObservableCollection<PrintPaperModel> _printPaperType;
        public ObservableCollection<PrintPaperModel> PrintPaperType
        {
            get { return _printPaperType; }
            set
            {
                _printPaperType = value;
                this.RaisePropertyChanged("PrintPaperType");
            }
        }


        private int _selectPrintPaper;
        public int SelectPrintPaper
        {
            get { return _selectPrintPaper; }
            set
            {
                _selectPrintPaper = value;
                this.RaisePropertyChanged("SelectPrintPaper");
            }
        }

        private bool _openMoveAntenna;
        public bool OpenMoveAntenna
        {
            get { return _openMoveAntenna; }
            set
            {
                _openMoveAntenna = value;
                this.RaisePropertyChanged("OpenMoveAntenna");
            }
        }

        private string _moveAntennaCom;
        public string MoveAntennaCom
        {
            get { return _moveAntennaCom; }
            set
            {
                _moveAntennaCom = value;
                this.RaisePropertyChanged("MoveAntennaCom");
            }
        }

        private ObservableCollection<string> _coms;
        public ObservableCollection<string> Coms
        {
            get { return _coms; }
            set
            {
                _coms = value;
                this.RaisePropertyChanged("Coms");
            }
        }

        private bool _truckIsRequired = true;
        public bool TruckIsRequired
        {
            get { return _truckIsRequired; }
            set
            {
                _truckIsRequired = value;
                this.RaisePropertyChanged("TruckIsRequired");
            }
        }

        private int _InFactoryPercentage = 90;
        public int InFactoryPercentage
        {
            get { return _InFactoryPercentage; }
            set
            {
                _InFactoryPercentage = value;
                this.RaisePropertyChanged("InFactoryPercentage");
            }
        }
    }

    public class PrintPaperModel : ViewModelBase
    {
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

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                this.RaisePropertyChanged("Name");
            }
        }
    }
}
