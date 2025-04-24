using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SpotControl.Converters
{
    // Converter for Toggles to be Noticeable
    public class BoolToBrushConverter : IValueConverter
    {
        // Set Styling Based on True or False Value
        public Brush TrueBrush { get; set; } = Brushes.LightGreen;
        public Brush FalseBrush { get; set; } = Brushes.Transparent;

        // Convert From Value to Styling
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool b)
            {
                return b ? TrueBrush : FalseBrush;
            }

            return FalseBrush;
        }

        // Fullfill Abstract
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException(); // Not needed for one-way binding
        }
    }
}
