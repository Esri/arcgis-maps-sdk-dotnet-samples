// Copyright 2018 Esri.
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
using Windows.Foundation.Metadata;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
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

            // Get selected sample and set that as the DataContext.
            DataContext = SampleManager.Current.SelectedSample;

            // Load and show the sample.
            SampleContainer.Content = SampleManager.Current.SampleToControl(SampleManager.Current.SelectedSample);

            if (App.Current.RequestedTheme == Windows.UI.Xaml.ApplicationTheme.Dark)
            {
                // Do dark stuff
            }

            // Set file path for the readme.
            string readmePath = System.IO.Path.Combine(SampleManager.Current.SelectedSample.Path, "Readme.md");

            //DescriptionView.NavigateToString(htmlString);
            string markdowntext = System.IO.File.ReadAllText(readmePath);

            // Take off first line (the title header)
            markdowntext = markdowntext.Substring(markdowntext.IndexOf('\n') + 1);

            // Fix image links from the old readme format.
            markdowntext = markdowntext.Replace("<img src=\"", "![](").Replace("\" width=\"350\"/>", ")");

            // Set readme in the mark down block.
            MarkDownBlock.Text = markdowntext;

            MarkDownBlock.Background = new SolidColorBrush() { Opacity = 0 };

            SourceCodeContainer.LoadSourceCode();
        }

        private static async void HideStatusBar()
        {
            // Check if the phone contract is available (mobile) and hide status bar if it is there.
            if (!ApiInformation.IsApiContractPresent("Windows.Phone.PhoneContract", 1, 0)) return;
            await StatusBar.GetForCurrentView().HideAsync();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Prevent user from going back
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        }

        private void TabChanged(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
        {
            switch (((TabViewItem)Tabs.SelectedItem).Header.ToString())
            {
                case "Live Sample":
                    SampleContainer.Visibility = Visibility.Visible;
                    DescriptionContainer.Visibility = Visibility.Collapsed;
                    SourceCodeContainer.Visibility = Visibility.Collapsed;
                    break;

                case "Description":
                    SampleContainer.Visibility = Visibility.Collapsed;
                    DescriptionContainer.Visibility = Visibility.Visible;
                    SourceCodeContainer.Visibility = Visibility.Collapsed;
                    break;

                case "Source Code":
                    SampleContainer.Visibility = Visibility.Collapsed;
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
    }
}