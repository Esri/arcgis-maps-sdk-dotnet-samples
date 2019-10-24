// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V4.Content;
using Esri.ArcGISRuntime.ARToolkit.Forms;
using System;
using System.Threading.Tasks;

namespace ArcGISRuntime.Droid
{
    [Activity(Label = "ArcGIS Runtime SDK for .NET", Icon = "@drawable/icon", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsApplicationActivity
    {
        internal static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Instance = this;

            // Copy files from the asset folder onto the filesystem to support browsing of sample code and readmes.
            SyncAssets("Samples", System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData));

            Xamarin.Essentials.Platform.Init(this, bundle);
            Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        #region Permissions

        private const int LocationPermissionRequestCode = 99;
        private Esri.ArcGISRuntime.Xamarin.Forms.MapView _lastUsedMapView;
        private const int CameraPermissionRequestCode = 98;
        private ARSceneView _lastARView;

        public async void AskForLocationPermission(Esri.ArcGISRuntime.Xamarin.Forms.MapView myMapView)
        {
            // Save the mapview for later.
            _lastUsedMapView = myMapView;

            // Only check if permission hasn't been granted yet.
            if (ContextCompat.CheckSelfPermission(this, LocationService) != Permission.Granted)
            {
                // Show the standard permission dialog.
                // Once the user has accepted or denied, OnRequestPermissionsResult is called with the result.
                RequestPermissions(new[] { Manifest.Permission.AccessFineLocation }, LocationPermissionRequestCode);
            }
            else
            {
                try
                {
                    // Explicit DataSource.LoadAsync call is used to surface any errors that may arise.
                    await myMapView.LocationDisplay.DataSource.StartAsync();
                    myMapView.LocationDisplay.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    ShowMessage(ex.Message, "Failed to start location display.");
                }
            }
        }

        public async void AskForCameraPermission(ARSceneView arView)
        {
            // Save the mapview for later.
            _lastARView = arView;

            // Only check if permission hasn't been granted yet.
            if (ContextCompat.CheckSelfPermission(this, CameraService) != Permission.Granted)
            {
                // Show the standard permission dialog.
                // Once the user has accepted or denied, OnRequestPermissionsResult is called with the result.
                RequestPermissions(new[] { Manifest.Permission.Camera }, CameraPermissionRequestCode);

            }
            else
            {
                try
                {
                    await arView.StartTrackingAsync();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                    ShowMessage(ex.Message, "Failed to start camera.");
                }
            }
        }

        public override async void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            // Ignore other location requests.
            if (requestCode == LocationPermissionRequestCode)
            {
                // If the permissions were granted, enable location.
                if (grantResults.Length == 1 && grantResults[0] == Permission.Granted && _lastUsedMapView != null)
                {
                    System.Diagnostics.Debug.WriteLine("User affirmatively gave permission to use location. Enabling location.");
                    try
                    {
                        // Explicit DataSource.LoadAsync call is used to surface any errors that may arise.
                        await _lastUsedMapView.LocationDisplay.DataSource.StartAsync();
                        _lastUsedMapView.LocationDisplay.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                        ShowMessage(ex.Message, "Failed to start location display.");
                    }
                }
                else
                {
                    ShowMessage("Location permissions not granted.", "Failed to start location display.");
                }

                // Reset the mapview.
                _lastUsedMapView = null;
            }
            else if(requestCode == CameraPermissionRequestCode)
            {
                if (grantResults.Length == 1 && grantResults[0] == Permission.Granted && _lastARView != null)
                {
                    System.Diagnostics.Debug.WriteLine("User affirmatively gave permission to use camera. Enabling camera.");
                    try
                    {
                       await _lastARView.StartTrackingAsync();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                        ShowMessage(ex.Message, "Failed to start camera.");
                    }
                }
                else
                {
                    ShowMessage("Location permissions not granted.", "Failed to start location display.");
                }

                // Reset the mapview.
                _lastUsedMapView = null;
            }

        }

        private void ShowMessage(string message, string title = "Error") => new AlertDialog.Builder(this).SetTitle(title).SetMessage(message).Show();

        #endregion

        public static void SyncAssets(string assetFolder, string targetDir)
        {
            string[] assets = Application.Context.Assets.List(assetFolder);

            foreach (string asset in assets)
            {
                string combinedPath = System.IO.Path.Combine(assetFolder, asset);
                string[] subAssets = Application.Context.Assets.List(combinedPath);

                // Recur on folders.
                if (subAssets.Length > 0)
                {
                    SyncAssets(combinedPath, targetDir);
                }
                else
                {
                    // Only readmes need to be copied for now.
                    if (!combinedPath.EndsWith(".md")) { continue; }

                    // Copy the file.
                    using (var source = Application.Context.Assets.Open(combinedPath))
                    {
                        string combinedTargetDirPath = System.IO.Path.Combine(targetDir, assetFolder);
                        if (!System.IO.Directory.Exists(combinedTargetDirPath))
                        {
                            System.IO.Directory.CreateDirectory(combinedTargetDirPath);
                        }

                        using (var dest = System.IO.File.Create(System.IO.Path.Combine(combinedTargetDirPath, asset)))
                        {
                            source.CopyTo(dest);
                        }
                    }
                }
            }
        }
    }
}