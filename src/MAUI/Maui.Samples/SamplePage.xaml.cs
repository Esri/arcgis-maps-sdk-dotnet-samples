﻿// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace ArcGISRuntimeMaui
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SamplePage
    {
        private ContentPage _sample;
        private Assembly _assembly;

        public ObservableCollection<SourceCodeFile> SourceFiles { get; } = new ObservableCollection<SourceCodeFile>();

        public SamplePage()
        {
            InitializeComponent();
        }

        public SamplePage(ContentPage sample, SampleInfo sampleInfo) : this()
        {
            this.NavigatedFrom += NavigatedFromEvent;

#if IOS || MACCATALYST
            // iOS / MacCatalyst lifecycle works differently, so we need to use the main page changing instead of the NavigatedFrom event for this.
            Application.Current.MainPage.PropertyChanged += MainPagePropertyChanged;
#endif

            // Set the sample variable.
            _sample = sample;

            // Set the executing assembly (for accessing embedded resources).
            _assembly = Assembly.GetExecutingAssembly();

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
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            try
            {
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
        }

        private void MainPagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "CurrentPage")
            {
                TryDispose();
            }
        }

        private void NavigatedFromEvent(object sender, NavigatedFromEventArgs e)
        {
            TryDispose();
        }

        private void TryDispose()
        {
            // Check that the navigation is backward from the sample and not forward into another page in the sample.
            if (!Application.Current.MainPage.Navigation.NavigationStack.OfType<SamplePage>().Any())
            {
                if (_sample is IDisposable disposableSample) disposableSample.Dispose();
                if (_sample is IARSample ARSample) ARSample.StopAugmentedReality();
            }
        }

        private string GetDescriptionHtml(SampleInfo sampleInfo)
        {
            string folderPath = sampleInfo.Path;
            string readmePath = Path.Combine(folderPath, "readme.md");
            string screenshotPath = Path.Combine(sampleInfo.Path, $"{sampleInfo.FormalName}.jpg");

            // Handle AR edge cases
            folderPath = folderPath.Replace("RoutePlanner", "NavigateAR").Replace("PipePlacer", "ViewHiddenInfrastructureAR");

            string readmeContent = new StreamReader(_assembly.GetManifestResourceStream($"ArcGISRuntimeMaui.Samples.{sampleInfo.Category}.{sampleInfo.FormalName}.readme.md")).ReadToEnd();

            readmeContent = Markdig.Markdown.ToHtml(readmeContent);

            // Set CSS for dark mode or light mode.
            string markdownCssType = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "github-markdown-dark.css" : "github-markdown.css";
            string cssContent = new StreamReader(_assembly.GetManifestResourceStream($"ArcGISRuntimeMaui.SyntaxHighlighting.{markdownCssType}")).ReadToEnd();

#if WINDOWS
            // Remove the readme header on Windows so it doesn't repeat the title.
            cssContent = $"{cssContent} h1 {{\r\n    display: none;\r\n}}";
#endif

            // Convert the image into a string of bytes to embed into the html.
            var sourceStream = _assembly.GetManifestResourceStream($"ArcGISRuntimeMaui.Samples.{sampleInfo.Category}.{sampleInfo.FormalName}.{sampleInfo.FormalName}.jpg");
            var memoryStream = new MemoryStream();
            sourceStream.CopyTo(memoryStream);
            byte[] image = memoryStream.ToArray();
            memoryStream.Close();

            string imgSrc = $"data:image/jpg;base64,{Convert.ToBase64String(image)}";

            // Replace paths for image.
            readmeContent = readmeContent.Replace($"{sampleInfo.FormalName}.jpg", imgSrc);

            // Build the html.
            var fullContent =
                "<!doctype html><head><style>" +
                cssContent +
                "body {padding: 10px; }" +
                "</style>" +
#if IOS
                // Need to set the viewport on iOS to scale page correctly.
                "<meta name=\"viewport\" content=\"width=" +
                Application.Current.MainPage.Width +
                ", shrink-to-fit=YES\">" +
#endif
                "</head><body class=\"markdown-body\">" +
                readmeContent +
                "</body>";

            return fullContent;
        }

        private void LoadSourceCode(SampleInfo sampleInfo)
        {
            // Get all files in the samples folder.
            var fileNames = _assembly.GetManifestResourceNames().Where(name => name.Contains(sampleInfo.FormalName));

            // Add every .cs and .xaml file in the directory of the sample.
            foreach (string filepath in fileNames.Where(file => file.EndsWith(".cs") || file.EndsWith(".xaml")).OrderByDescending(x => x))
            {
                SourceFiles.Add(new SourceCodeFile(filepath));
            }

            // Add additional class files from the sample.
            if (sampleInfo.ClassFiles != null)
            {
                foreach (string additionalPath in sampleInfo.ClassFiles)
                {
                    // Don't add source files already found in the directory.
                    if (!SourceFiles.Any(f => f.Name == additionalPath))
                    {
                        var embeddedResourcePath = additionalPath.Replace('\\', '.');
                        var embeddedName = _assembly.GetManifestResourceNames().Single(name => name.Contains(embeddedResourcePath));

                        // Add class files to the front of the list, they are usually critical to the sample.
                        SourceFiles.Insert(0, new SourceCodeFile(embeddedName));
                    }
                }
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
            // No need to handle any cancellation.
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
        private string _fullContent;

        public string Path => _path;

        public string Name
        {
            get
            {
                var split = _path.Split('.');
                if (_path.Contains(".xaml.cs")) return $"{split[split.Length - 3]}.{split[split.Length - 2]}.{split[split.Length - 1]}";
                return $"{split[split.Length - 2]}.{split[split.Length - 1]}";
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

        public SourceCodeFile(string embeddedResourceFilePath)
        {
            _path = embeddedResourceFilePath;
        }

        private void LoadContent()
        {
            try
            {
                var assembly = Assembly.GetExecutingAssembly();
                string baseContent = new StreamReader(assembly.GetManifestResourceStream(_path)).ReadToEnd();

                // > and < characters will be incorrectly parsed by the html.
                baseContent = baseContent.Replace("<", "&lt;").Replace(">", "&gt;");

                // Set the type of highlighting for the source file.
                string codeClass = _path.EndsWith(".xaml") ? "xml" : "csharp";

                // Set CSS for dark mode or light mode.
                string markdownCssType = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "highlight-dark.css" : "highlight.css";
                string cssContent = new StreamReader(assembly.GetManifestResourceStream($"ArcGISRuntimeMaui.SyntaxHighlighting.{markdownCssType}")).ReadToEnd();

                // Set the background color. Color values are taken from corresponding css files.
                string backgroundColor = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "#1e1e1e" : "#fff";
                cssContent = $"{cssContent} body {{ background: {backgroundColor};}}";

                // Read javascript content.
                string jsContent = new StreamReader(assembly.GetManifestResourceStream($"ArcGISRuntimeMaui.SyntaxHighlighting.highlight.js")).ReadToEnd();

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