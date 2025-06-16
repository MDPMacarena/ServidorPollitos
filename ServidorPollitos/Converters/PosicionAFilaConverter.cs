using System;
using System.Globalization;
using System.Windows.Data;

namespace ServidorPollitos.Converters
{
    public class PosicionAFilaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            byte posicion = (byte)value;
            return (posicion / 5) * 100; // 100 = alto de cada celda
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
