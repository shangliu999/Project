using ETexsys.APIRequestModel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ETextsys.Terminal.Utilities
{
    public class BasisSendConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = "";
            if (value == null)
            {
                return result;
            }

            EnumBasisSend bs = (EnumBasisSend)value;
            if (bs == EnumBasisSend.OnRecieve)
            {
                result = "收货";
            }
            else if (bs == EnumBasisSend.OnOrder)
            {
                result = "订单";
            }
            else if (bs == EnumBasisSend.OnRecieveAndOrder)
            {
                result = "收货+订单";
            }
            else
            {
                result = "未知";
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
