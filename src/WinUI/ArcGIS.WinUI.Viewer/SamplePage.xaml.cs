// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Managers;
using CommunityToolkit.WinUI.UI.Controls;
using Esri.ArcGISRuntime.UI.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using Windows.System;

namespace ArcGIS.WinUI.Viewer
{
    public sealed partial class SamplePage
    {
        public SamplePage()
        {
            InitializeComponent();

            // Add events for cleaning up sample page when closed or opened.
            Unloaded += SamplePage_Unloaded;
            Loaded += SamplePage_Loaded;

            // Get selected sample and set that as the DataContext.
            DataContext = SampleManager.Current.SelectedSample;

            // Load and show the sample.
            SampleContainer.Content = SampleManager.Current.SampleToControl(SampleManager.Current.SelectedSample);

            if (ScreenshotManager.ScreenshotSettings.ScreenshotEnabled)
            {
                KeyDown += ScreenshotKeyDown_Event;
                SampleContainer.Width = ScreenshotManager.ScreenshotSettings.Width.HasValue ? ScreenshotManager.ScreenshotSettings.Width.Value : double.NaN;
                SampleContainer.Height = ScreenshotManager.ScreenshotSettings.Height.HasValue ? ScreenshotManager.ScreenshotSettings.Height.Value : double.NaN;
            }

            // Change UI elements to be dark.
            if (Application.Current.RequestedTheme == ApplicationTheme.Dark)
            {
                DescriptionBlock.RequestedTheme = ElementTheme.Dark;
            }

            // Set file path for the readme.
            string readmePath = System.IO.Path.Combine(SampleManager.Current.SelectedSample.Path, "Readme.md");
            string readmeText = System.IO.File.ReadAllText(readmePath);

            // Take off first line (the title header)
            readmeText = readmeText.Substring(readmeText.IndexOf('\n') + 1);

            // Fix image links from the old readme format.
            readmeText = readmeText.Replace("<img src=\"", "![](").Replace("\" width=\"350\"/>", ")");

            // Set readme in the mark down block.
            DescriptionBlock.Text = readmeText;

            // Remove the background from the mark down renderer.
            DescriptionBlock.Background = new SolidColorBrush() { Opacity = 0 };

            // Set the appropriate backgrounds.
            DescriptionContainer.Background = (Microsoft.UI.Xaml.Media.Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];

            // Load the source code files.
            _ = SourceCodeContainer.LoadSourceCodeAsync();
        }

        private void MarkDownBlock_ImageResolving(object sender, ImageResolvingEventArgs e)
        {
            e.Image = new BitmapImage(new Uri(System.IO.Path.Combine(SampleManager.Current.SelectedSample.Path, e.Url)));
            e.Handled = true;
        }

        private void SamplePage_Loaded(object sender, RoutedEventArgs e)
        {
            DescriptionBlock.ImageResolving += MarkDownBlock_ImageResolving;
        }

        private void SamplePage_Unloaded(object sender, RoutedEventArgs e)
        {
            DescriptionBlock.ImageResolving -= MarkDownBlock_ImageResolving;

            // Explicit cleanup of the Map and SceneView instead of waiting for garbage collector can help when
            // lots of geoviews are being opened and closed
            foreach (var geoView in TreeWalker<GeoView>(SampleContainer.Content as UIElement))
            {
                if (geoView is MapView mapView)
                {
                    mapView.Map = null;
                    if (mapView.LocationDisplay != null) mapView.LocationDisplay.IsEnabled = false;
                }
                else if (geoView is SceneView sceneView) sceneView.Scene = null;
            }
        }

        private static IEnumerable<T> TreeWalker<T>(UIElement root)
        {
            if (root is not null)
            {
                if (root is T t)
                    yield return t;
                else if (root is UIElement it)
                {
                    for (int i = 0; i < VisualTreeHelper.GetChildrenCount(it); i++)
                    {
                        var e = VisualTreeHelper.GetChild(it, 0);
                        foreach (var obj in TreeWalker<T>(e as UIElement))
                            yield return obj;
                    }
                }
            }
        }

        private async void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }

        #region Screenshot Tool

        private void ScreenshotKeyDown_Event(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.End)
            {
                SaveScreenshot(SampleContainer);
            }
        }

        // Code here is adapted from the following Stack Overflow answers:
        // https://stackoverflow.com/q/24466482
        // https://stackoverflow.com/a/15537372
        private void SaveScreenshot(UIElement source)
        {
            double scale = ScreenshotManager.ScreenshotSettings.ScaleFactor.HasValue ? ScreenshotManager.ScreenshotSettings.ScaleFactor.Value : double.NaN;

            int height = (int)(source.DesiredSize.Height * scale);
            int width = (int)(source.DesiredSize.Width * scale);
            var visual = source.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, 0));

            // This is the height of the top bar for the viewer application.
            int topBorderWidth = 29;

            int X = (int)(visual.X * scale);
            int Y = (int)(visual.Y * scale) + topBorderWidth;

            Bitmap screenshot = new Bitmap(width, height);
            Graphics G = Graphics.FromImage(screenshot);
            G.CopyFromScreen(X, Y, 0, 0, new Size(width, height), CopyPixelOperation.SourceCopy);

            // If scaling has occurred due to screen scaling we need to resize the image.
            Bitmap resizedScreenshot = new Bitmap(screenshot, new Size((int)(screenshot.Width / scale), (int)(screenshot.Height / scale)));

            string filePath = $"{ScreenshotManager.ScreenshotSettings.SourcePath}\\WinUI\\ArcGIS.WinUI.Viewer\\Samples\\" +
                $"{SampleManager.Current.SelectedSample.Category}\\" +
                $"{SampleManager.Current.SelectedSample.FormalName}\\" +
                $"{SampleManager.Current.SelectedSample.FormalName}.jpg";

            // Remove white space.
            filePath = Regex.Replace(filePath, @"\s+", "");

            try
            {
                System.IO.FileStream fs = System.IO.File.Open(filePath, System.IO.FileMode.OpenOrCreate);
                resizedScreenshot.Save(fs, System.Drawing.Imaging.ImageFormat.Jpeg);
                fs.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving screenshot: {ex.Message}");
            }
        }

        #endregion Screenshot Tool
    }
}