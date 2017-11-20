﻿using Etextsys.Terminal.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETextsys.Terminal.Model.Choose
{
    public class DetailsModel : ViewModelBase
    {
        private string waitContent;
        public string WaitContent
        {
            get { return waitContent; }
            set
            {
                waitContent = value;
                RaisePropertyChanged("WaitContent");
            }
        }


        private string loadingWait;
        public string LoadingWait
        {
            get { return loadingWait; }
            set
            {
                loadingWait = value;
                RaisePropertyChanged("LoadingWait");
            }
        }


        private string printTime;
        public string PrintTime
        {
            get { return printTime; }
            set { printTime = value; }
        }

        private string regionName;
        public string RegionName
        {
            get { return regionName; }
            set { regionName = value; }
        }

        private string handlerName;
        public string HandlerName
        {
            get { return handlerName; }
            set { handlerName = value; }
        }

        private string documentNumberparam;
        public string DocumentNumberparam
        {
            get { return documentNumberparam; }
            set
            {
                documentNumberparam = value;
                RaisePropertyChanged("DocumentNumberparam");
            }
        }

        private string orderNumber;
        public string OrderNumber
        {
            get { return orderNumber; }
            set
            {
                orderNumber = value;
                RaisePropertyChanged("OrderNumber");
            }
        }

        private string createTime;
        public string CreateTime
        {
            get { return createTime; }
            set
            {
                createTime = value;
                RaisePropertyChanged("CreateTime");
            }
        }

        private string createUser;
        public string CreateUser
        {
            get { return createUser; }
            set
            {
                createUser = value;
                RaisePropertyChanged("CreateUser");
            }
        }

        private string hotel;
        public string Hotel
        {
            get { return hotel; }
            set
            {
                hotel = value;
                RaisePropertyChanged("Hotel");
            }
        }

        private string region;
        public string Region
        {
            get { return region; }
            set
            {
                region = value;
                RaisePropertyChanged("Region");
            }
        }

        private string _bags;
        public string Bags
        {
            get { return _bags; }
            set
            {
                _bags = value;
                this.RaisePropertyChanged("Bags");
            }
        }

        private ObservableCollection<ReturnTable> returnTable;
        public ObservableCollection<ReturnTable> ReturnTable
        {
            get { return returnTable; }
            set
            {
                returnTable = value;
                RaisePropertyChanged("ReturnTable");
            }
        }

    }
    public class ReturnTable : ViewModelBase
    {
        private string type;
        public string Type
        {
            get { return type; }
            set { type = value; }
        }

        private string size;
        public string Size
        {
            get { return size; }
            set { size = value; }
        }

        private int number;
        public int Number
        {
            get { return number; }
            set { number = value; }
        }


    }
}
