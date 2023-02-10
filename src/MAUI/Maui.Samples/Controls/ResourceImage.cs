#if WINDOWS || MACCATALYST
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGIS.Controls
{
    public class ResourceImage : Image
    {
        public static readonly BindableProperty ResourceProperty = BindableProperty.Create(nameof(Resource), typeof(string), typeof(string), null, BindingMode.OneWay, null, ResourceChanged);

        private static void ResourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var resourceString = (string)newValue;
            var imageControl = (Image)bindable;

            imageControl.Source = ImageSource.FromFile(resourceString);
        }

        public string Resource
        {
            get => (string)GetValue(ResourceProperty);
            set => SetValue(ResourceProperty, value);
        }
    }
}
#endif