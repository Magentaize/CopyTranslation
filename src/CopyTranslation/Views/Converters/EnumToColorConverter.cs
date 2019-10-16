using CopyTranslation.ViewModels;
using System;
using Windows.UI;
using Windows.UI.Xaml.Data;

namespace CopyTranslation.Views.Converters
{
    public class MainPageStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (MainPageStatus)value switch
            {
                MainPageStatus.Successul => "#5ed279",
                MainPageStatus.Busy => "#e7ca51",
                MainPageStatus.Failed => "#e74242",
                _ => Colors.Azure,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
