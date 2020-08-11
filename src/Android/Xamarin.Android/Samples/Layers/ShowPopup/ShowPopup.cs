// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Popups;
using Esri.ArcGISRuntime.Toolkit.UI.Controls;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.ShowPopup
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Show popup",
        category: "Layers",
        description: "Show predefined popups from a web map.",
        instructions: "Tap on the features to prompt a popup that displays information about the feature.",
        tags: new[] { "feature", "feature layer", "popup", "toolkit", "web map" })]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("ShowPopup.xml")]
    public class ShowPopup : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private PopupViewer _popupViewer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Show popup";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Load the map.
            _myMapView.Map = new Map(new Uri("https://runtime.maps.arcgis.com/home/webmap/viewer.html?webmap=e4c6eb667e6c43b896691f10cc2f1580"));
        }

        private async void MapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the feature layer from the map.
                FeatureLayer incidentLayer = _myMapView.Map.OperationalLayers.First() as FeatureLayer;

                // Identify the tapped on feature.
                IdentifyLayerResult result = await _myMapView.IdentifyLayerAsync(incidentLayer, e.Position, 12, true);

                if (result != null && result.Popups.Any())
                {
                    // Get the first popup from the identify result.
                    Popup popup = result.Popups.First();

                    // Create a new popup manager for the popup.
                    _popupViewer.PopupManager = new PopupManager(popup);

                    QueryParameters queryParams = new QueryParameters
                    {
                        // Set the geometry to selection envelope for selection by geometry.
                        Geometry = new Envelope((MapPoint)popup.GeoElement.Geometry, 6, 6)
                    };

                    // Select the features based on query parameters defined above.
                    await incidentLayer.SelectFeaturesAsync(queryParams, SelectionMode.New);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Print(ex.Message);
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            SetContentView(Resource.Layout.ShowPopup);

            _myMapView = FindViewById<MapView>(Resource.Id.MapView);
            _popupViewer = FindViewById<PopupViewer>(Resource.Id.popupViewer);

            // Add event handlers.
            _myMapView.GeoViewTapped += MapViewTapped;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unhook event handlers.
            _myMapView.GeoViewTapped -= MapViewTapped;
        }
    }
}