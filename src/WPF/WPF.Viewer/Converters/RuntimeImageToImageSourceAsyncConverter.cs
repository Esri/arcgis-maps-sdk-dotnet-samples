using Esri.ArcGISRuntime.UI;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace ArcGIS.Converters
{
    public class RuntimeImageToImageSourceAsyncConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is RuntimeImage runtimeImage)
            {
                return new RuntimeImageToImageSourceTask(runtimeImage);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class RuntimeImageToImageSourceTask : INotifyPropertyChanged
    {
        private ImageSource _imageSource;

        public RuntimeImageToImageSourceTask(RuntimeImage runtimeImage)
        {
            _imageSource = null;
            _ = SetImageSourceAsync(runtimeImage);
        }

        private async Task SetImageSourceAsync(RuntimeImage runtimeImage)
        {
            ImageSource = await runtimeImage.ToImageSourceAsync();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ImageSource ImageSource
        {
            get => _imageSource;
            private set
            {
                _imageSource = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ImageSource)));
            }
        }
    }
}
