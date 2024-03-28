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
using ArcGIS.Samples.Managers;

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
            string link = 
                "https://github.com/Esri/arcgis-maps-sdk-dotnet-samples/issues/new?assignees=&labels=Type%3A+Bug&projects=&template=bug_report.yml&title=%5BBug%5D";
            
            if (SampleManager.Current.SelectedSample != null)
            {
                link += "+&impacted-samples=" + SampleManager.Current.SelectedSample.FormalName;
            }

            OpenWebpage(link);
        }

        private void FeatureRequestButton_Click(object sender, RoutedEventArgs e)
        {
            string link =
                "https://github.com/Esri/arcgis-maps-sdk-dotnet-samples/issues/new?assignees=&labels=Type%3A+Feature&projects=&template=feature_request.yml&title=%5BFeature%5D";
            OpenWebpage(link);
        }

        private void OpenWebpage(string link)
        {
            try
            {
#if NETFRAMEWORK
            Process.Start(link);
#elif NETCOREAPP
            Process.Start(new ProcessStartInfo
            {
                FileName = link,
                UseShellExecute = true
            });
#endif
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}