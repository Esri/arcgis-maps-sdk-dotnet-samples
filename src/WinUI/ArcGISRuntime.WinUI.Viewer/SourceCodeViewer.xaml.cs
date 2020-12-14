// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.WinUI.Samples.ArcGISVectorTiledLayerUrl;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArcGISRuntime.WinUI.Viewer
{
    public partial class SourceCode
    {
        // List of all of the tabs that will load.
        private List<TabViewItem> tabs;

        public SourceCode()
        {
            InitializeComponent();
        }

        public void LoadSourceCode()
        {
            string folderPath = SampleManager.Current.SelectedSample.Path;

            // Source code of the file.
            string source;

            // Create a new list of TabViewItems.
            tabs = new List<TabViewItem>();

            // Loop through all .cs and .xaml files.
            foreach (string filepath in Directory.GetFiles(folderPath)
                .Where(candidate => candidate.EndsWith(".cs") || candidate.EndsWith(".xaml")))
            {
                // Get the source text from the file.
                source = File.ReadAllText(filepath);

                // Create a new tab.
                TabViewItem newTab = new TabViewItem();

                // Set the tab text to the file name.
                newTab.Header = Path.GetFileName(filepath);
                /* TODO: Monaco.CodeEditor not available for WinUI yet. See https://github.com/hawkerm/monaco-editor-uwp/pull/32
                // Create the code viewer.
                Monaco.CodeEditor viewer = new Monaco.CodeEditor();
                viewer.Options.ReadOnly = true;
                viewer.Text = source;
                
                 // Change Monaco language for C# files.
                if (filepath.EndsWith(".cs")) { viewer.CodeLanguage = "csharp"; }

                */
                // Instead use textbox for now
                TextBox viewer = new TextBox();
                viewer.IsReadOnly = true;
                viewer.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Courier New");
                viewer.AcceptsReturn = true;
                viewer.VerticalAlignment = Microsoft.UI.Xaml.VerticalAlignment.Stretch;
                viewer.Text = source;

                // Adjust tabs for dark mode.
                if (App.Current.RequestedTheme == Microsoft.UI.Xaml.ApplicationTheme.Dark)
                {
                    Tabs.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Dark;
                    newTab.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Dark;
                }

                // Set the tabs content to the code viewer.
                newTab.Content = viewer;

                // Add the tab to the beginning of the list.
                tabs.Insert(0, newTab);
            }

            // Set the Tab source to the list of tabs.
            Tabs.TabItemsSource = tabs;
        }
    }
}