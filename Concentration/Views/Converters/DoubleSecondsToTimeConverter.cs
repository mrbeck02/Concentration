using System;
using System.Windows.Data;

namespace Concentration.Views.Converters
{
    public class IntSecondsToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int)
            {
                TimeSpan span = TimeSpan.FromSeconds((int)value);
                return span.ToString(@"mm\:ss");
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
