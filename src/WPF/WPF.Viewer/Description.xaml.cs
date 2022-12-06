// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Shared.Models;
using System.Diagnostics;
using System.IO;

namespace ArcGIS.WPF.Viewer
{
    public partial class Description
    {
        public Description()
        {
            InitializeComponent();
        }

        public void SetSample(SampleInfo sample)
        {
            DataContext = sample;

            string folderPath = sample.Path;
            string cssPath = Path.Combine(App.ResourcePath, "Resources", "github-markdown.css");
            string readmePath = Path.Combine(folderPath, "Readme.md");
            string readmeContent = File.ReadAllText(readmePath);
            string overrideCssPath = Path.Combine(App.ResourcePath, "Resources", "hide-header.css");
            readmeContent = Markdig.Markdown.ToHtml(readmeContent);

            string htmlString = "<!doctype html><head><base href=\"" + readmePath + "\"><link rel=\"stylesheet\" href=\"" + cssPath + "\" /><link rel=\"stylesheet\" href=\"" + overrideCssPath + "\" /></head><body class=\"markdown-body\">" + readmeContent + "</body>";

            // Set the html in web browser.
            DescriptionView.DocumentText = htmlString;

            // Remove the exisiting handler.
            DescriptionView.Document.Click -= HyperlinkClick;

            // Add an event handler for hyperlink clicks.
            DescriptionView.Document.Click += HyperlinkClick;

            // Create an event handler for canceling the default hyperlink behavior.
            DescriptionView.NewWindow += (s, e) => { e.Cancel = true; };
        }

        private void HyperlinkClick(object sender, System.Windows.Forms.HtmlElementEventArgs e)
        {
            // Get the html element that the user clicked on.
            System.Windows.Forms.HtmlElement src = DescriptionView.Document?.GetElementFromPoint(e.ClientMousePosition);

            // Check if the element is a hyperlink.
            if (src?.OuterHtml.StartsWith("<A href=") == true)
            {
                // Parse the url from the hyperlink html.
                string url = src.OuterHtml.Split('\"')[1];

                // Open the url.
                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
        }
    }
}