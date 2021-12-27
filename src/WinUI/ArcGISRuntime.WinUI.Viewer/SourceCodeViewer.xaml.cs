// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntime.WinUI.Viewer
{
    public partial class SourceCode
    {
        // List of all of the tabs that will load.
        private List<TabViewItem> tabs;

        private const string FolderMapping = "arcgisruntime.viewer";

        private List<SourceCodeFile> _sourceFiles;

        public SourceCode()
        {
            InitializeComponent();
        }

        public async Task LoadSourceCodeAsync()
        {
            // Create a new list of TabViewItems.
            tabs = new List<TabViewItem>();

            // Create a list of source files for the sample.
            _sourceFiles = new List<SourceCodeFile>();

            foreach (string filepath in Directory.GetFiles(SampleManager.Current.SelectedSample.Path)
                .Where(candidate => candidate.EndsWith(".cs") || candidate.EndsWith(".xaml")))
            {
                _sourceFiles.Add(new SourceCodeFile(filepath));
            }

            // Add additional class files from the sample.
            if (SampleManager.Current.SelectedSample.ClassFiles != null)
            {
                foreach (string additionalPath in SampleManager.Current.SelectedSample.ClassFiles)
                {
                    string actualPath = Path.Combine(SampleManager.Current.SelectedSample.Path, "..", "..", "..", additionalPath);
                    _sourceFiles.Add(new SourceCodeFile(actualPath));
                }
            }

            // Set up the tabs.
            foreach (SourceCodeFile file in _sourceFiles)
            {
                // Create a new tab.
                TabViewItem newTab = new TabViewItem() { IsClosable = false };

                // Set the tab text to the file name.
                newTab.Header = Path.GetFileName(file.FilePath);

                // Add the tab to the beginning of the list.
                tabs.Insert(0, newTab);
            }

            // Set the Tab source to the list of tabs.
            Tabs.TabItemsSource = tabs;

            // Set up web view to display syntax-highlighted code.
            await WebView.EnsureCoreWebView2Async();

            string applicationPath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            WebView.CoreWebView2.SetVirtualHostNameToFolderMapping(FolderMapping, applicationPath, CoreWebView2HostResourceAccessKind.Allow);

            Tabs.SelectionChanged += TabChanged;
            Tabs.SelectedIndex = 0;
            
        }

        private void TabChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = Tabs.SelectedItem as TabViewItem;
            string content = _sourceFiles.FirstOrDefault(x => x.Name == item.Header.ToString()).HtmlContent;
            WebView.NavigateToString(content);
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
                    string baseContent = File.ReadAllText(_path);

                    // > and < characters will be incorrectly parsed by the html.
                    baseContent = baseContent.Replace("<", "&lt;").Replace(">", "&gt;");

                    // Build the html.
                    _fullContent =
                        htmlStart.Replace("{csspath}", cssPath).Replace("{jspath}", jsPath) +
                        "<code class=\"csharp\">" +
                        baseContent +
                        "</code>" +
                        htmlEnd;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }
    }
}