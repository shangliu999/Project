using ETexsys.WashingCabinet.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETexsys.WashingCabinet.Model
{
    public class MainWindowModel : ViewModelBase
    {
        private string _ICCode { get; set; }
        public string ICCode
        {
            get { return _ICCode; }
            set
            {
                _ICCode = value;
                this.RaisePropertyChanged("ICCode");
            }
        }

        private string netVisible { get; set; }
        public string NetVisible
        {
            get { return netVisible; }
            set
            {
                netVisible = value;
                this.RaisePropertyChanged("NetVisible");
            }
        }

        private string dirtVisible { get; set; }
        public string DirtVisible
        {
            get { return dirtVisible; }
            set
            {
                dirtVisible = value;
                RaisePropertyChanged("DirtVisible");
            }
        }

        private ObservableCollection<MessageTable> messageList;
        public ObservableCollection<MessageTable> MessageList
        {
            get { return messageList; }
            set
            {
                messageList = value;
                this.RaisePropertyChanged("MessageList");
            }
        }

        private ObservableCollection<DetailsTable> detailsList;
        public ObservableCollection<DetailsTable> DetailsList
        {
            get { return detailsList; }
            set
            {
                detailsList = value;
                this.RaisePropertyChanged("DetailsList");
            }
        }

        private ObservableCollection<ChooesTable> chooesList;
        public ObservableCollection<ChooesTable> ChooesList
        {
            get { return chooesList; }
            set
            {
                chooesList = value;
                RaisePropertyChanged("ChooesList");
            }
        }
    }

    public class MessageTable : ViewModelBase
    {
        private string time;
        public string Time
        {
            get { return time; }
            set
            {
                time = value;
                this.RaisePropertyChanged("Time");
            }
        }

        private string operatorName;
        public string OperatorName
        {
            get { return operatorName; }
            set
            {
                operatorName = value;
                this.RaisePropertyChanged("OperatorName");
            }
        }

        private string deliveryVisible;
        public string DeliveryVisible
        {
            get { return deliveryVisible; }
            set
            {
                deliveryVisible = value;
                this.RaisePropertyChanged("DeliveryVisible");
            }
        }

        private string receiveVisible;
        public string ReceiveVisible
        {
            get { return receiveVisible; }
            set
            {
                receiveVisible = value;
                this.RaisePropertyChanged("ReceiveVisible");
            }
        }
        
        private ObservableCollection<MessageDetails> messageDetailsList;
        public ObservableCollection<MessageDetails> MessageDetailsList
        {
            get { return messageDetailsList; }
            set
            {
                messageDetailsList = value;
                this.RaisePropertyChanged("MessageDetailsList");
            }
        }
    }

    public class DetailsTable : ViewModelBase
    {
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

        private int classCount;
        public int ClassCount
        {
            get { return classCount; }
            set
            {
                classCount = value;
                this.RaisePropertyChanged("ClassCount");
            }
        }

        private ObservableCollection<ChooesTable> chooesList;
        public ObservableCollection<ChooesTable> ChooesList
        {
            get { return chooesList; }
            set
            {
                chooesList = value;
                RaisePropertyChanged("ChooesList");
            }
        }

        private string visible1;
        public string Visible1
        {
            get { return visible1; }
            set
            {
                visible1 = value;
                this.RaisePropertyChanged("Visible1");
            }
        }

        private string visible2;
        public string Visible2
        {
            get { return visible2; }
            set
            {
                visible2 = value;
                this.RaisePropertyChanged("Visible2");
            }
        }
    }

    public class ChooesTable : ViewModelBase
    {
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

        private int sizeCount;
        public int SizeCount
        {
            get { return sizeCount; }
            set
            {
                sizeCount = value;
                this.RaisePropertyChanged("SizeCount");
            }
        }
    }

    public class MessageDetails : ViewModelBase
    {
        private string className;
        public string ClassName
        {
            get { return className; }
            set
            {
                className = value;
                RaisePropertyChanged("ClassName");
            }
        }

        private int classCount;
        public int ClassCount
        {
            get { return classCount; }
            set
            {
                classCount = value;
                RaisePropertyChanged("ClassCount");
            }
        }
    }
}
