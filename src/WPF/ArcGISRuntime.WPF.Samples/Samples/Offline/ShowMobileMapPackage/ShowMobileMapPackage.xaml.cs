// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Windows;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Tasks.Offline;
using ArcGISRuntime.Samples.Managers;
using System.IO;
using Esri.ArcGISRuntime.Tasks;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Linq;
using System.Text.RegularExpressions;

namespace ArcGISRuntime.WPF.Samples.ShowMobileMapPackage
{
    public partial class ShowMobileMapPackage
    {
        public ShowMobileMapPackage()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Get path to the used data. This can be a mobile map package (.mmpk) file or 
                // an exploded mobile map package (folder that contains .info file).
                var dataPath = Path.Combine(DataManager.GetDataFolder(), 
                    "SampleData", "Show mobile map package metadata", "NaperilleWaterNetwork_mmpk");

                // Open a local mobile map package
                MobileMapPackage myMobileMapPackage = await MobileMapPackage.OpenAsync(dataPath);

                TitleText.Text = RemoveHTML(myMobileMapPackage.Item.Title) ?? "<No title available>";
                SnippetText.Text = RemoveHTML(myMobileMapPackage.Item.Snippet) ?? "<No describtion available>";
                Thumbnail.Source = await myMobileMapPackage.Item.Thumbnail.ToImageSourceAsync();
                descriptionText.Text = RemoveHTML(myMobileMapPackage.Item.Description);
                CreatedAtText.Text = myMobileMapPackage.Item.Created.ToLocalTime().ToString("MM/dd/yyyy HH:mm:ss");

                // Calculate the size of the offline package and format it for presentation
                FormattedSizeText.Text =
                       BytesToString(Directory.GetFiles(dataPath, "*", SearchOption.AllDirectories)
                           .Sum(t => (new FileInfo(t).Length)));

                var tagsBuilder = new StringBuilder();
                foreach (var tag in myMobileMapPackage.Item.Tags)
                {
                    if (tagsBuilder.Length > 0)
                        tagsBuilder.Append(",");
                    tagsBuilder.Append(tag);
                }
                tagsText.Text = tagsBuilder.ToString();
                creditsText.Text = myMobileMapPackage.Item.AccessInformation;

                loadingIndicator.Visibility = Visibility.Collapsed;
                mapPackageMetadataRegion.Visibility = Visibility.Visible;
            }
            catch (FileNotFoundException ex)
            {
                loadingIndicator.Visibility = Visibility.Collapsed;
                dataNotFoundMessage.Visibility = Visibility.Visible;
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred. " + ex.ToString(), "Sample error");
            }
            finally
            {
                // Hide loading UI
                loadingIndicator.Visibility = Visibility.Collapsed;
            }
        }

        private string BytesToString(long byteCount)
        {
            string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" }; // Longs run out around EB
            if (byteCount == 0)
                return "0" + suf[0];
            long bytes = Math.Abs(byteCount);
            int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            double num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return (Math.Sign(byteCount) * num).ToString() + suf[place];
        }

        private string RemoveHTML(string text)
        {
            var formattedDescription = text;
            string pattern = @"<.*?>";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            Match m = r.Match(text);
            List<Capture> captures = new List<Capture>();
            while (m.Success)
            {
                Group g = m.Groups[0];
                CaptureCollection cc = g.Captures;
                foreach (var capture in cc)
                    captures.Add(capture as Capture);
                m = m.NextMatch();
            }
            captures.Reverse();
            foreach (var capture in captures)
                formattedDescription = formattedDescription.Remove(capture.Index, capture.Length);

            return formattedDescription;
        }
    }
}