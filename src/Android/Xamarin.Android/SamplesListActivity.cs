// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using ArcGISRuntime.Managers;
using ArcGISRuntime.Models;

namespace ArcGISRuntime
{
    [Activity(Label = "Samples")]
    public class SamplesListActivity : Activity
    {
        List<SampleModel> _listSampleItems;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set the ContentView to the SamplesList (a ListView)
            SetContentView(Resource.Layout.SamplesList);

            // Retrieve the selected category from the Categories List. 
            var selectedCategory = Intent.GetIntExtra("SelectedCategory", 0);

            // Get the listing of categories; Would be good to eventually be able to pass
            // this info, but Android doesn't allow passing Complex types. 
            var sampleCategories = SampleManager.Current.GetSamplesAsTree();  // TODO: Cache this in the SampleManager
            var category = sampleCategories[selectedCategory] as TreeItem;

            // Loop through the categories and create a list of each subcategory and the 
            // items (Samples) it contains.
            List<Object> listSubCategories = new List<Object>();
            for (int i = 0; i < category.Items.Count; i++)
            {
                listSubCategories.Add((category.Items[i] as TreeItem).Items);
            }

            // With the newly-created list of subcategory items, create a new list for the
            // adapter that contains just the individual Samples.
            _listSampleItems = new List<SampleModel>();
            foreach (List<object> subCategoryItem in listSubCategories)
            {
                foreach (var sample in subCategoryItem)
                {
                    _listSampleItems.Add(sample as SampleModel);
                }
            }

            var samplesAdapter = new SamplesListAdapter(this, _listSampleItems);

            ListView samplesListView = FindViewById<ListView>(Resource.Id.samplesListView);
            samplesListView.Adapter = samplesAdapter;
            samplesListView.ItemClick += SamplesListView_ItemClick;
        }

        private async void SamplesListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var sampleName = string.Empty;

            try
            {
                // Get the clicked item along with its name and namespace
                var item = _listSampleItems[e.Position];
                sampleName = item.SampleName;
                var sampleNamespace = item.SampleNamespace;

                //If Offline data is required for the sample to work download it 
                if (item.RequiresOfflineData)
                {
                    foreach (string id in item.DataItemIds)
                    { 
                        //TODO - Add splash screen/progress bar
                        await DataManager.GetData(id, item.SampleName);
                    }
                }

                // Each sample is an Activity, so locate it and launch it via an Intent
                Type t = Type.GetType(sampleNamespace + "." + sampleName);
                var newActivity = new Intent(this, t);

                // Call a function to clear existing credentials
                ClearCredentials();

                StartActivity(newActivity);
            }
            catch (Exception ex)
            {
                AlertDialog.Builder bldr = new AlertDialog.Builder(this);
                var dialog = bldr.Create();
                dialog.SetTitle("Unable to load " + sampleName);
                dialog.SetMessage(ex.Message);
                dialog.Show();
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