// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Models;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace ArcGISRuntimeMaui
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SamplePage
    {
        private ContentPage _sample;

        public ObservableCollection<SourceCodeFile> SourceFiles { get; } = new ObservableCollection<SourceCodeFile>();

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

                LoadSourceCode(sampleInfo);

                SourceCodeView.Source = new HtmlWebViewSource()
                {
                    Html = SourceFiles[0].HtmlContent,
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            Application.Current.RequestedThemeChanged += (s, a) =>
            {
                // Respond to the theme change
                var htmlString = GetDescriptionHtml(sampleInfo);
                DescriptionView.Source = new HtmlWebViewSource()
                {
                    Html = htmlString
                };
            };
        }

        private void LoadSourceCode(SampleInfo sampleInfo)
        {
            //SourceFiles.Clear();

            string folderPath = sampleInfo.Path;

            // Add every .cs and .xaml file in the directory of the sample.
            foreach (string filepath in Directory.GetFiles(folderPath)
                .Where(candidate => candidate.EndsWith(".cs") || candidate.EndsWith(".xaml")))
            {
                SourceFiles.Insert(0, new SourceCodeFile(filepath, sampleInfo.PathStub));
            }

            // Add additional class files from the sample.
            if (sampleInfo.ClassFiles != null)
            {
                foreach (string additionalPath in sampleInfo.ClassFiles)
                {
                    SourceFiles.Insert(0, new SourceCodeFile(additionalPath, sampleInfo.PathStub));
                }
            }

            //SelectedSourceFile = SourceFiles[0];
        }

        private string GetDescriptionHtml(SampleInfo sampleInfo)
        {
            string folderPath = sampleInfo.Path;
            string readmePath = Path.Combine(folderPath, "readme.md");
            string screenshotPath = Path.Combine(sampleInfo.Path, $"{sampleInfo.FormalName}.jpg");

            // Handle AR edge cases
            folderPath = folderPath.Replace("RoutePlanner", "NavigateAR").Replace("PipePlacer", "ViewHiddenInfrastructureAR");

#if ANDROID

            //readmePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), readmePath);
            //syntaxPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), syntaxPath);
            //screenshotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), screenshotPath);
#endif

            string readmeContent = File.ReadAllText(readmePath);
            readmeContent = Markdig.Markdown.ToHtml(readmeContent);

            // Set CSS for dark mode or light mode.
            string markdownCssType = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "github-markdown-dark.css" : "github-markdown.css";
            string cssContent = File.ReadAllText(Path.Combine(sampleInfo.PathStub, "SyntaxHighlighting", markdownCssType));

#if WINDOWS
            cssContent = $"{cssContent} h1 {{\r\n    display: none;\r\n}}";
#endif

            // Convert the image into a string of bytes to embed into the html.
            byte[] image = File.ReadAllBytes(screenshotPath);
            string imgSrc = $"data:image/jpg;base64,{Convert.ToBase64String(image)}";

            // Replace paths for image.
            readmeContent = readmeContent.Replace($"{sampleInfo.FormalName}.jpg", imgSrc);

            return $"<!doctype html><head><style>{cssContent}</style></head><body class=\"markdown-body\">{readmeContent}</body>";
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

    public class SourceCodeFile
    {
        // Start of each html string.
        private const string htmlStart =
            "<html>" +
            "<head>" +
            "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=11\">" +
            "<style>{csscontent}</style>" +
            "<script type=\"text/javascript\">{jscontent} hljs.initHighlightingOnLoad();</script>" +
            "</head>" +
            "<body>" +
            "<pre>";

        private const string htmlEnd =
            "</pre>" +
            "</body>" +
            "</html>";

        private string _path;
        private string _resourcePath;
        private string _fullContent;

        public string Path => _path;

        public string Name => System.IO.Path.GetFileName(_path);

        public string HtmlContent
        {
            get
            {
                if (_fullContent == null)
                {
                    LoadContent();
                }

                return _fullContent;
            }
        }

        public SourceCodeFile(string sourceFilePath, string resourcePath)
        {
            _path = sourceFilePath;
            _resourcePath = resourcePath;
        }

        private void LoadContent()
        {
            try
            {
                string baseContent = File.ReadAllText(_path);

                // Set the type of highlighting for the source file.
                string codeClass = _path.EndsWith(".xaml") ? "xml" : "csharp";

                // Set CSS for dark mode or light mode.
                string markdownCssType = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "highlight-dark.css" : "highlight.css";
                string cssContent = File.ReadAllText(System.IO.Path.Combine(_resourcePath, "SyntaxHighlighting", markdownCssType));

                // Set the background color. Color values are taken from corresponding css files.
                string backgroundColor = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "#1e1e1e" : "#fff";
                cssContent = $"{cssContent} body {{ background: {backgroundColor};}}";

                // Read javascript content.
                string jsContent = File.ReadAllText(System.IO.Path.Combine(_resourcePath, "SyntaxHighlighting", "highlight.pack.js"));

                // > and < characters will be incorrectly parsed by the html.
                baseContent = baseContent.Replace("<", "&lt;").Replace(">", "&gt;");

                // Build the html.
                _fullContent =
                    htmlStart.Replace("{csscontent}", cssContent).Replace("{jscontent}", jsContent) +
                    $"<code class=\"{codeClass}\">" +
                    baseContent +
                    "</code>" +
                    htmlEnd;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}