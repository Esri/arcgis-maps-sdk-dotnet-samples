// Copyright 2024 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using System;
using System.Diagnostics;
using System.Windows;

namespace ArcGIS
{
    public partial class FeedbackWindow : Window
    {
        public FeedbackWindow()
        {
            InitializeComponent();
        }

        private void BugReportButton_Click(object sender, RoutedEventArgs e)
        {
            string link = "https://github.com/Esri/arcgis-maps-sdk-dotnet-samples/issues/new?assignees=&labels=t%2Fbug&projects=&template=bug_report.yml";
            Process.Start(link);
        }

        private void FeatureRequestButton_Click(object sender, RoutedEventArgs e)
        {
            string link = "https://github.com/Esri/arcgis-maps-sdk-dotnet-samples/issues/new?assignees=&labels=t%2Fbug&projects=&template=feature_request.yml";
            Process.Start(link);
        }
    }
}