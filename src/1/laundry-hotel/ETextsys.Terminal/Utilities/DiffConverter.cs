using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ETextsys.Terminal.Utilities
{
    public class DiffConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int reValue = System.Convert.ToInt32(value);

            SolidColorBrush scb = new SolidColorBrush();
            if (reValue > 0)
            {
                scb.Color = Colors.Red;
            }
            else if (reValue == 0)
            {
                scb.Color = Colors.Green;
            }
            else
            {
                scb.Color = Colors.Transparent;
            }

            return scb;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value;
        }
    }
}
