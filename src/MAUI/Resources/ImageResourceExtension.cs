﻿using System;
using System.Reflection;
using Xamarin.Forms;

namespace Forms.Resources
{
    [ContentProperty(nameof(Source))]
    public class ImageResourceExtension : Xamarin.Forms.Xaml.IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null)
            {
                return null;
            }

            // Do your translation lookup here, using whatever method you require
            var imageSource = ImageSource.FromResource(Source, typeof(ImageResourceExtension).GetTypeInfo().Assembly);

            return imageSource;
        }
    }
}