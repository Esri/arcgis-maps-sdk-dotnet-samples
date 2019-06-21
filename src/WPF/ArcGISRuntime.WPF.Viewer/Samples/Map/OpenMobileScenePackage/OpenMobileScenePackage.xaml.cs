﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.OpenMobileScenePackage
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Open mobile scene package",
        "Map",
        "Display a scene from an offline mobile scene package (.mspk).",
        "", "Featured")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7dd2f97bb007466ea939160d0de96a9d")]
    public partial class OpenMobileScenePackage
    {
        public OpenMobileScenePackage()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Get the path to the scene package.
            string scenePath = DataManager.GetDataFolder("7dd2f97bb007466ea939160d0de96a9d", "philadelphia.mspk");

            try
            {
                // Determine if the package can be read directly or needs to be unpacked.
                if (await MobileScenePackage.IsDirectReadSupportedAsync(scenePath))
                {
                    // Open the package.
                    MobileScenePackage package = await MobileScenePackage.OpenAsync(scenePath);

                    // Load the package.
                    await package.LoadAsync();

                    // Show the first scene.
                    MySceneView.Scene = package.Scenes.First();
                }
                else
                {
                    // Create a path for the unpacked package.
                    string unpackedPath = scenePath + "unpacked";

                    // Unpack the package.
                    await MobileScenePackage.UnpackAsync(scenePath, unpackedPath);

                    // Open the package.
                    MobileScenePackage package = await MobileScenePackage.OpenAsync(unpackedPath);

                    // Load the package.
                    await package.LoadAsync();

                    // Show the first scene.
                    MySceneView.Scene = package.Scenes.First();
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Couldn't open scene");
                Debug.WriteLine(e);
            }
        }
    }
}