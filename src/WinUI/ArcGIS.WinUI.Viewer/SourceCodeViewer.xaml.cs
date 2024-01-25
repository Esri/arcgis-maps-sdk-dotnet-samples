// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Viewer
{
    public partial class SourceCode
    {
        private SourceCodeFile _selectedFile;

        // Pattern for mapping a local folder to a url host name.
        private const string FolderMapping = "arcgisruntime.viewer";

        // List of source code files relevant to the selected sample.
        public ObservableCollection<SourceCodeFile> SourceFiles { get; } = new ObservableCollection<SourceCodeFile>();

        public SourceCodeFile SelectedSourceFile
        {
            get => _selectedFile;
            set
            {
                if (value != _selectedFile)
                {
                    _selectedFile = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SelectedSourceFile)));
                }
            }
        }

        public SourceCode()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public async Task LoadSourceCodeAsync()
        {
            // Create a list of source files for the sample.
            SourceFiles.Clear();

            foreach (string filepath in Directory.GetFiles(SampleManager.Current.SelectedSample.Path)
                .Where(candidate => candidate.EndsWith(".cs") || candidate.EndsWith(".xaml")))
            {
                SourceFiles.Insert(0, new SourceCodeFile(filepath));
            }

            // Add additional class files from the sample.
            if (SampleManager.Current.SelectedSample.ClassFiles != null)
            {
                foreach (string additionalPath in SampleManager.Current.SelectedSample.ClassFiles)
                {
                    string actualPath = Path.Combine(SampleManager.Current.SelectedSample.Path, "..", "..", "..", additionalPath);
                    SourceFiles.Add(new SourceCodeFile(actualPath));
                }
            }

            SelectedSourceFile = SourceFiles[0];

            try
            {
                // Load web view.
                await WebView.EnsureCoreWebView2Async();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                return;
            }

            // Set up folder mapping for local syntax highlighting resources.
            string applicationPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(FolderMapping, applicationPath, CoreWebView2HostResourceAccessKind.Allow);
        }

        public class SourceCodeFile
        {
            // Start of each html string.
            private const string htmlStart =
                "<html>" +
                "<head>" +
                "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=11\">" +
                "<link rel=\"stylesheet\" href=\"{csspath}\">" +
                "<script type=\"text/javascript\" src=\"{jspath}\"></script>" +
                "<script>hljs.initHighlightingOnLoad();</script>" +
                "</head>" +
                "<body>" +
                "<pre>";

            private const string htmlEnd =
                "</pre>" +
                "</body>" +
                "</html>";

            private string _path;
            private string _fullContent;

            public string FilePath => _path;

            public string Name => Path.GetFileName(_path);

            public string SourceCode { get; set; }

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

            public SourceCodeFile(string sourceFilePath)
            {
                _path = sourceFilePath;
            }

            private void LoadContent()
            {
                // Filepaths for the css and js files used for syntax highlighting.
                string cssPath = Path.Combine($"https://{FolderMapping}", "Resources", "SyntaxHighlighting", "highlight.css");
                string jsPath = Path.Combine($"https://{FolderMapping}", "Resources", "SyntaxHighlighting", "highlight.pack.js");

                // Source code of the file.
                try
                {
                    string baseContent = SourceCode = File.ReadAllText(_path);

                    // Set the type of highlighting for the source file.
                    string codeClass = _path.EndsWith(".xaml") ? "xml" : "csharp";

                    // > and < characters will be incorrectly parsed by the html.
                    baseContent = baseContent.Replace("<", "&lt;").Replace(">", "&gt;");

                    // Build the html.
                    _fullContent =
                        htmlStart.Replace("{csspath}", cssPath).Replace("{jspath}", jsPath) +
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

        private void CopyCodeButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            // Copy the source code to the clipboard.
            var dataPackage = new Windows.ApplicationModel.DataTransfer.DataPackage();
            dataPackage.SetText(SelectedSourceFile.SourceCode);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(dataPackage);
        }

        private void File_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update the selected source file.
            SelectedSourceFile = (sender as ComboBox).SelectedItem as SourceCodeFile;

            // Display the html content in the web view.
            WebView.NavigateToString(SelectedSourceFile.HtmlContent);
        }
    }
}