using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace ETextsys.Terminal.Model
{
    public class LoginModel : ViewModelBase
    {
        private string loginName;
        private string loginPwd;
        private bool chechBox;
        private string lableVisibility;
        private string prompt;
        private bool open;
        private string selectval;
        private PasswordBox password;
        private double checkboximage;
        private double unCheckboximage;
        private string waitVisibled;
        private string waitContent;

        public string WaitContent
        {
            get { return waitContent; }
            set { waitContent = value; }
        }
        
        public string WaitVisibled
        {
            get { return waitVisibled; }
            set
            {
                waitVisibled = value;
                RaisePropertyChanged("WaitVisibled");
            }
        }
        
        public double UnCheckboximage
        {
            get { return unCheckboximage; }
            set
            {
                unCheckboximage = value;
                RaisePropertyChanged("UnCheckboximage");
            }
        }
        
        public double Checkboximage
        {
            get { return checkboximage; }
            set
            {
                checkboximage = value;
                RaisePropertyChanged("Checkboximage");
            }
        }
        
        public PasswordBox Password
        {
            get { return password; }
            set
            {
                password = value;
                RaisePropertyChanged("Password");
            }
        }

        public string Selectval
        {
            get { return selectval; }
            set
            {
                selectval = value;
                RaisePropertyChanged("Selectval");
            }
        }
        
        public bool Open
        {
            get { return open; }
            set
            {
                open = value;
                RaisePropertyChanged("Open");
            }
        }

        public string Prompt
        {
            get { return prompt; }
            set
            {
                prompt = value;
                RaisePropertyChanged("Prompt");
            }
        }
        
        public string LableVisibility
        {
            get { return lableVisibility; }
            set
            {
                lableVisibility = value;
                RaisePropertyChanged("LableVisibility");
            }
        }

        public bool Checkbox
        {
            get { return chechBox; }
            set
            {
                chechBox = value;
                RaisePropertyChanged("CheckBox");
            }
        }
        
        public string LoginPwd
        {
            get { return loginPwd; }
            set
            {
                loginPwd = value;
                RaisePropertyChanged("LoginPwd");
            }
        }

        public string LoginName
        {
            get { return loginName; }
            set
            {
                loginName = value;
                RaisePropertyChanged("LoginName");
                if (password != null)
                {
                    password.Password = LoginPwd;
                }
            }
        }

        public string TimeStamp { get; set; }
        public string UUID { get; set; }
        public string TerminalType { get; set; }
    }
}
