using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace SubZeroPOS.WPF.Converters
{
    /// <summary>
    /// Item.ImagePath is stored as a simple relative path, e.g. "Images/Items/7.jpg".
    /// This resolves it against the folder the .exe runs from (SiteOfOrigin).
    ///
    /// IMPORTANT: DecodePixelWidth forces WPF to decode a small thumbnail instead
    /// of the full-resolution photo. Without this, loading ~30 multi-megabyte
    /// phone photos at once (one per item card) can freeze the UI for many
    /// seconds - this was the cause of the Order Entry screen taking ~30s to load.
    /// </summary>
    public class ItemImagePathConverter : IValueConverter
    {
        private const int ThumbnailWidth = 200; // plenty for a 150px-wide card

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path))
                return null;

            try
            {
                var uri = new Uri($"pack://siteoforigin:,,,/{path}", UriKind.Absolute);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = uri;
                bitmap.DecodePixelWidth = ThumbnailWidth;   // decode small, not full-res
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // decode immediately, then release the file handle
                bitmap.EndInit();
                bitmap.Freeze(); // makes it safe/fast to use across threads and improves performance

                return bitmap;
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
