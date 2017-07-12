// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Xamarin.Forms;


namespace ArcGISRuntimeXamarin.Samples.ShowMobileMapPackage
{
    public partial class ShowMobileMapPackage : ContentPage
    {

        public ShowMobileMapPackage()
        {
            InitializeComponent();

            Title = "Show mobile map package metadata";

            InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                // Get path to the used data. This can be a mobile map package (.mmpk) file or 
                // an exploded mobile map package (folder that contains .info file).
                var dataPath = Path.Combine(DataManager.GetDataFolder(),
                    "SampleData", "Show mobile map package metadata", "NaperilleWaterNetwork_mmpk");
                
                // Open a local mobile map package
                var myMobileMapPackage = await MobileMapPackage.OpenAsync(dataPath);

                ItemTitle.Text = myMobileMapPackage.Item.Title;

                var stream = await myMobileMapPackage.Item.Thumbnail.GetEncodedBufferAsync();
                ItemImage.Source = ImageSource.FromStream(() => stream);

                ItemCreationDate.Text = myMobileMapPackage.Item.Created.ToLocalTime().ToString("MM/dd/yyyy HH:mm:ss");
                ItemSnippet.Text = myMobileMapPackage.Item.Snippet;
                ItemDescription.Text = RemoveHtmlTags(myMobileMapPackage.Item.Description);
                ItemCredits.Text = $"Credits: {myMobileMapPackage.Item.AccessInformation}";
                ItemTags.Text = $"Tags: {string.Join(",", myMobileMapPackage.Item.Tags)}";

                ItemSize.Text = BytesToString(Directory.GetFiles(dataPath, "*", SearchOption.AllDirectories).Sum(f => f.Length));

                ItemInfo.IsVisible = true;
                DownloadInfo.IsVisible = false;                 
            }
            catch (FileNotFoundException)
            {
                ItemInfo.IsVisible = false;
                DownloadInfo.IsVisible = true;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
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

        private string RemoveHtmlTags(string html)
        {
            return Regex.Replace(html, "<.+?>", string.Empty);
        }

        private async void OnDownloadDataClicked(object sender, EventArgs e)
        {
            try
            {
                if (SampleManager.Current.SelectedSample.DataItemIds != null)
                {
                    foreach (string id in SampleManager.Current.SelectedSample.DataItemIds)
                    {
                        await DataManager.GetData(id, SampleManager.Current.SelectedSample.Name);
                    }
                }
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}