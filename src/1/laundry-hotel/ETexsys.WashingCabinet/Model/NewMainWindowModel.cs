using ETexsys.WashingCabinet.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ETexsys.WashingCabinet.Model
{
    public class NewMainWindowModel : ViewModelBase
    {
        /// <summary>
        /// IC 卡号
        /// </summary>
        private string _ICCode;
        public string ICCode
        {
            get { return _ICCode; }
            set
            {
                _ICCode = value;
                this.RaisePropertyChanged("ICCode");
            }
        }

        /// <summary>
        /// 衣仓内文本头
        /// </summary>
        private string _subTitle;
        public string SubTitle
        {
            get { return _subTitle; }
            set
            {
                _subTitle = value;
                this.RaisePropertyChanged("SubTitle");
            }
        }

        /// <summary>
        /// 衣仓内文本颜色
        /// </summary>
        private string _subTitleColor;
        public string SubTitleColor
        {
            get { return _subTitleColor; }
            set
            {
                _subTitleColor = value;
                this.RaisePropertyChanged("SubTitleColor");
            }
        }

        /// <summary>
        /// 切换按钮文本
        /// </summary>
        private string _changeBtnText;
        public string ChangeBtnText
        {
            get { return _changeBtnText; }
            set
            {
                _changeBtnText = value;
                this.RaisePropertyChanged("ChangeBtnText");
            }
        }

        private Visibility _cleanStoreVisibility;
        public Visibility CleanStoreVisibility
        {
            get { return _cleanStoreVisibility; }
            set
            {
                _cleanStoreVisibility = value;
                this.RaisePropertyChanged("CleanStoreVisibility");
            }
        }

        private Visibility _dirtStoreVisibility;
        public Visibility DirtStoreVisibility
        {
            get { return _dirtStoreVisibility; }
            set
            {
                _dirtStoreVisibility = value;
                this.RaisePropertyChanged("DirtStoreVisibility");
            }
        }

        private List<CostReceiveRecordsModel> _costReceiveRecordsList;
        public List<CostReceiveRecordsModel> CostReceiveRecordsList
        {
            get { return _costReceiveRecordsList; }
            set
            {
                _costReceiveRecordsList = value;
                this.RaisePropertyChanged("CostReceiveRecordsList");
            }
        }

        private List<TextileStoreModel> _textileStoreList;
        public List<TextileStoreModel> TextileStoreList
        {
            get { return _textileStoreList; }
            set
            {
                _textileStoreList = value;
                this.RaisePropertyChanged("TextileStoreList");
            }
        }

        private List<TextileStoreModel> _dirtTextileList;
        public List<TextileStoreModel> DirtTextileList
        {
            get { return _dirtTextileList; }
            set
            {
                _dirtTextileList = value;
                this.RaisePropertyChanged("DirtTextileList");
            }
        }
    }

    /// <summary>
    /// 纺织品衣仓数据
    /// </summary>
    public class TextileStoreModel : ViewModelBase
    {
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

        private int _textileCount;
        public int TextileCount
        {
            get { return _textileCount; }
            set
            {
                _textileCount = value;
                this.RaisePropertyChanged("TextileCount");
            }
        }

        private Visibility _sizeVisibility;
        public Visibility SizeVisibility
        {
            get { return _sizeVisibility; }
            set
            {
                _sizeVisibility = value;
                this.RaisePropertyChanged("SizeVisibility");
            }
        }

        private Visibility _noSizeVisibility;
        public Visibility NoSizeVisibility
        {
            get { return _noSizeVisibility; }
            set
            {
                _noSizeVisibility = value;
                this.RaisePropertyChanged("NoSizeVisibility");
            }
        }

        private List<TextileModel> _textileDetail;
        public List<TextileModel> TextileDetail
        {
            get { return _textileDetail; }
            set
            {
                _textileDetail = value;
                this.RaisePropertyChanged("TextileDetail");
            }
        }

    }

    /// <summary>
    /// 投放领取记录
    /// </summary>
    public class CostReceiveRecordsModel : ViewModelBase
    {
        /// <summary>
        /// 时间
        /// </summary>
        private string _time;
        public string Time
        {
            get { return _time; }
            set
            {
                _time = value;
                this.RaisePropertyChanged("Time");
            }
        }

        /// <summary>
        /// 操作员
        /// </summary>
        private string _operatorName;
        public string OperatorName
        {
            get { return _operatorName; }
            set
            {
                _operatorName = value;
                this.RaisePropertyChanged("OperatorName");
            }
        }

        /// <summary>
        /// 状态
        /// </summary>
        private string _stateName;
        public string StateName
        {
            get { return _stateName; }
            set
            {
                _stateName = value;
                this.RaisePropertyChanged("StateName");
            }
        }

        private string _stateColor;
        public string StateColor
        {
            get { return _stateColor; }
            set
            {
                _stateColor = value;
                this.RaisePropertyChanged("StateColor");
            }
        }

        /// <summary>
        /// 详情
        /// </summary>
        private List<TextileModel> _recordsDetail;
        public List<TextileModel> RecordsDetail
        {
            get { return _recordsDetail; }
            set
            {
                _recordsDetail = value;
                this.RaisePropertyChanged("RecordsDetail");
            }
        }
    }

    /// <summary>
    /// 纺织品信息
    /// </summary>
    public class TextileModel : ViewModelBase
    {
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

        private int _textileCount;
        public int TextileCount
        {
            get { return _textileCount; }
            set
            {
                _textileCount = value;
                this.RaisePropertyChanged("TextileCount");
            }
        }
    }
}
