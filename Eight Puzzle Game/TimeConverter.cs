using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Eight_Puzzle_Game
{
    class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result;
            int minute = ((int)value / 60);
            int second = ((int)value - minute * 60);
            if (second < 10)
            {
                result = $"0{minute.ToString()}:0{second.ToString()}";
            }
            else
            {
                result = $"0{minute.ToString()}:{second.ToString()}";
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
