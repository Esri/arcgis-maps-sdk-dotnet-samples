using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Controls;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;

namespace ArcGIS.UWP.Samples.MobileMapSearchAndRoute
{
    /// <summary>
    /// This attached property for UWP's Image control allows you to show Item thumbnail by binding directly to Item.
    /// The Image is updated when async loading and conversion is done in the background, without having to wrap each Item in a view model.
    /// </summary>
    public static class AsyncImageLoader
    {
        public static readonly DependencyProperty AsyncSourceProperty =
            DependencyProperty.RegisterAttached(
                "AsyncSource",
                typeof(Item),
                typeof(AsyncImageLoader),
                new PropertyMetadata(null, OnAsyncSourceChanged));

        public static Item GetAsyncSource(Image image)
        {
            return (Item)image.GetValue(AsyncSourceProperty);
        }

        public static void SetAsyncSource(Image image, Item value)
        {
            image.SetValue(AsyncSourceProperty, value);
        }

        private static async void OnAsyncSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Image image && e.NewValue is Item item)
            {
                if (item.ThumbnailUri != null)
                {
                    // If the item has a thumbnail URI, let UWP handle the loading
                    image.Source = new BitmapImage(item.ThumbnailUri);
                }
                else if (item.Thumbnail != null)
                {
                    // Else, we need to asynchronously convert RuntimeImage to ImageSource.
                    try
                    {
                        image.Source = await item.Thumbnail.ToImageSourceAsync();
                    }
                    catch (Exception ex)
                    {
                        // Log any exceptions that may occur during loading
                        System.Diagnostics.Debug.WriteLine($"Error loading thumbnail: {ex.Message}");
                    }
                }
            }
        }
    }
}