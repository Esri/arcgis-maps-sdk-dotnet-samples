// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Viewer
{
    public partial class SourceCode
    {
        // Holds source files (as strings of html).
        private Dictionary<string, string> _sourceFiles;

        public SourceCode()
        {
            InitializeComponent();
        }

        public void LoadSourceCode()
        {
            string folderPath = SampleManager.Current.SelectedSample.Path;

            // Filepaths for the css and js files used for syntax highlighting.
            string cssPath = folderPath.Substring(0, folderPath.IndexOf("Samples")) + "Resources\\SyntaxHighlighting\\highlight.css";
            string cssBackgroundPath = folderPath.Substring(0, folderPath.IndexOf("Samples")) + "Resources\\SyntaxHighlighting\\screen.css";
            string jsPath = folderPath.Substring(0, folderPath.IndexOf("Samples")) + "Resources\\SyntaxHighlighting\\highlight.pack.js";

            // Dictionary holds html strings for source code as values. Keys are strings of filepaths.
            _sourceFiles = new Dictionary<string, string>();

            // Clear out any old items when loading for a new sample.
            FileSelection.Items.Clear();

            // Start of each html string.
            string htmlStart =
                "<html>" +
                "<head>" +
                "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=edge\">" +
                "<link rel=\"stylesheet\" href=\"" + cssPath + "\">" +
                "<link rel=\"stylesheet\" href=\"" + cssBackgroundPath + "\">" +
                "<script type=\"text/javascript\" src=\"" + jsPath + "\"></script>" +
                "<script>hljs.initHighlightingOnLoad();</script>" +
                "</head>" +
                "<body>" +
                "<pre>";

            // End of each html string.
            string htmlEnd =
                "</pre>" +
                "</body>" +
                "</html>";

            // Index of the .cs file. -1 will result in no selection if a .cs file is not found.
            int csIndex = -1;

            // Source code of the file.
            string source;

            // Add every .cs and .xaml file in the directory of the sample.
            foreach (string filepath in Directory.GetFiles(folderPath))
            {
                try
                {
                    if (filepath.EndsWith(".cs"))
                    {
                        // Get the source text from the file.
                        source = File.ReadAllText(filepath);

                        // Build the html.
                        source =
                            htmlStart +
                            "<code class=\"csharp\">" +
                            source +
                            "</code>" +
                            htmlEnd;

                        // Set the index of the .cs file.
                        csIndex = _sourceFiles.Count;
                    }
                    else if (filepath.EndsWith(".xaml"))
                    {
                        // Get the source text from the file.
                        source = File.ReadAllText(filepath);

                        // Replace the tag characters so that the html renders correctly.
                        source = source.Replace("<", "&lt;");
                        source = source.Replace(">", "&gt;");

                        // Build the html.
                        source =
                            htmlStart +
                            "<code class=\"xml\">" +
                            source +
                            "</code>" +
                            htmlEnd;
                    }
                    else
                    {
                        // Continue looping over other files if file is not .cs or .xaml.
                        continue;
                    }
                    // Add the source html to the _sourceFiles dictionary.
                    _sourceFiles[filepath] = source;

                    // Add the file as a selectable item in the combobox.
                    FileSelection.Items.Add(filepath);
                }
                catch (Exception e)
                {
                    // Any files that failed to be read will have error messages printed to the console for debugging.
                    Console.WriteLine(e.Message);
                }
            }

            // Default to the index of the last .cs file.
            FileSelection.SelectedIndex = csIndex;
        }

        private void SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Check that there are Items in the combobox. Prevents error when switching between samples.
            if(!FileSelection.Items.IsEmpty)
            {
                // Set the web browser to display the source code of the selected file.
                sourceCodeBrowser.NavigateToString(_sourceFiles[FileSelection.SelectedValue.ToString()]);
            }
        }
    }
}