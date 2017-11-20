using ETexsys.APIRequestModel;
using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETextsys.Terminal.Model.Choose
{
    public class DownModalModel : ViewModelBase
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
    }

    public class ChooseModel : ViewModelBase
    {
        private int chooseId;
        public int ChooseID
        {
            get { return chooseId; }
            set
            {
                chooseId = value;
                this.RaisePropertyChanged("ChooseID");
            }
        }

        private string chooseName;
        public string ChooseName
        {
            get { return chooseName; }
            set
            {
                chooseName = value;
                this.RaisePropertyChanged("ChooseName");
            }
        }

        #region 净物配送任务

        /// <summary>
        /// 是否选择
        /// </summary>
        private int isChoose;
        public int IsChoose
        {
            get { return isChoose; }
            set
            {
                isChoose = value;
                this.RaisePropertyChanged("IsChoose");
            }
        }

        /// <summary>
        /// 净物配送依据
        /// </summary>
        private int basisOnSend;
        public int BasisOnSend
        {
            get { return basisOnSend; }
            set
            {
                basisOnSend = value;
                this.RaisePropertyChanged("BasisOnSend");
            }
        }

        /// <summary>
        /// 酒店ID
        /// </summary>
        private int? regionMode;
        public int? RegionMode
        {
            get { return regionMode; }
            set
            {
                regionMode = value;
                this.RaisePropertyChanged("RegionMode");
            }
        }

        /// <summary>
        /// 酒店ID
        /// </summary>
        private int hotelID;
        public int HotelID
        {
            get { return hotelID; }
            set
            {
                hotelID = value;
                this.RaisePropertyChanged("HotelID");
            }
        }

        /// <summary>
        /// 楼层ID
        /// </summary>
        private int regionID;
        public int RegionID
        {
            get { return regionID; }
            set
            {
                regionID = value;
                this.RaisePropertyChanged("RegionID");
            }
        }

        /// <summary>
        /// 酒店名称
        /// </summary>
        private string hotelName;
        public string HotelName
        {
            get { return hotelName; }
            set
            {
                hotelName = value;
                this.RaisePropertyChanged("HotelName");
            }
        }

        /// <summary>
        /// 楼层名称
        /// </summary>
        private string regionName;
        public string RegionName
        {
            get { return regionName; }
            set
            {
                regionName = value;
                this.RaisePropertyChanged("RegionName");
            }
        }

        /// <summary>
        /// 净物配送依据
        /// </summary>
        private int taskCount;
        public int TaskCount
        {
            get { return taskCount; }
            set
            {
                taskCount = value;
                this.RaisePropertyChanged("TaskCount");
            }
        }

        #endregion
    }
}
