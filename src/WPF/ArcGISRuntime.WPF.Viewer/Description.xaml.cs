// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System.IO;
using ArcGISRuntime.Samples.Shared.Models;

namespace ArcGISRuntime.WPF.Viewer
{
    public partial class Description
    {
        private readonly MarkedNet.Marked _markdownRenderer = new MarkedNet.Marked();

        public Description()
        {
            InitializeComponent();
        }

        public void SetSample(SampleInfo sample)
        {
            DataContext = sample;

            string folderPath = sample.Path;
            string cssPath = System.IO.Path.Combine(App.ResourcePath, "Resources", "github-markdown.css");
            string readmePath = System.IO.Path.Combine(folderPath, "Readme.md");
            string readmeContent = System.IO.File.ReadAllText(readmePath);
            string overrideCssPath = System.IO.Path.Combine(App.ResourcePath, "Resources", "hide-header.css");
            readmeContent = _markdownRenderer.Parse(readmeContent);

            string htmlString = "<!doctype html><head><base href=\"" + readmePath + "\"><link rel=\"stylesheet\" href=\"" + cssPath + "\" /><link rel=\"stylesheet\" href=\"" + overrideCssPath + "\" /></head><body class=\"markdown-body\">" + readmeContent + "</body>";
            DescriptionView.NavigateToString(htmlString);
        }
    }
}