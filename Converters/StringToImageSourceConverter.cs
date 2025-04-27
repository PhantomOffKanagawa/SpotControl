using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SpotControl.Converters
{
    // Converter to convert a string URL to an ImageSource
    public class StringToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Ensure value is set
            if (value is string imageUrl && !string.IsNullOrWhiteSpace(imageUrl))
            {
                try
                {
                    // Return image
                    return new BitmapImage(new Uri(imageUrl, UriKind.Absolute));
                }
                catch
                {
                    // Handle invalid URL or other issues gracefully  
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
