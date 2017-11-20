using ETexsys.RFIDServer.Model;
using ETexsys.APIRequestModel.Response;
using Etextsys.Terminal.Domain;
using Etextsys.Terminal.Model.Settings;
using ETextsys.Terminal.DAL;
using ETextsys.Terminal.Domain;
using ETextsys.Terminal.Model;
using ETextsys.Terminal.Model.Choose;
using ETextsys.Terminal.Utilities;
using ETextsys.Terminal.View.Choose;
using ETextsys.Terminal.ViewModel.Choose;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Diagnostics;

namespace ETextsys.Terminal.ViewModel
{
    public class LoginViewModel : ViewModelBase
    {
        private LoginModel model;
        public LoginModel Model
        {
            get { return model; }
            set { model = value; }
        }

        private Action _closeWindow;

        private RelayCommand<object> checkBoxstate;
        public ICommand CheckBoxstate
        {
            get
            {
                if (checkBoxstate == null)
                {
                    checkBoxstate = new RelayCommand<object>(CheckBoxStateAction);
                }
                return checkBoxstate;
            }
        }
        public void CheckBoxStateAction(object pramet)
        {
            if (model.Checkboximage == 0.0)
            {
                model.Checkboximage = 1;
                model.UnCheckboximage = 0.0;
                model.Checkbox = true;
            }
            else
            {
                model.Checkboximage = 0.0;
                model.UnCheckboximage = 1;
                model.Checkbox = false;
            }
        }

        private RelayCommand<PasswordBox> showUser;
        public ICommand ShowUser
        {
            get
            {
                if (showUser == null)
                {
                    showUser = new RelayCommand<PasswordBox>(ShowUserAction);
                }
                return showUser;
            }
        }
        public async void ShowUserAction(PasswordBox Password)
        {
            model.WaitVisibled = "Visible";
            model.WaitContent = "玩命加载中";
            ObservableCollection<ChooseModel> list = new ObservableCollection<ChooseModel>();
            ChooseModel choosemodel = null;
            choosemodel = new ChooseModel();
            int appId = 2;
            var apiRtn = await ApiController.Instance.DoPost(ApiController.Instance.GetUserList, appId);
            if (apiRtn.ResultCode == 0)
            {
                if (apiRtn.Result != null)
                {
                    List<sys_user> objs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<sys_user>>(apiRtn.Result.ToString());
                    foreach (var item in objs)
                    {
                        choosemodel = new ChooseModel();
                        choosemodel.ChooseID = item.UserID;
                        choosemodel.ChooseName = item.LoginName;
                        list.Add(choosemodel);
                    }
                }
            }
            model.WaitVisibled = "Hidden";
            DownModal modal = new Terminal.View.Choose.DownModal();
            DownModalViewModel modalModel = new DownModalViewModel(modal.Close);
            modalModel.Model.ChooseList = list;
            modalModel.Model.Title = "选择用户名：";
            modal.DataContext = modalModel;
            modal.ShowDialog();
            if (modalModel.ChooseItem != null)
            {
                model.LoginName = modalModel.ChooseItem.ChooseName;

                List<LoginModel> listloginmodel = ConfigController.GetLoginModelConfig();
                foreach (LoginModel item1 in listloginmodel)
                {
                    if (item1.LoginName == modalModel.ChooseItem.ChooseName)
                    {
                        Password.Password = item1.LoginPwd;
                        this.model.Checkboximage = 1;
                        this.model.Checkbox = true;
                    }
                    else
                    {
                        Password.Password = string.Empty;
                        this.model.Checkboximage = 0.0;
                        this.model.Checkbox = false;
                        model.UnCheckboximage = 1;

                    }
                }
            }
            else
            {
                Password.Password = string.Empty;
            }
        }

        private RelayCommand<Window> close;
        public ICommand Close
        {
            get
            {
                if (close == null)
                {
                    close = new RelayCommand<Window>(CloseAction);
                }
                return close;
            }
        }
        public void CloseAction(Window windows)
        {
            Environment.Exit(0);
        }

        private RelayCommand<PasswordBox> login;
        public ICommand Login
        {
            get
            {
                if (login == null)
                {
                    login = new RelayCommand<PasswordBox>(LoginAction);
                }
                return login;
            }
        }
        public async void LoginAction(PasswordBox password)
        {
            model.WaitVisibled = "Visible";
            model.WaitContent = "玩命加载中";
            if (model.LoginName == "")
            {
                model.WaitVisibled = "Hidden";
                EtexsysMessageBox.Show("提示", "用户名不能为空", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            if (password.Password == "")
            {
                model.WaitVisibled = "Hidden";
                EtexsysMessageBox.Show("提示", "密码不能为空", MessageBoxButton.OK, MessageBoxImage.Information);
                model.Open = true;
                return;
            }
            else
            {
                model.LoginPwd = GetMD5(password.Password);
                model.TimeStamp = ApiController.Instance.GetTimeStamp();
                model.UUID = ConfigController.MacCode;
                model.TerminalType = ConfigController.TerminalType;

                var m = await ApiController.Instance.DoLogin(ApiController.Instance.Login, model);
                if (m.ResultCode == 1)
                {
                    model.WaitVisibled = "Hidden";
                    string mag = m.ResultMsg;
                    if (mag == null)
                    {
                        EtexsysMessageBox.Show("提示", "服务器异常", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        EtexsysMessageBox.Show("提示", mag, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    return;
                }
                else
                {
                    if (m.Result != null)
                    {
                        var userModel = JsonConvert.DeserializeObject<ResponseSysUserModel>(m.Result.ToString());
                        App.CurrentLoginUser = userModel;
                        model.LableVisibility = "Hidden";
                        if (model.Checkbox == true)
                        {
                            model.Password = password;
                            ConfigController.SaveLoginModelConfig(model);
                        }
                        else
                        {
                            ConfigController.ClearLoginModelConfig();
                        }

                        List<ResponseSysRightModel> parentList = userModel.SysRights.Where(c => c.RightParentID == null).ToList();
                        if (parentList == null || parentList.Count == 0)
                        {
                            model.WaitVisibled = "Hidden";
                            EtexsysMessageBox.Show("无权访问", "该用户未分配终端权限", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        CloseSoftBoard();

                        MainWindowController.SysRights = userModel.SysRights;
                        MainWindowController.SysCustomer = userModel.SysCustomer;
                        MainWindowViewModel mwvm = new MainWindowViewModel();
                        MainWindowController.Instance.Closing += mwvm.RequestClose;
                        MainWindowController.Instance.DataContext = mwvm;

                        MainWindowController.Instance.Show();

                        model.WaitVisibled = "Hidden";
                        _closeWindow.Invoke();
                    }
                }
            }
        }

        /// <summary>
        /// 关闭软键盘
        /// </summary>
        private void CloseSoftBoard()
        {
            try
            {
                Process[] process = Process.GetProcessesByName("SoftBoard");
                if (process.Length > 0)
                {
                    process[0].Kill();
                }
            }
            catch { }
        }

        private string GetMD5(string str)
        {
            string pwd = string.Empty;
            MD5 md5 = MD5.Create();//实例化一个md5对像
            // 加密后是一个字节类型的数组，这里要注意编码UTF8/Unicode等的选择　
            byte[] s = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(str));
            // 通过使用循环，将字节类型的数组转换为字符串，此字符串是常规字符格式化所得
            for (int i = 0; i < s.Length; i++)
            {
                // 将得到的字符串使用十六进制类型格式。格式后的字符是小写的字母，如果使用大写（X）则格式后的字符是大写字符
                pwd = pwd + s[i].ToString("x").PadLeft(2, '0');
            }
            return pwd;
        }

        public LoginViewModel(PasswordBox password, Action closeWindow)
        {
            _closeWindow = closeWindow;
            LoginModel loginModel = ConfigController.GetLoginModelConfig().FirstOrDefault();
            string loginName = loginModel == null ? "" : loginModel.LoginName;
            string loginPwd = loginModel == null ? "" : loginModel.LoginPwd;
            bool checkBox = loginPwd != null ? true : false;
            double checkboximage = checkBox == true ? 1 : 0.0;
            model = new LoginModel()
            {
                LoginName = loginName,
                LoginPwd = loginPwd,
                Open = false,
                Checkbox = checkBox,
                LableVisibility = "Hidden",
                //LableVisibility = "Visible",
                Checkboximage = checkboximage,
                UnCheckboximage = 1,
                WaitVisibled = "Hidden"
            };
            password.Password = model.LoginPwd;
        }

    }
}
