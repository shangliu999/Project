using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.View.Choose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ETextsys.Terminal.ViewModel.Choose
{
    public class ChooseBagViewModel : ViewModelBase
    {
        Action _closeAction;

        private ChooseBagModel model;
        public ChooseBagModel Model
        {
            get { return model; }
            set
            {
                model = value;
                this.RaisePropertyChanged("Model");
            }
        }

        /// <summary>
        /// 所有包号
        /// </summary>
        private List<ChooseModel> alllist;

        /// <summary>
        /// 一页显示条数
        /// </summary>
        private int PageCount = 20;

        /// <summary>
        /// 当前页码
        /// </summary>
        private int CurrentPageIndex = 1;

        /// <summary>
        /// 总条数
        /// </summary>
        private int PageTotal = 0;

        /// <summary>
        /// 总页码
        /// </summary>
        private int PageIndexTotal = 0;

        public ChooseBagViewModel(Action closeAction)
        {
            this._closeAction = closeAction;
            this.model = new ChooseBagModel();
        }

        public async void Bag_ContentRendered(object sender, EventArgs e)
        {
            alllist = new List<ChooseModel>();
            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.BagList, null);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    var bag = JsonConvert.DeserializeObject<List<ResponseBagModel>>(apiRtn.Result.ToString());
                    if (bag != null)
                    {
                        bag.ForEach(q =>
                        {
                            alllist.Add(new ChooseModel() { ChooseID = q.ID, ChooseName = q.BagNo });
                        });

                        //默认加载已选择的
                        ChooseModel cm = null;
                        for (int i = 0; i < model.AllChooseList.Count; i++)
                        {
                            cm = model.AllChooseList[i];
                            ChooseModel chooseModel = alllist.SingleOrDefault(c => c.ChooseName.Equals(cm.ChooseName));
                            if (chooseModel != null)
                            {
                                cm.ChooseID = chooseModel.ChooseID;
                            }
                        }
                        RefreshPage();
                    }
                }
            }
        }

        public void RefreshPage()
        {
            PageTotal = this.model.AllChooseList.Count;

            PageIndexTotal = PageTotal % PageCount == 0 ? PageTotal / PageCount : (PageTotal / PageCount) + 1;

            var query = this.model.AllChooseList.Skip((CurrentPageIndex - 1) * PageCount).Take(PageCount);

            if (CurrentPageIndex < PageIndexTotal)
            {
                this.model.RightEnabled = true;
            }
            else
            {
                this.model.RightEnabled = false;
            }

            if (CurrentPageIndex > 1)
            {
                this.model.LeftEnabled = true;
            }
            else
            {
                this.model.LeftEnabled = false;
            }

            this.model.ChooseList.Clear();
            ChooseModel chooseModel = null;
            query.ToList().ForEach(q =>
            {
                chooseModel = new ChooseModel();
                chooseModel.ChooseID = q.ChooseID;
                //string a = "{0}{1}{2}";
                //chooseModel.ChooseName = string.Format(a, q.ChooseName.Substring(0, 12), Environment.NewLine, q.ChooseName.Substring(12, 12)); ;
                chooseModel.ChooseName = q.ChooseName;
                this.model.ChooseList.Add(chooseModel);
            });
        }

        #region Action

        private void Add(string sender)
        {
            ChooseModel chooseModel = alllist.SingleOrDefault(c => c.ChooseName.Equals(sender));
            if (chooseModel == null)
            {
                EtexsysMessageBox.Show("提示", "包号不存在", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            else
            {
                ChooseModel chooseModel1 = model.AllChooseList.SingleOrDefault(c => c.ChooseName.Equals(sender));
                if (chooseModel1 == null)
                {
                    model.AllChooseList.Insert(0, chooseModel);
                    RefreshPage();
                }
                else
                {
                    EtexsysMessageBox.Show("提示", "包号已经添加到列表中", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        private void Submit(object sender)
        {
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private void CloseModalAction(string sender)
        {
            App.Current.Dispatcher.Invoke((Action)(() => { this._closeAction.Invoke(); }));
        }

        private void SelectedItemAction(object sender)
        {
            Button btn = sender as Button;

            string bagno = btn.Content.ToString();
            string tag = btn.Tag.ToString();

            int bagid = 0;
            int.TryParse(tag, out bagid);

            bool? isok = EtexsysMessageBox.Show("删除提示", "确定删除该包号吗？", MessageBoxButton.OKCancel, MessageBoxImage.Question);
            if (isok == true)
            {
                ChooseModel chooseModel = model.AllChooseList.FirstOrDefault(c => c.ChooseID.Equals(bagid));
                if (chooseModel != null)
                {
                    model.AllChooseList.Remove(chooseModel);
                    RefreshPage();
                }
            }
        }

        private void LeftAction(object sender)
        {
            CurrentPageIndex--;
            RefreshPage();
        }

        private void RightAction(object sender)
        {
            CurrentPageIndex++;
            RefreshPage();
        }

        #endregion

        #region Command


        private RelayCommand<string> _CloseModal;
        public ICommand CloseModal
        {
            get
            {
                if (_CloseModal == null)
                {
                    _CloseModal = new RelayCommand<string>(CloseModalAction);
                }
                return _CloseModal;
            }
        }

        private RelayCommand<string> _AddCommand;
        public ICommand AddCommand
        {
            get
            {
                if (_AddCommand == null)
                {
                    _AddCommand = new RelayCommand<string>(Add);
                }
                return _AddCommand;
            }
        }

        private RelayCommand<object> _SubmitCommand;
        public ICommand SubmitCommand
        {
            get
            {
                if (_SubmitCommand == null)
                {
                    _SubmitCommand = new RelayCommand<object>(Submit);
                }
                return _SubmitCommand;
            }
        }

        private RelayCommand<object> _SelectionChangedCmd;
        public ICommand SelectionChangedCmd
        {
            get
            {
                if (_SelectionChangedCmd == null)
                {
                    _SelectionChangedCmd = new RelayCommand<object>(SelectedItemAction);
                }
                return _SelectionChangedCmd;
            }
        }

        private RelayCommand<object> _LeftCommand;
        public ICommand LeftCommand
        {
            get
            {
                if (_LeftCommand == null)
                {
                    _LeftCommand = new RelayCommand<object>(LeftAction);
                }
                return _LeftCommand;
            }
        }

        private RelayCommand<object> _RightCommand;
        public ICommand RightCommand
        {
            get
            {
                if (_RightCommand == null)
                {
                    _RightCommand = new RelayCommand<object>(RightAction);
                }
                return _RightCommand;
            }
        }

        #endregion
    }
}
