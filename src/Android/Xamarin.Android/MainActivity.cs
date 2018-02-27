// Copyright 2016 Esri.
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

namespace ArcGISRuntime
{
    [Activity(Label = "ArcGIS Runtime SDK for .NET", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        List<SearchableTreeNode> _sampleCategories;

        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            SetContentView(Resource.Layout.CategoriesList);

            try
            {
                // Initialize the SampleManager and create the Sample Categories
                SampleManager.Current.Initialize();
                _sampleCategories = SampleManager.Current.FullTree.Items.OfType<SearchableTreeNode>().ToList();

                // Set up the custom ArrayAdapter for displaying the Categories.
                var categoriesAdapter = new CategoriesAdapter(this, _sampleCategories);
                ListView categoriesListView = FindViewById<ListView>(Resource.Id.categoriesListView);
                categoriesListView.Adapter = categoriesAdapter;

                categoriesListView.ItemClick += CategoriesItemClick;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void CategoriesItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            // Don't need this currently, but would be nice eventually to pass the category instead of the 
            // position. TBD since you can't pass complex types via Intents. 
            var category = _sampleCategories[e.Position];

            var samplesListActivity = new Intent(this, typeof(SamplesListActivity));

            // Pass the index of the selected category to the SamplesListActivity
            samplesListActivity.PutExtra("SelectedCategory", e.Position);
            StartActivity(samplesListActivity);
        }
    }
}

