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
using System.Reflection;

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

                if (SourceFiles.Any())
                {
                    SelectFile(SourceFiles[0].Name);
                }
                SourceCodeView.Navigating += Webview_Navigating;
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
#if ANDROID
            var fileNames = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(name => name.Contains(sampleInfo.FormalName));
#else
            var fileNames = Directory.GetFiles(sampleInfo.Path);
#endif
            // Add every .cs and .xaml file in the directory of the sample.
            foreach (string filepath in fileNames.Where(candidate => candidate.EndsWith(".cs") || candidate.EndsWith(".xaml")))
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
        }

        private string GetDescriptionHtml(SampleInfo sampleInfo)
        {
            string folderPath = sampleInfo.Path;
            string readmePath = Path.Combine(folderPath, "readme.md");
            string screenshotPath = Path.Combine(sampleInfo.Path, $"{sampleInfo.FormalName}.jpg");

            // Handle AR edge cases
            folderPath = folderPath.Replace("RoutePlanner", "NavigateAR").Replace("PipePlacer", "ViewHiddenInfrastructureAR");

#if ANDROID
            string readmeContent = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"ArcGISRuntimeMaui.Samples.{sampleInfo.Category}.{sampleInfo.FormalName}.readme.md")).ReadToEnd();
#else
            string readmeContent = File.ReadAllText(readmePath);
#endif

            readmeContent = Markdig.Markdown.ToHtml(readmeContent);

            // Set CSS for dark mode or light mode.
            string markdownCssType = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "github-markdown-dark.css" : "github-markdown.css";

#if ANDROID
            string cssContent = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"ArcGISRuntimeMaui.SyntaxHighlighting.{markdownCssType}")).ReadToEnd();
#else
            string cssContent = File.ReadAllText(Path.Combine(sampleInfo.PathStub, "SyntaxHighlighting", markdownCssType));
#endif

#if WINDOWS
            cssContent = $"{cssContent} h1 {{\r\n    display: none;\r\n}}";
#endif

            // Convert the image into a string of bytes to embed into the html.
#if ANDROID
            var sourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream($"ArcGISRuntimeMaui.Samples.{sampleInfo.Category}.{sampleInfo.FormalName}.{sampleInfo.FormalName}.jpg");
            var memoryStream = new MemoryStream();
            sourceStream.CopyTo(memoryStream);
            byte[] image = memoryStream.ToArray();
            memoryStream.Close();
#else
            byte[] image = File.ReadAllBytes(screenshotPath);
#endif

            string imgSrc = $"data:image/jpg;base64,{Convert.ToBase64String(image)}";

            // Replace paths for image.
            readmeContent = readmeContent.Replace($"{sampleInfo.FormalName}.jpg", imgSrc);
            return $"<!doctype html><head><style>{cssContent} {"body {padding: 10; }"}</style></head><body class=\"markdown-body\">{readmeContent}</body>";
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

        private void DetailButton_Clicked(object sender, EventArgs e)
        {
            SampleDetailPage.IsVisible = true;
            SampleContentPage.IsVisible = SourceCodePage.IsVisible = false;
        }

        private void SourceButton_Clicked(object sender, EventArgs e)
        {
            SourceCodePage.IsVisible = true;
            SampleDetailPage.IsVisible = SampleContentPage.IsVisible = false;
        }

        private async void FileChange_Clicked(object sender, EventArgs e)
        {
            try
            {
                string[] fileList = SourceFiles.Select(s => s.Name).ToArray();
                string result = await Application.Current.MainPage.DisplayActionSheet("Choose file:", "Cancel", null, fileList);
                if (fileList.Contains(result))
                {
                    SelectFile(result);
                }
            }
            catch
            {
            }
        }

        private void SelectFile(string fileName)
        {
            SourceCodeView.Source = new HtmlWebViewSource()
            {
                Html = SourceFiles.Single(s => s.Name == fileName).HtmlContent,
            };
            CurrentFileLabel.Text = fileName;
        }
    }

    public class SourceCodeFile
    {
        // Start of each html string.
        private const string htmlStart =
            "<html>" +
            "<head>" +
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

        public string Name
        {
            get
            {
#if ANDROID
                var split = _path.Split('.');
                if (_path.Contains(".xaml.cs")) return $"{split[split.Length - 3]}.{split[split.Length - 2]}.{split[split.Length - 1]}";
                return $"{split[split.Length - 2]}.{split[split.Length - 1]}";
#else
                return System.IO.Path.GetFileName(_path);
#endif
            }
        }

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

        // Android constructor.
        public SourceCodeFile(string embeddedResourcePath)
        {
            _path = embeddedResourcePath;
        }

        private void LoadContent()
        {
            try
            {
#if ANDROID
                string baseContent = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream(_path)).ReadToEnd();
#else
                string baseContent = File.ReadAllText(_path);
#endif

                // > and < characters will be incorrectly parsed by the html.
                baseContent = baseContent.Replace("<", "&lt;").Replace(">", "&gt;");

                // Set the type of highlighting for the source file.
                string codeClass = _path.EndsWith(".xaml") ? "xml" : "csharp";

                // Set CSS for dark mode or light mode.
                string markdownCssType = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "highlight-dark.css" : "highlight.css";
#if ANDROID
                string cssContent = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"ArcGISRuntimeMaui.SyntaxHighlighting.{markdownCssType}")).ReadToEnd();
#else
                string cssContent = File.ReadAllText(System.IO.Path.Combine(_resourcePath, "SyntaxHighlighting", markdownCssType));
#endif

                // Set the background color. Color values are taken from corresponding css files.
                string backgroundColor = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "#1e1e1e" : "#fff";
                cssContent = $"{cssContent} body {{ background: {backgroundColor};}}";

                // Read javascript content.
#if ANDROID
                string jsContent = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream($"ArcGISRuntimeMaui.SyntaxHighlighting.highlight.js")).ReadToEnd();
#else
                string jsContent = File.ReadAllText(System.IO.Path.Combine(_resourcePath, "SyntaxHighlighting", "highlight.js"));
#endif

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