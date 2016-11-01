// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Metadata;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace ArcGISRuntime.UWP.Viewer
{
    public sealed partial class SamplePage 
    {
        public SamplePage()
        {
            InitializeComponent();

            HideStatusBar();

            // Get selected sample and set that as a DataContext
            DataContext = SampleManager.Current.SelectedSample;
            // Set loaded sample to the UI 
            SampleContainer.Content = SampleManager.Current.SampleToControl(SampleManager.Current.SelectedSample);
            LiveSample.IsChecked = true; // Default to the live sample view
        }

        // Check if the phone contract is available (mobile) and hide status bar if it is there
        private async void HideStatusBar()
        {
            // If we have a phone contract, hide the status bar
            if (ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0))
            {
                var statusBar = StatusBar.GetForCurrentView();
                await statusBar.HideAsync();
            }
        }

        private void LiveSample_Checked(object sender, RoutedEventArgs e)
        {
            // Make sure that only one is  selected
            if (Description.IsChecked.HasValue && Description.IsChecked.Value)
                Description.IsChecked = false;

            DescriptionContainer.Visibility = Visibility.Collapsed;
            SampleContainer.Visibility = Visibility.Visible;
        }

        private void Description_Checked(object sender, RoutedEventArgs e)
        {
            // Make sure that only one is  selected
            if (LiveSample.IsChecked.HasValue && LiveSample.IsChecked.Value)
                LiveSample.IsChecked = false;

            DescriptionContainer.Visibility = Visibility.Visible;
            SampleContainer.Visibility = Visibility.Collapsed;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
#if DEBUG
            Window.Current.CoreWindow.KeyDown += CoreWindow_KeyDown;
#endif
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
#if DEBUG
            Window.Current.CoreWindow.KeyDown -= CoreWindow_KeyDown;
#endif
        }

        private bool isResized = false;
        private double originalHeight;
        private double originalWidth;

        private async void CoreWindow_KeyDown(global::Windows.UI.Core.CoreWindow sender, global::Windows.UI.Core.KeyEventArgs args)
        {
            var ctrl = Window.Current.CoreWindow.GetKeyState(VirtualKey.Control);
            if (ctrl.HasFlag(CoreVirtualKeyStates.Down) && args.VirtualKey == VirtualKey.T)
            {
                if (!isResized)
                {
                    originalHeight = SampleContainer.ActualHeight;
                    originalWidth = SampleContainer.ActualWidth;
                    SampleContainer.Height = 600;
                    SampleContainer.Width = 800;
                    isResized = true;
                    return;
                }

                var layoutRoot = new Grid();
                var mapViewImage = new Image() { VerticalAlignment = VerticalAlignment.Top };
                var uiImage = new Image();

                // Create image from the non-map UI
                var uiLayerImage = await CreateBitmapFromElement(SampleContainer.Content as UIElement);

                // Find mapview from the sample. This expects that we use the same name in all samples
                var mapview = (SampleContainer.Content as UserControl).FindName("MyMapView") as MapView;

                // Retrieve general transform 
                var tranform = mapview.TransformToVisual((SampleContainer.Content as UIElement));
                // Retrieve the point value relative to the child.
                var currentPoint = tranform.TransformPoint(new Point(0, 0));
                // Setup the location where the mapview was in the view to respect the ui layout
                mapViewImage.Margin = new Thickness(currentPoint.X, currentPoint.Y, 0, 0);

                // Create snapshot from MapView
                var exportedWritableBitmap = await Esri.ArcGISRuntime.UI.RuntimeImageExtensions.ToImageSourceAsync(await mapview.ExportImageAsync());

                // Set sources to the images and add them to the layout
                uiImage.Source = uiLayerImage;
                mapViewImage.Source = exportedWritableBitmap;
                layoutRoot.Children.Add(mapViewImage);
                layoutRoot.Children.Add(uiImage);

                // Add layout to the view
                var sample = SampleContainer.Content;
                SampleContainer.Content = layoutRoot;

                // Wait that images are rendered
                await Task.Delay(TimeSpan.FromSeconds(1));

                // Save image to the disk
                var combinedImage = await CreateBitmapFromElement(SampleContainer.Content as UIElement);
                await SaveBitmapToFileAsync(combinedImage, SampleManager.Current.SelectedSample.SampleName);

                // Reset view
                SampleContainer.Content = sample;
                SampleContainer.Height = originalHeight;
                SampleContainer.Width = originalWidth;
                isResized = false;
            }
        }

        private static async Task<WriteableBitmap> CreateBitmapFromElement(UIElement element)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap();
            await bitmap.RenderAsync(element);
            var pixelBuffer = await bitmap.GetPixelsAsync();
            byte[] pixels = pixelBuffer.ToArray();
            var writableBitmap = new WriteableBitmap((int)bitmap.PixelWidth, (int)bitmap.PixelHeight);
            using (Stream stream = writableBitmap.PixelBuffer.AsStream())
                await stream.WriteAsync(pixels, 0, pixels.Length);

            return writableBitmap;
        }


        public static async Task SaveBitmapToFileAsync(WriteableBitmap image, string fileName = "screenshot")
        {
            // This stores image to  C:\Users\{user}\AppData\Local\Packages\b13e56ac-7531-429d-baf2-003653d989c1_cc4tdm0yr4r3t\LocalState\Screenshots 
            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("Screenshots", CreationCollisionOption.OpenIfExists);
            var file = await pictureFolder.CreateFileAsync(fileName + ".png", CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream.AsRandomAccessStream());
                var pixelStream = image.PixelBuffer.AsStream();
                byte[] pixels = new byte[image.PixelBuffer.Length];
                await pixelStream.ReadAsync(pixels, 0, pixels.Length);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Straight, (uint)image.PixelWidth, (uint)image.PixelHeight, 96, 96, pixels);
                await encoder.FlushAsync();
            }
        }
    }
}
