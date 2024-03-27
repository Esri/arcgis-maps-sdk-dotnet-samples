// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using System.Text;
using Microsoft.Maui.ApplicationModel;

namespace ArcGIS.Helpers
{
    internal static class FeedbackPrompt
    {
        private const string BugReport = "Bug Report";
        private const string FeatureRequest = "Feature Request";

        public async static Task ShowFeedbackPromptAsync()
        {
            await Application.Current.MainPage.DisplayActionSheet("Open an issue on GitHub:", "Cancel", null, [BugReport, FeatureRequest]).ContinueWith((result) =>
            {
                string link;
                switch (result.Result)
                {
                    case BugReport:
                        var sb = new StringBuilder("https://github.com/Esri/arcgis-maps-sdk-dotnet-samples/issues/new?assignees=&labels=Type%3A+Bug&projects=&template=bug_report.yml&title=%5BBug%5D");
                        if (SampleManager.Current.SelectedSample != null)
                        {
                            sb.Append("+&impacted-samples=");
                            sb.Append(SampleManager.Current.SelectedSample.FormalName);
                        }
                        _ = Browser.Default.OpenAsync(new Uri(sb.ToString()), BrowserLaunchMode.SystemPreferred);
                        break;

                    case FeatureRequest:
                        link =
                            "https://github.com/Esri/arcgis-maps-sdk-dotnet-samples/issues/new?assignees=&labels=Type%3A+Feature&projects=&template=feature_request.yml&title=%5BFeature%5D";
                        _ = Browser.Default.OpenAsync(new Uri(link), BrowserLaunchMode.SystemPreferred);
                        break;

                    case "Cancel":
                        break;
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}