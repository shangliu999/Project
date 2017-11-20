using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Model.Choose
{
    public class TaskDetailsModel : ViewModelBase
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

        private ObservableCollection<TaskDetailsTableModel> _TaskDetailsTable;
        public ObservableCollection<TaskDetailsTableModel> TaskDetailsTable
        {
            get { return _TaskDetailsTable; }
            set
            {
                _TaskDetailsTable = value;
                this.RaisePropertyChanged("TaskDetailsTable");
            }
        }
    }

    public class TaskDetailsTableModel : ViewModelBase
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
