// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.FeatureLayerGeodatabase
{
    [Register("FeatureLayerGeodatabase")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("2b0f9e17105847809dfeb04e3cad69e0")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer (geodatabase)",
        "Data",
        "This sample demonstrates how to consume an Esri .geodatabase file (aka. mobile geodatabase) by using a FeatureLayer and a GeodatabaseFeatureTable.",
        "The mobile geodatabase (.geodatabase file) will be downloaded from an ArcGIS Online portal automatically from the Url: https://www.arcgis.com/home/item.html?id=2b0f9e17105847809dfeb04e3cad69e0.",
        "")]
    public class FeatureLayerGeodatabase : UIViewController
    {
        // Create and hold a reference to the MapView.
        private readonly MapView _myMapView = new MapView();

        public FeatureLayerGeodatabase()
        {
            Title = "Feature layer (geodatabase)";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;

                // Reposition controls.
                _myMapView.Frame = new CoreGraphics.CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Create a new map to display in the map view with a streets basemap.
            _myMapView.Map = new Map(Basemap.CreateStreets());

            // Get the path to the downloaded mobile geodatabase (.geodatabase file).
            string mobileGeodatabaseFilePath = DataManager.GetDataFolder("2b0f9e17105847809dfeb04e3cad69e0", "LA_Trails.geodatabase");

            // Open the mobile geodatabase.
            Geodatabase mobileGeodatabase = await Geodatabase.OpenAsync(mobileGeodatabaseFilePath);

            // Get the 'Trailheads' geodatabase feature table from the mobile geodatabase.
            GeodatabaseFeatureTable trailheadsGeodatabaseFeatureTable = mobileGeodatabase.GeodatabaseFeatureTable("Trailheads");

            // Asynchronously load the 'Trailheads' geodatabase feature table.
            await trailheadsGeodatabaseFeatureTable.LoadAsync();

            // Create a feature layer based on the geodatabase feature table.
            FeatureLayer trailheadsFeatureLayer = new FeatureLayer(trailheadsGeodatabaseFeatureTable);

            // Add the feature layer to the operations layers collection of the map.
            _myMapView.Map.OperationalLayers.Add(trailheadsFeatureLayer);

            // Zoom the map to the extent of the feature layer.
            await _myMapView.SetViewpointGeometryAsync(trailheadsFeatureLayer.FullExtent, 50);
        }

        private void CreateLayout()
        {
            // Add MapView to the page.
            View.AddSubview(_myMapView);
        }
    }
}