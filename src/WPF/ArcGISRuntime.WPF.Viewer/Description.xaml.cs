// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Models;
using System;

namespace ArcGISRuntime.WPF.Viewer
{
    public partial class Description
    {
        MarkedNet.Marked markdownRenderer = new MarkedNet.Marked();
        public Description()
        {
            InitializeComponent();
        }

        public void SetSample(SampleInfo sample)
        {
            DataContext = sample;

            string folderPath = sample.Path;
            string readmePath = System.IO.Path.Combine(folderPath, "Readme.md");
            string readmeContent = System.IO.File.ReadAllText(readmePath);
            string cssPath = folderPath.Substring(0, folderPath.LastIndexOf("Samples")) + "Resources\\github-markdown.css";
            readmeContent = markdownRenderer.Parse(readmeContent);

            string htmlString = "<!doctype html><head><base href=\"" + readmePath + "\"><link rel=\"stylesheet\" href=\"" + cssPath + "\" /></head><body class=\"markdown-body\">" + readmeContent + "</body>";
            DescriptionView.NavigateToString(htmlString);
        }
    }
}
