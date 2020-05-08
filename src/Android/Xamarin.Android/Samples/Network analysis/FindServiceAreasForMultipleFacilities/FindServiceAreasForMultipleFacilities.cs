// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.FindServiceAreasForMultipleFacilities
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Find service areas for multiple facilities",
        category: "Network analysis",
        description: "Find the service areas of several facilities from a feature service.",
        instructions: "Tap 'find service area' to calculate and display the service area of each facility on the map. The polygons displayed around each facility represents the service area; in red is the area that is within 3 minutes away from the hospital by car. Light orange is the area that is within 5 minutes away from the hospital by car.",
        tags: new[] { "facilities", "feature service", "impedance", "network analysis", "service area", "travel time" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class FindServiceAreasForMultipleFacilities : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private ProgressBar _activityIndicator;
        private Button _findServiceAreasButton;

        // URLs to resources used by the sample.
        private const string NetworkAnalysisUrl = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/ServiceArea";
        private const string FacilitiesFeatureUrl = "https://services2.arcgis.com/ZQgQTuoyBrtmoGdP/ArcGIS/rest/services/San_Diego_Facilities/FeatureServer/0";
        private const string IconUrl = "https://static.arcgis.com/images/Symbols/SafetyHealth/Hospital.png";

        // The table that contains the facilities.
        private ServiceFeatureTable _facilitiesTable;

        // The task for performing the service area analysis.
        private ServiceAreaTask _serviceAreaTask;

        // The graphics overlay for displaying the resulting polygons.
        private GraphicsOverlay _resultOverlay;

        // Symbology for the resulting service areas.
        private List<SimpleFillSymbol> _fillSymbols;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Find service areas for multiple facilities";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Create the map and show it in the view.
                Map newMap = new Map(Basemap.CreateLightGrayCanvas());
                _myMapView.Map = newMap;

                // Create the table containing the facilities.
                _facilitiesTable = new ServiceFeatureTable(new Uri(FacilitiesFeatureUrl));

                // Create the layer for rendering the facilities table.
                FeatureLayer facilitiesLayer = new FeatureLayer(_facilitiesTable);

                // Create a simple renderer that will display an image for each facility.
                facilitiesLayer.Renderer = new SimpleRenderer(new PictureMarkerSymbol(new Uri(IconUrl)));

                // Add the layer to the map.
                newMap.OperationalLayers.Add(facilitiesLayer);

                // Create the graphics overlay for displaying the result polygons.
                _resultOverlay = new GraphicsOverlay();

                // Add the result overlay to the view.
                _myMapView.GraphicsOverlays.Add(_resultOverlay);

                // Create a list of fill symbols for rendering the results.
                _fillSymbols = new List<SimpleFillSymbol>();
                _fillSymbols.Add(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(0x66, 0xFF, 0xA5, 0x00), null));
                _fillSymbols.Add(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, System.Drawing.Color.FromArgb(0x66, 0xFF, 0x00, 0x00), null));

                // Wait for the table to load and zoom to its extent.
                await _facilitiesTable.LoadAsync();
                await _myMapView.SetViewpointGeometryAsync(_facilitiesTable.Extent, 50);

                // Enable the button now that the sample is ready.
                _findServiceAreasButton.Enabled = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ShowMessage("Error", "Error starting the sample.");
            }
        }

        private async Task FindServiceAreas()
        {
            try
            {
                // Create the service area task.
                _serviceAreaTask = await ServiceAreaTask.CreateAsync(new Uri(NetworkAnalysisUrl));

                // Create the default parameters for the service.
                ServiceAreaParameters serviceAreaParameters = await _serviceAreaTask.CreateDefaultParametersAsync();

                // Configure the service area parameters.
                serviceAreaParameters.PolygonDetail = ServiceAreaPolygonDetail.High;
                serviceAreaParameters.ReturnPolygons = true;
                serviceAreaParameters.DefaultImpedanceCutoffs.Clear();
                serviceAreaParameters.DefaultImpedanceCutoffs.Add(0);
                serviceAreaParameters.DefaultImpedanceCutoffs.Add(3);
                serviceAreaParameters.DefaultImpedanceCutoffs.Add(5);

                // A query that finds all of the relevant facilities from the facilities feature service.
                QueryParameters facilityQueryParameters = new QueryParameters();
                facilityQueryParameters.WhereClause = "1=1";

                // Provide the feature service and the query as parameters to the service area task.
                serviceAreaParameters.SetFacilities(_facilitiesTable, facilityQueryParameters);

                // Perform the service area analysis.
                ServiceAreaResult result = await _serviceAreaTask.SolveServiceAreaAsync(serviceAreaParameters);

                // Count the features in the facilities layer. 
                long facilityCount = await _facilitiesTable.QueryFeatureCountAsync(facilityQueryParameters);

                // Get the service area for each facility.
                for (int facilityIndex = 0; facilityIndex < facilityCount; facilityIndex++)
                {
                    // Get each area polygon from the result for that facility.
                    List<ServiceAreaPolygon> areaPolygons = result.GetResultPolygons(facilityIndex).ToList();

                    // Add each service area polygon to the graphics overlay.
                    for (int polygonIndex = 0; polygonIndex < areaPolygons.Count; polygonIndex++)
                    {
                        // Get the polygon from the result.
                        Polygon resultingPolygon = areaPolygons[polygonIndex].Geometry;

                        // Choose a symbol for the polygon.
                        SimpleFillSymbol selectedSymbol = _fillSymbols[polygonIndex % _fillSymbols.Count];

                        // Create and add the graphic.
                        _resultOverlay.Graphics.Add(new Graphic(resultingPolygon, selectedSymbol));
                    }
                }

                // Zoom to the extent of the results.
                await _myMapView.SetViewpointGeometryAsync(_resultOverlay.Extent, 50);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
                ShowMessage("Error", "Couldn't complete service area analysis.");
            }
        }

        private async void FindServiceArea_Clicked(object sender, EventArgs e)
        {
            try
            {
                // Clear the old results.
                _resultOverlay?.Graphics.Clear();

                // Update the UI.
                _findServiceAreasButton.Enabled = false;
                _activityIndicator.Visibility = ViewStates.Visible;

                // Do the service area work.
                await FindServiceAreas();

                // Update the UI.
                _activityIndicator.Visibility = ViewStates.Gone;
                _findServiceAreasButton.Enabled = true;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(exception);
                ShowMessage("Error", "Couldn't complete the service area analysis.");
            }
        }

        private void ShowMessage(string title, string detail) => new AlertDialog.Builder(this).SetTitle(title).SetMessage(detail).Show();

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Create the activity spinner.
            _activityIndicator = new ProgressBar(this);
            _activityIndicator.Indeterminate = true;
            _activityIndicator.Visibility = ViewStates.Gone;

            // Create the service areas button.
            _findServiceAreasButton = new Button(this);
            _findServiceAreasButton.Text = "Find service areas";
            _findServiceAreasButton.Enabled = false;
            _findServiceAreasButton.Click += FindServiceArea_Clicked;

            // Add the button and the spinner.
            layout.AddView(_findServiceAreasButton);
            layout.AddView(_activityIndicator);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}