// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

#if WINDOWS
        using uiXaml =  Microsoft.UI.Xaml;
        using System.Drawing;
        using ArcGIS.Samples.Shared.Managers;
        using System.Text.RegularExpressions;
        using Microsoft.Maui.Graphics;
#endif

using ArcGIS.Samples.Managers;
using ArcGIS.Samples.Shared.Models;
using Esri.ArcGISRuntime.Maui;
using Microsoft.Maui.Platform;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace ArcGIS
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SamplePage
    {
        private ContentPage _sample;
        private Assembly _assembly;
        private int _lastViewedFileIndex = 0;
        // single-instance webviews reused on each view, to avoid a memory leak in webview
        static WebView DescriptionView = new WebView();
        static WebView SourceCodeView = new WebView();

        public ObservableCollection<SourceCodeFile> SourceFiles { get; } = new ObservableCollection<SourceCodeFile>();

        public SamplePage()
        {
            InitializeComponent();
        }

        public SamplePage(ContentPage sample, SampleInfo sampleInfo) : this()
        {
            // Set the sample variable.
            _sample = sample;

            // Set the executing assembly (for accessing embedded resources).
            _assembly = Assembly.GetExecutingAssembly();

            // Update the binding context - this is important for the description tab.
            BindingContext = sampleInfo;

            // Update the content - this displays the sample.
            SampleContentPage.Content = sample.Content;

#if WINDOWS
            if (ScreenshotManager.ScreenshotSettings.ScreenshotEnabled)
            {
                var screenshotToolbarItem = new ToolbarItem();
                screenshotToolbarItem.Clicked += ScreenshotButton_Clicked;
                screenshotToolbarItem.IconImageSource="camera.png";
                screenshotToolbarItem.Text = "Screenshot";
                ToolbarItems.Insert(0, screenshotToolbarItem);

                SampleContentPage.WidthRequest = ScreenshotManager.ScreenshotSettings.Width.HasValue ? ScreenshotManager.ScreenshotSettings.Width.Value : double.NaN;
                SampleContentPage.HeightRequest = ScreenshotManager.ScreenshotSettings.Height.HasValue ? ScreenshotManager.ScreenshotSettings.Height.Value : double.NaN;
            }
#endif

            //  Start AR
            if (_sample is IARSample ARSample) ARSample.StartAugmentedReality();

            // Set the title.
            Title = sampleInfo.SampleName;

            LoadSampleData(sampleInfo);
        }

        protected override void OnNavigatedTo(NavigatedToEventArgs args)
        {
            base.OnNavigatedTo(args);
            SampleDetailPage.Content = DescriptionView;
            SourceCodeViewContainer.Content = SourceCodeView;
            DescriptionView.Navigating += Webview_Navigating;
            SourceCodeView.Navigating += Webview_Navigating;
        }

        protected override void OnNavigatingFrom(NavigatingFromEventArgs args)
        {
            SampleDetailPage.Content = null;
            SourceCodeViewContainer.Content = null;
            DescriptionView.Navigating -= Webview_Navigating;
            SourceCodeView.Navigating -= Webview_Navigating;
            base.OnNavigatingFrom(args);
        }

        protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
        {
            base.OnNavigatedFrom(args);

            // Check that the navigation is backward from the sample and not forward into another page in the sample.
            if (!Application.Current.MainPage.Navigation.NavigationStack.OfType<SamplePage>().Any())
            {
                // Explicit cleanup of the Map and SceneView instead of waiting for garbage collector can help when
                // lots of geoviews are being opened and closed
                foreach (var geoView in TreeWalker<GeoView>(_sample))
                {
                    if (geoView is MapView mapView)
                    {
                        mapView.Map = null;
                        if (mapView.LocationDisplay != null) mapView.LocationDisplay.IsEnabled = false;
                    }
                    else if (geoView is SceneView sceneView) sceneView.Scene = null;

                    geoView.Handler?.DisconnectHandler();
                }

                if (_sample is IDisposable disposableSample) disposableSample.Dispose();
                if (_sample is IARSample ARSample) ARSample.StopAugmentedReality();
            }
        }

        private async void LoadSampleData(SampleInfo sampleInfo)
        { 
            // Set up the description page.
            try
            {
                var htmlString = await GetDescriptionHtml(sampleInfo);
                DescriptionView.Source = new HtmlWebViewSource()
                {
                    Html = htmlString
                };
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }

            try
            {
                LoadSourceCode(sampleInfo);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private static IEnumerable<T> TreeWalker<T>(VisualElement root)
        {
            if (root is not null)
            {
                if (root is T t)
                    yield return t;
                else if (root is IVisualTreeElement it)
                {
                    foreach (var e in it.GetVisualChildren())
                        foreach (var obj in TreeWalker<T>(e as VisualElement))
                            yield return obj;
                }
            }
        }

        private async Task<string> GetDescriptionHtml(SampleInfo sampleInfo)
        {
            string category = sampleInfo.Category;
            if (category.Contains(" "))
            {
                // Make categories with spaces into titlecase folder name.
                category = $"{category.Split(" ")[0]}{category.Split(" ")[1][0].ToString().ToUpper()}{category.Split(" ")[1].Substring(1)}";
            }

            using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync($"Samples/{category}/{sampleInfo.FormalName}/readme.md").ConfigureAwait(false);
            StreamReader r = new StreamReader(fileStream);
            var readmeContent = r.ReadToEnd();
            readmeContent = Markdig.Markdown.ToHtml(readmeContent);

            // Set CSS for dark mode or light mode.
            string markdownCssType = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "github-markdown-dark.css" : "github-markdown.css";
            string cssResource = _assembly.GetManifestResourceNames().Single(n => n.EndsWith($"SyntaxHighlighting.{markdownCssType}"));
            string cssContent = new StreamReader(_assembly.GetManifestResourceStream(cssResource)).ReadToEnd();

#if WINDOWS
            // Remove the readme header on Windows so it doesn't repeat the title.
            cssContent = $"{cssContent} h1 {{\r\n    display: none;\r\n}}";
#endif

            // Convert the image into a string of bytes to embed into the html.
            var sourceStream = await LoadImageStreamAsync(sampleInfo.SampleImageName);
            if (sourceStream is not null)
            {
                using var memoryStream = new MemoryStream();
                sourceStream.CopyTo(memoryStream);
                byte[] image = memoryStream.ToArray();
                memoryStream.Close();

                string imgSrc = $"data:image/jpg;base64,{Convert.ToBase64String(image)}";

                // Replace paths for image.
                readmeContent = readmeContent.Replace($"{sampleInfo.FormalName.ToLower()}.jpg", imgSrc);
            }
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

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public static async Task<Stream> LoadImageStreamAsync(string file)
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (Path.IsPathRooted(file) && File.Exists(file))
                return File.OpenRead(file);

#if __ANDROID__
            var context = Android.App.Application.Context;
            var resources = context.Resources;

            var resourceId = context.GetDrawableId(file);

            if (resourceId > 0)
            {
                var imageUri = new Android.Net.Uri.Builder()
                    .Scheme(Android.Content.ContentResolver.SchemeAndroidResource)
                    .Authority(resources.GetResourcePackageName(resourceId))
                    .AppendPath(resources.GetResourceTypeName(resourceId))
                    .AppendPath(resources.GetResourceEntryName(resourceId))
                    .Build();

                var stream = context.ContentResolver.OpenInputStream(imageUri);
                if (stream is not null)
                    return stream;
            }
#else
		    try
            {
                return await FileSystem.Current.OpenAppPackageFileAsync(file);
            }
            catch
            {
            }
#endif
            return null;
        }

        private void LoadSourceCode(SampleInfo sampleInfo)
        {
            // Get all files in the samples folder.
            var fileNames = new List<string>
            {
                $"Samples/{sampleInfo.Category}/{sampleInfo.FormalName}/{sampleInfo.FormalName}.xaml.cs",
                $"Samples/{sampleInfo.Category}/{sampleInfo.FormalName}/{sampleInfo.FormalName}.xaml",
            };

            // Add every .cs and .xaml file in the directory of the sample.
            foreach (string filepath in fileNames.OrderByDescending(x => x))
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
                        if (!additionalPath.Contains('/'))
                        {
                            string fullPath = $"Samples/{sampleInfo.Category}/{sampleInfo.FormalName}/{additionalPath}";
                            SourceFiles.Add(new SourceCodeFile(fullPath));
                        }
                        else
                        {
                            SourceFiles.Add(new SourceCodeFile(additionalPath));
                        }
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
            if (SourceFiles.Any())
            {
                SelectFile(SourceFiles[_lastViewedFileIndex].Name);
            }

            SourceCodePage.IsVisible = true;
            SampleDetailPage.IsVisible = SampleContentPage.IsVisible = false;
        }

        private async void FileChange_Clicked(object sender, EventArgs e)
        {
            try
            {
                string[] fileList = SourceFiles.Select(s => s.Name.Split('/').Last()).ToArray();
                string result = await Application.Current.MainPage.DisplayActionSheet("Choose file:", "Cancel", null, fileList);

                _lastViewedFileIndex = fileList.ToList().IndexOf(result);

                if (fileList.Contains(result))
                {
                    SelectFile(SourceFiles.FirstOrDefault(s => s.Name.Split("/").Last().Equals(result)).Name);
                }
            }
            // No need to handle any cancellation.
            catch
            {
            }
        }


#if WINDOWS
        private void ScreenshotButton_Clicked(object sender, EventArgs e)
        {
            SaveScreenshot(SampleContentPage);
        }

        // Code here is adapted from the following Stack Overflow answers:
        // https://stackoverflow.com/q/24466482
        // https://stackoverflow.com/a/15537372
        private void SaveScreenshot(VisualElement source)
        {
            double scale = ScreenshotManager.ScreenshotSettings.ScaleFactor.HasValue ? ScreenshotManager.ScreenshotSettings.ScaleFactor.Value : double.NaN;

            var nativeElement = (uiXaml.UIElement)source.Handler.PlatformView;

            int height = (int)(source.DesiredSize.Height * scale);
            int width = (int)(source.DesiredSize.Width * scale);
            var visual = nativeElement.TransformToVisual(null).TransformPoint(new Windows.Foundation.Point(0, 0));

            int X = (int)(visual.X * scale);
            int Y = (int)(visual.Y * scale);

            Bitmap screenshot = new Bitmap(width, height);
            Graphics G = Graphics.FromImage(screenshot);
            G.CopyFromScreen(X, Y, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);

            // If scaling has occurred due to screen scaling we need to resize the image.
            Bitmap resizedScreenshot = new Bitmap(screenshot, new System.Drawing.Size((int)(screenshot.Width / scale), (int)(screenshot.Height / scale)));

            string filePath = $"{ScreenshotManager.ScreenshotSettings.SourcePath}\\MAUI\\MAUI.Samples\\Samples\\" +
                $"{SampleManager.Current.SelectedSample.Category}\\" +
                $"{SampleManager.Current.SelectedSample.FormalName}\\" +
                $"{SampleManager.Current.SelectedSample.FormalName.ToLower()}.jpg";

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
#endif


        private void SelectFile(string fileName)
        {
            SourceCodeView.Source = new HtmlWebViewSource()
            {
                Html = SourceFiles.FirstOrDefault(s => s.Name == fileName).HtmlContent,
            };

            CurrentFileLabel.Text = fileName.Split('/').Last();
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
            get { return _fullContent; }
        }

        public SourceCodeFile(string embeddedResourceFilePath)
        {
            _path = embeddedResourceFilePath;

            _ = LoadContent();
        }
        private async Task LoadContent()
        {
            try
            {
                string baseContent = string.Empty;
                var assembly = Assembly.GetExecutingAssembly();
#if __ANDROID__
                if (_path.EndsWith(".xaml"))
                {
                    var fileName = _path.Split('/').Last();
                    var xamlPath = assembly.GetManifestResourceNames().Single(n => n.EndsWith($"{fileName}"));
                    baseContent = new StreamReader(assembly.GetManifestResourceStream(xamlPath)).ReadToEnd();
                }
                else
                {
                    using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(_path).ConfigureAwait(false);
                    baseContent = new StreamReader(fileStream).ReadToEnd();
                }
#else
                using Stream fileStream = await FileSystem.Current.OpenAppPackageFileAsync(_path).ConfigureAwait(false);
                baseContent = new StreamReader(fileStream).ReadToEnd();
#endif
                // > and < characters will be incorrectly parsed by the html.
                baseContent = baseContent.Replace("<", "&lt;").Replace(">", "&gt;");

                // Set the type of highlighting for the source file.
                string codeClass = _path.EndsWith(".xaml") ? "xml" : "csharp";

                // Set CSS for dark mode or light mode.
                string markdownCssType = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "highlight-dark.css" : "highlight.css";
                string cssResource = assembly.GetManifestResourceNames().Single(n => n.EndsWith($"SyntaxHighlighting.{markdownCssType}"));
                string cssContent = new StreamReader(assembly.GetManifestResourceStream(cssResource)).ReadToEnd();

                // Set the background color. Color values are taken from corresponding css files.
                string backgroundColor = Application.Current.RequestedTheme == Microsoft.Maui.ApplicationModel.AppTheme.Dark ? "#1e1e1e" : "#fff";
                cssContent = $"{cssContent} body {{ background: {backgroundColor};}}";

                // Read javascript content.
                string jsResource = assembly.GetManifestResourceNames().Single(n => n.EndsWith($"SyntaxHighlighting.highlight.js"));
                string jsContent = new StreamReader(assembly.GetManifestResourceStream(jsResource)).ReadToEnd();

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