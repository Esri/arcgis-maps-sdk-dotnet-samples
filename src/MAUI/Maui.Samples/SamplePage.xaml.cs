// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Models;
using System.Diagnostics;

namespace ArcGISRuntimeMaui
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SamplePage
    {
        private ContentPage _sample;

        public SamplePage()
        {
            InitializeComponent();
        }

        public SamplePage(ContentPage sample, SampleInfo sampleInfo) : this()
        {
            this.NavigatedFrom += NavigatedFromEvent;

            // Set the sample variable.
            _sample = sample;

            // Update the binding context - this is important for the description tab.
            BindingContext = sampleInfo;

            // Update the content - this displays the sample.
            SampleContentPage.Content = sample.Content;

            //  Start AR
            if (_sample is IARSample ARSample) ARSample.StartAugmentedReality();

            // Set the title.
            Title = sampleInfo.SampleName;

            // Set up the description page.
            try
            {
                var htmlString = GetDescriptionHtml(sampleInfo);
                DescriptionView.Source = new HtmlWebViewSource()
                {
                    Html = htmlString
                };
                DescriptionView.Navigating += Webview_Navigating;

                SourceCodeView.Source = new HtmlWebViewSource()
                {
                    Html = "Placeholder for source code.",
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private string GetDescriptionHtml(SampleInfo sampleInfo)
        {
            string folderPath = sampleInfo.Path;
            string baseUrl = "";
            string readmePath = "";
            string basePath = "";

            string syntaxPath = folderPath.Substring(0, sampleInfo.Path.LastIndexOf("Samples"));

            // Handle AR edge cases
            folderPath = folderPath.Replace("RoutePlanner", "NavigateAR").Replace("PipePlacer", "ViewHiddenInfrastructureAR");

#if WINDOWS
            baseUrl = "ms-appx-web:///";
            basePath = $"{baseUrl}{folderPath.Substring(folderPath.LastIndexOf("Samples"))}";
            readmePath = System.IO.Path.Combine(folderPath, "readme.md");

#elif ANDROID
            baseUrl = "file:///android_asset";
            basePath = System.IO.Path.Combine(baseUrl, folderPath);
            readmePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), folderPath, "readme.md");
#elif IOS
                baseUrl = Foundation.NSBundle.MainBundle.BundlePath;
                basePath = folderPath;
                readmePath = System.IO.Path.Combine(folderPath, "readme.md");
#endif

            string readmeContent = System.IO.File.ReadAllText(readmePath);
            readmeContent = Markdig.Markdown.ToHtml(readmeContent);

            string markdownCssType = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "github-markdown-dark.css" : "github-markdown.css";

            string cssContent = System.IO.File.ReadAllText(System.IO.Path.Combine(syntaxPath, "SyntaxHighlighting", markdownCssType));

            // <img src="data:image/jpg;base64, (byte array)">
            byte[] image = System.IO.File.ReadAllBytes(Path.Combine(sampleInfo.Path, $"{sampleInfo.FormalName}.jpg"));

            var base64 = Convert.ToBase64String(image);
            var imgSrc = $"data:image/jpg;base64,{base64}";

            // Fix paths for images.
            readmeContent = readmeContent.Replace($"{sampleInfo.FormalName}.jpg", imgSrc);

            //string htmlString = $"<!doctype html><head><link rel=\"stylesheet\" href=\"{cssPath}\" /></head><body class=\"markdown-body\">{readmeContent}</body>";
            string htmlString = $"<!doctype html><head><style>{cssContent}</style></head><body class=\"markdown-body\">{readmeContent}</body>";
            return htmlString;
        }

        private void NavigatedFromEvent(object sender, NavigatedFromEventArgs e)
        {
            // Check that the navigation is backward from the sample and not forward into another page in the sample.
            if (!Application.Current.MainPage.Navigation.NavigationStack.OfType<SamplePage>().Any())
            {
                if (_sample is IDisposable disposableSample) disposableSample.Dispose();
                if (_sample is IARSample ARSample) ARSample.StopAugmentedReality();
            }
        }

        private void Webview_Navigating(object sender, WebNavigatingEventArgs e)
        {
            // Open links in a new window instead of inside the web view.
            if (e.Url.StartsWith("http"))
            {
                try
                {
                    Microsoft.Maui.ApplicationModel.Launcher.OpenAsync(new Uri(e.Url));
                    e.Cancel = true;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }
        }

        private void SampleButton_Clicked(object sender, EventArgs e)
        {
            SampleContentPage.IsVisible = true;
            SampleDetailPage.IsVisible = SourceCodePage.IsVisible = false;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            SampleDetailPage.IsVisible = true;
            SampleContentPage.IsVisible = SourceCodePage.IsVisible = false;
        }

        private void Button_Clicked_1(object sender, EventArgs e)
        {
            SourceCodePage.IsVisible = true;
            SampleDetailPage.IsVisible = SampleContentPage.IsVisible = false;
        }
    }
}