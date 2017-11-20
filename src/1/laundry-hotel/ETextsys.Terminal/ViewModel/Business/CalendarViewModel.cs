using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Business;
using ETextsys.Terminal.Model.Choose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Business
{
   public class CalendarViewModel
    {

        Action _closeAction;

        Calendar _calendar;

        private CalendarModel model;
        public CalendarModel Model
        {
            get { return model; }
            set
            {
                model = value;
            }
        }

        private ChooseModel _chooseItem;
        public ChooseModel ChooseItem
        {
            get { return _chooseItem; }
            set
            {
                _chooseItem = value;
            }
        }
        
        public CalendarViewModel(Action close,Calendar calendar)
        {
            _closeAction = close;
            _calendar = calendar;
            model = new CalendarModel();
            calendar.SelectedDatesChanged += Calendar_GetDateTime;
            
        }

        public void Calendar_GetDateTime(object sender, EventArgs e)
        {
            model.Time = Convert.ToDateTime(_calendar.SelectedDate).ToString("yyyy-MM-dd");
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }


        private RelayCommand<Calendar> getToday;
        public ICommand GetToday
        { 
            get
            {
                if (getToday == null)
                {
                    getToday = new RelayCommand<Calendar>(GetTodayAction);
                }
                return getToday;
            }
        }
        public void GetTodayAction(Calendar calendar)
        {
            model.Time = DateTime.Now.ToString("yyyy-MM-dd");
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }


        private RelayCommand<Calendar> getYesterDay;
        public ICommand GetYesterDay
        {
            get
            {
                if (getYesterDay == null)
                {
                    getYesterDay = new RelayCommand<Calendar>(GetYesterDayAction);
                }
                return getYesterDay;
            }
        }
        public void GetYesterDayAction(Calendar calendar)
        {
            model.Time = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private RelayCommand<Calendar> theDayBeforeYesterday;
        public ICommand TheDayBeforeYesterday
        {
            get
            {
                if (theDayBeforeYesterday == null)
                {
                    theDayBeforeYesterday = new RelayCommand<Calendar>(TheDayBeforeYesterdayAction);
                }
                return theDayBeforeYesterday;
            }
        }
        public void TheDayBeforeYesterdayAction(Calendar calendar)
        {
            model.Time = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }
        

        private RelayCommand<object> closeWindows;
        public ICommand CloseWindows
        {
            get
            {
                if (closeWindows==null)
                {
                    closeWindows = new RelayCommand<object>(CloseAction);
                }
                return closeWindows;
            }
        }
        public void CloseAction(object paramte)
        {
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }
    }
}
