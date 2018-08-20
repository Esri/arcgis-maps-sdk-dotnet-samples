// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.Content.PM;
using Android.OS;

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

            Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

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