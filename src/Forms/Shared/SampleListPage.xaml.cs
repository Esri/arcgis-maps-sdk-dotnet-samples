// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;

namespace ArcGISRuntime
{
    public partial class SampleListPage
    {
        private readonly string _categoryName;
        private List<SampleInfo> _listSampleItems;

        public SampleListPage(string name)
        {
            _categoryName = name;

            Initialize();

            InitializeComponent();

            Title = _categoryName;
        }

        private void Initialize()
        {
            // Get the list of sample categories.
            List<object> sampleCategories = SampleManager.Current.FullTree.Items;

            // Get the tree node for this category.
            var category = sampleCategories.FirstOrDefault(x => ((SearchableTreeNode)x).Name == _categoryName) as SearchableTreeNode;

            // Get the samples from the category.
            _listSampleItems = category?.Items.OfType<SampleInfo>().ToList();

            // Update the binding to show the samples.
            BindingContext = _listSampleItems;
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            // Call a function to clear existing credentials.
            ClearCredentials();

            try
            {
                // Get the selected sample.
                SampleInfo item = (SampleInfo)e.Item;

                // Load offline data before showing the sample.
                if (item.OfflineDataItems != null)
                {
                    // Show the wait page.
                    await Navigation.PushModalAsync(new WaitPage { Title = item.SampleName }, false);

                    // Wait for the sample data download.
                    await DataManager.EnsureSampleDataPresent(item);

                    // Remove the waiting page.
                    await Navigation.PopModalAsync(false);
                }

                // Get the sample control from the selected sample.
                var sampleControl = (ContentPage)SampleManager.Current.SampleToControl(item);

                // Create the sample display page to show the sample and the metadata.
                SamplePage page = new SamplePage(sampleControl, item);

                // Show the sample.
                await Navigation.PushAsync(page, true);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Exception occurred on OnItemTapped. Exception = " + ex);
            }
        }

        private static void ClearCredentials()
        {
            foreach (Credential cred in AuthenticationManager.Current.Credentials)
            {
                AuthenticationManager.Current.RemoveCredential(cred);
            }
        }
    }
}