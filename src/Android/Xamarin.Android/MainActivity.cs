// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Android.App;
using Android.Widget;
using Android.OS;
using System.Collections.Generic;
using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using Android.Content;
using System.Linq;
using Esri.ArcGISRuntime.Security;

namespace ArcGISRuntime
{
    [Activity(Label = "ArcGIS Runtime SDK for .NET", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private List<SearchableTreeNode> _sampleCategories;
        private List<SearchableTreeNode> _filteredSampleCategories;
        private ExpandableListView _categoriesListView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.CategoriesList);

            try
            {
                // Initialize the SampleManager and create the Sample Categories.
                SampleManager.Current.Initialize();
                _sampleCategories = SampleManager.Current.FullTree.Items.OfType<SearchableTreeNode>().ToList();
                _filteredSampleCategories = _sampleCategories;

                // Set up the custom ArrayAdapter for displaying the Categories.
                var categoriesAdapter = new CategoriesAdapter(this, _sampleCategories);
                _categoriesListView = FindViewById<ExpandableListView>(Resource.Id.categoriesListView);
                _categoriesListView.SetAdapter(categoriesAdapter);
                _categoriesListView.ChildClick += CategoriesListViewOnChildClick;
                _categoriesListView.DividerHeight = 2;
                _categoriesListView.SetGroupIndicator(null);

                // Set up the search filtering.
                SearchView searchBox = FindViewById<SearchView>(Resource.Id.categorySearchView);
                searchBox.QueryTextChange += SearchBoxOnQueryTextChange;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void SearchBoxOnQueryTextChange(object sender, SearchView.QueryTextChangeEventArgs queryTextChangeEventArgs)
        {
            SearchableTreeNode stnResult = SampleManager.Current.FullTree.Search(sample => SampleManager.Current.SampleSearchFunc(sample, queryTextChangeEventArgs.NewText));
            if (stnResult != null)
            {
                _filteredSampleCategories = stnResult.Items.OfType<SearchableTreeNode>().ToList();
            }
            else
            {
                _filteredSampleCategories = new List<SearchableTreeNode>();
            }
                
            _categoriesListView.SetAdapter(new CategoriesAdapter(this, _filteredSampleCategories));

            // Expand all entries; makes it easier to see search results.
            for (int index = 0; index < _filteredSampleCategories.Count; index++)
            {
                _categoriesListView.ExpandGroup(index);
            }
        }

        private async void CategoriesListViewOnChildClick(object sender, ExpandableListView.ChildClickEventArgs childClickEventArgs)
        {
            var sampleName = string.Empty;

            try
            {
                // Call a function to clear existing credentials.
                ClearCredentials();

                // Get the clicked item.
                SampleInfo item = (SampleInfo)_filteredSampleCategories[childClickEventArgs.GroupPosition].Items[childClickEventArgs.ChildPosition];

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

