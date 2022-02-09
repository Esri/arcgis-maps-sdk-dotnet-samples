// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using AndroidX.Core.Content;
using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Managers;
using ArcGISRuntime.Samples.Shared.Models;
using Esri.ArcGISRuntime.Security;
using Google.AR.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

            // Initialize Xamarin.Essentials for Auth usage.
            Xamarin.Essentials.Platform.Init(this, bundle);

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

                // Add a button that brings up settings.
                Button settingsButton = FindViewById<Button>(Resource.Id.settingsButton);
                settingsButton.Click += (s, e) => PromptForKey();

                // Check the existing API key for validity.
                _ = CheckApiKey();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task CheckApiKey()
        {
            // Attempt to load a locally stored API key.
            await ApiKeyManager.TrySetLocalKey();

            // Check that the current API key is valid.
            ApiKeyStatus status = await ApiKeyManager.CheckKeyValidity();
            if (status != ApiKeyStatus.Valid)
            {
                PromptForKey();
            }
        }

        private void PromptForKey()
        {
            // Bring up API Key prompt screen.
            var keyActivity = new Intent(this, typeof(ApiKeyPrompt));
            StartActivity(keyActivity);
        }

        protected override void OnResume()
        {
            // Garbage collect when sample is closed.
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            Java.Lang.JavaSystem.Gc();
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
            Java.Lang.JavaSystem.Gc();
            base.OnResume();
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

                // Check for AR compatiblity.
                if (item.Category == "Augmented reality")
                {
                    try
                    {
                        // Get camera permission, then try to create an AR session.
                        await AskForCameraPermission();
                        var arSession = new Session(this);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"{ex.GetType().Name} {ex.Message}");
                        new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle("Augmented reality not supported").Show();
                        return;
                    }
                }

                // Download any offline data before showing the sample.
                if (item.OfflineDataItems != null)
                {
                    // Show the waiting dialog.
                    var builder = new AlertDialog.Builder(this);
                    builder.SetView(new ProgressBar(this)
                    {
                        Indeterminate = true
                    });
                    builder.SetMessage("Downloading data");
                    AlertDialog dialog = builder.Create();
                    dialog.Show();

                    try
                    {
                        // Begin downloading data.
                        await DataManager.EnsureSampleDataPresent(item);
                    }
                    finally
                    {
                        // Hide the progress dialog.
                        dialog.Dismiss();
                    }
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

        #region Permissions

        // Permissions and permission request.
        private const int CameraPermissionRequestCode = 100;
        private TaskCompletionSource<bool> _cameraPermissionTCS;

        public async Task<bool> AskForCameraPermission()
        {
            if (ContextCompat.CheckSelfPermission(this, CameraService) != Permission.Granted)
            {
                _cameraPermissionTCS = new TaskCompletionSource<bool>();
                RequestPermissions(new[] { Manifest.Permission.Camera }, CameraPermissionRequestCode);
                return await _cameraPermissionTCS.Task;
            }
            return true;
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            if (requestCode == CameraPermissionRequestCode)
            {
                _cameraPermissionTCS.TrySetResult(grantResults.Length == 1 && grantResults[0] == Permission.Granted);
            }
        }

        #endregion Permissions

        private static void ClearCredentials()
        {
            // Clear credentials (if any) from previous sample runs.
            foreach (Credential cred in AuthenticationManager.Current.Credentials)
            {
                AuthenticationManager.Current.RemoveCredential(cred);
            }

            // Clear the challenge handler.
            AuthenticationManager.Current.ChallengeHandler = null;
        }
    }
}