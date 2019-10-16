using CopyTranslation.ViewModels;
using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace CopyTranslation.Views.Converters
{
    public class MainPageStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var color = (value ?? MainPageStatus.Normal) switch
            {
                MainPageStatus.Normal => HexToColor("#5ed279"),
                MainPageStatus.Busy => HexToColor("#e7ca51"),
                MainPageStatus.Failed => HexToColor("#e74242"),
                MainPageStatus.Pause => Colors.Gray,
                _ => Colors.Magenta,
            };

            return new SolidColorBrush(color);
        }

        private Color HexToColor(string str)
        {
            var colorStr = str.ToLower();

            colorStr = colorStr.Replace("#", string.Empty);

            var r = (byte)System.Convert.ToUInt32(colorStr.Substring(0, 2), 16);
            var g = (byte)System.Convert.ToUInt32(colorStr.Substring(2, 2), 16);
            var b = (byte)System.Convert.ToUInt32(colorStr.Substring(4, 2), 16);

            return Color.FromArgb(255, r, g, b);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
