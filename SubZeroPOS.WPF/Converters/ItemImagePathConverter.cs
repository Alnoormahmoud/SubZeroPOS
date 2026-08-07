using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SubZeroPOS.WPF.Converters
{
    /// <summary>
    /// Item.ImagePath is stored as a simple relative path, e.g. "Images/Items/7.jpg".
    /// This resolves it against the folder the .exe runs from (SiteOfOrigin),
    /// so item photos can just be copied next to the built app - no database
    /// blobs, no absolute paths that break when the app moves to another PC.
    /// </summary>
    public class ItemImagePathConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                var uri = new Uri($"pack://siteoforigin:,,,/{path}", UriKind.Absolute);
                return new BitmapImage(uri);
            }
            catch
            {
                // Missing/broken image file - fail quietly, placeholder box shows instead
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
