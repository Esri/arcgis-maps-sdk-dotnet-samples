// Copyright 2016 Esri.
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
using System.Linq;
using Xamarin.Forms;
using ArcGISRuntime.Samples.Shared.Models;

namespace ArcGISRuntime
{
    public partial class SampleListPage : ContentPage
    {
        private string _categoryName;
        private List<SampleInfo> _listSampleItems;

        public SampleListPage(string name)
        {
            _categoryName = name;
            Initialize();

            InitializeComponent();

            Title = _categoryName;
        }

        void Initialize()
        {
            var sampleCategories = SampleManager.Current.FullTree.Items;
            var category = sampleCategories.FirstOrDefault(x => (x as SearchableTreeNode).Name == _categoryName) as SearchableTreeNode;
            _listSampleItems = category.Items.OfType<SampleInfo>().ToList();
            BindingContext = _listSampleItems;
        }

        async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            try
            {
                var item = (SampleInfo)e.Item;
                
                if (item.OfflineDataItems != null)
                {
                    // Show wait page
                    await Navigation.PushAsync(new WaitPage(), false);

                    // Wait for sample data download
                    await DataManager.EnsureSampleDataPresent(item);

                    // Pop the stack
                    await Navigation.PopAsync(false);
                }

                var sampleControl = (ContentPage) SampleManager.Current.SampleToControl(item); 
                SamplePage page = new SamplePage(sampleControl, item);
                await Navigation.PushAsync(page, true);

                // Call a function to clear existing credentials
                ClearCredentials();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(string.Format("Exception occurred on OnItemTapped. Exception = ", ex));
            }
        }

        private void ClearCredentials()
        {
            // Clear credentials (if any) from previous sample runs
            var creds = Esri.ArcGISRuntime.Security.AuthenticationManager.Current.Credentials;
            for (var i = creds.Count() - 1; i >= 0; i--)
            {
                var c = creds.ElementAtOrDefault(i);
                if (c != null)
                {
                    Esri.ArcGISRuntime.Security.AuthenticationManager.Current.RemoveCredential(c);
                }
            }
        }
    }
}
