// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using Esri.ArcGISRuntime.Security;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntime
{
    [Activity(Label = "Samples")]
    public class SamplesListActivity : Activity
    {
        private List<SampleInfo> _listSampleItems;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set the ContentView to the SamplesList (a ListView).
            SetContentView(Resource.Layout.SamplesList);

            // Retrieve the selected category from the Categories List.
            var selectedCategory = Intent.GetIntExtra("SelectedCategory", 0);

            // Get the listing of categories; Would be good to eventually be able to pass
            // this info, but Android doesn't allow passing Complex types.
            List<object> sampleCategories = SampleManager.Current.FullTree.Items;  // TODO: Cache this in the SampleManager
            SearchableTreeNode category = (SearchableTreeNode)sampleCategories[selectedCategory];

            // Loop through the categories and create a list of samples.
            _listSampleItems = category.Items.OfType<SampleInfo>().ToList();

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
                // Call a function to clear existing credentials.
                ClearCredentials();

                // Get the clicked item.
                SampleInfo item = _listSampleItems[e.Position];

                // Download any offline data before showing the sample.
                if (item.OfflineDataItems != null)
                {
                    // Show the waiting dialog.
                    ProgressDialog mDialog = new ProgressDialog(this) { Indeterminate = true };
                    mDialog.SetMessage("Downloading Data");
                    mDialog.Show();

                    // Begin downloading data.
                    await DataManager.EnsureSampleDataPresent(item);

                    // Hide the progress dialog.
                    mDialog.Dismiss();
                }

                // Each sample is an Activity, so locate it and launch it via an Intent.
                var newActivity = new Intent(this, item.SampleType);

                // Start the activity.
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

        private static void ClearCredentials()
        {
            // Clear credentials (if any) from previous sample runs.
            foreach (Credential cred in AuthenticationManager.Current.Credentials)
            {
                AuthenticationManager.Current.RemoveCredential(cred);
            }
        }
    }
}