// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using System;
using Windows.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using ArcGISRuntime.WinUI.Viewer;
using Microsoft.UI.Xaml.Controls;

namespace ArcGISRuntime.WinUI.Viewer
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

            // Change UI elements to be dark.
            if (App.Current.RequestedTheme == ApplicationTheme.Dark)
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
            ContentArea.RequestedTheme = SampleContainer.RequestedTheme;
            ContentArea.Background = Tabs.Background;
            DescriptionContainer.Background = (Brush)Application.Current.Resources["ApplicationPageBackgroundThemeBrush"];

            // Load the source code files.
            SourceCodeContainer.LoadSourceCode();
        }

        private void TabChanged(object sender, Microsoft.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            switch (((TabViewItem)Tabs.SelectedItem).Header.ToString())
            {
                case "Live Sample":
                    SampleGrid.Visibility = Visibility.Visible;
                    DescriptionContainer.Visibility = Visibility.Collapsed;
                    SourceCodeContainer.Visibility = Visibility.Collapsed;
                    break;

                case "Description":
                    SampleGrid.Visibility = Visibility.Collapsed;
                    DescriptionContainer.Visibility = Visibility.Visible;
                    SourceCodeContainer.Visibility = Visibility.Collapsed;
                    break;

                case "Source Code":
                    SampleGrid.Visibility = Visibility.Collapsed;
                    DescriptionContainer.Visibility = Visibility.Collapsed;
                    SourceCodeContainer.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void MarkDownBlock_ImageResolving(object sender, ImageResolvingEventArgs e)
        {
            e.Image = new BitmapImage(new Uri(System.IO.Path.Combine(SampleManager.Current.SelectedSample.Path, e.Url)));
            e.Handled = true;
        }

        private void SamplePage_Loaded(object sender, RoutedEventArgs e)
        {
            Tabs.SelectionChanged += TabChanged;
            DescriptionBlock.ImageResolving += MarkDownBlock_ImageResolving;
        }

        private void SamplePage_Unloaded(object sender, RoutedEventArgs e)
        {
            Tabs.SelectionChanged -= TabChanged;
            DescriptionBlock.ImageResolving -= MarkDownBlock_ImageResolving;
        }

        private async void MarkdownText_LinkClicked(object sender, LinkClickedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri(e.Link));
        }
    }
}