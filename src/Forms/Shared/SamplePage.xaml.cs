﻿// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Diagnostics;
using ArcGISRuntime.Samples.Shared.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ArcGISRuntime
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SamplePage
    {
        private readonly MarkedNet.Marked _markdownRenderer = new MarkedNet.Marked();
        private ContentPage _sample;

        public SamplePage()
        {
            InitializeComponent();
            ToolbarItems[0].Clicked += (o, e) => { SampleDetailPage.IsVisible = !SampleDetailPage.IsVisible; };
        }

        public SamplePage(ContentPage sample, SampleInfo sampleInfo) : this()
        {
            // Set the sample variable.
            _sample = sample;

            // Update the binding context - this is important for the description tab.
            BindingContext = sampleInfo;

            // Update the content - this displays the sample.
            SampleContentPage.Content = sample.Content;

            // Because the sample control isn't navigated to (its content is displayed directly),
            //    navigation won't work from within the sample until the parent is manually set.
            sample.Parent = this;

            // Set the title. If the sample control didn't 
            // define the title, use the name from the sample metadata.
            if (!String.IsNullOrWhiteSpace(sample.Title))
            {
                Title = sample.Title;
            }
            else
            {
                Title = sampleInfo.SampleName;
            }

            // Set up the description page.
            try
            {
                string folderPath = sampleInfo.Path;
                string baseUrl = "";
                string readmePath = "";
                string basePath = "";
#if WINDOWS_UWP
                baseUrl = "ms-appx-web:///";
                basePath = $"{baseUrl}{folderPath.Substring(folderPath.LastIndexOf("Samples"))}";
                readmePath = System.IO.Path.Combine(folderPath, "readme.md");
#elif XAMARIN_ANDROID
                baseUrl = "file:///android_asset";
                basePath = baseUrl + folderPath;
                readmePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + folderPath, "readme.md");
#elif __IOS__
                baseUrl = Foundation.NSBundle.MainBundle.BundlePath;
                basePath = folderPath;
                readmePath = System.IO.Path.Combine(folderPath, "readme.md");
#endif
                string cssPath = $"{baseUrl}/github-markdown.css";

                string readmeContent = System.IO.File.ReadAllText(readmePath);
                readmeContent = _markdownRenderer.Parse(readmeContent);

                // Fix paths for images.
                readmeContent = readmeContent.Replace("src='", "src=\"").Replace(".jpg'", ".jpg\"").Replace("src=\"", $"src=\"{basePath}/");

                string htmlString = $"<!doctype html><head><link rel=\"stylesheet\" href=\"{cssPath}\" /></head><body class=\"markdown-body\">{readmeContent}</body>";
                DescriptionView.Source = new HtmlWebViewSource()
                {
                    Html = htmlString,
                    BaseUrl = basePath
                };
                DescriptionView.Navigating += Webview_Navigating;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        protected override void OnDisappearing()
        {
            if (_sample is IDisposable) ((IDisposable)_sample).Dispose();
            base.OnDisappearing();
        }

        private void Webview_Navigating(object sender, WebNavigatingEventArgs e)
        {
            // Open links in a new window instead of inside the web view.
            if (e.Url.StartsWith("http"))
            {
                try
                {
                    Device.OpenUri(new Uri(e.Url));
                    e.Cancel = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }
    }
}