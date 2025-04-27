using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SpotControl.Converters
{
    // Converter to convert a string URL to an ImageSource
    public class StringToImageSourceConverter : IValueConverter
    {
        // Convert a string URL to an ImageSource
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Check if the value is a string and not null or empty
            if (value is string url && !string.IsNullOrEmpty(url))
            {
                // Try to create a BitmapImage from the URL
                try
                {
                    // Create a new BitmapImage
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(url, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
                catch
                {
                    // Handle any exceptions that occur during the image loading
                    return null; // If the URL is bad, fallback safely
                }
            }
            // If the value is not a valid URL, return null
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
