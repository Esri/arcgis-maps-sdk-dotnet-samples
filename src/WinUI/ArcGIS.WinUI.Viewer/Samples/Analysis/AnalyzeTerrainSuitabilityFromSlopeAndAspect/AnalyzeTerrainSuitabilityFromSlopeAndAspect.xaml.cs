// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Analysis;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.GeoAnalysis;
using Microsoft.UI.Xaml;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.AnalyzeTerrainSuitabilityFromSlopeAndAspect
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Analyze terrain suitability from slope and aspect",
        category: "Analysis",
        description: "Analyze terrain suitability from an elevation raster by deriving slope and aspect.",
        instructions: "When the sample opens, the map shows the results of a preconfigured terrain suitability analysis which finds southward facing lowland slopes on the Isle of Arran, Scotland. The areas matching the criteria are rendered in green, and those not, in white. Use the radio buttons to choose another preconfigured scenario, that of a west to north facing slope in upland terrains. Areas matching this criteria are rendered in purple.",
        tags: new[] { "aspect", "elevation", "field analysis", "map algebra", "raster", "slope", "spatial reference", "terrain" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("aa97788593e34a32bcaae33947fdc271")]
    public partial class AnalyzeTerrainSuitabilityFromSlopeAndAspect
    {
        // The slope and aspect operations require a conformal projection, so the raster content and the map both use
        // UTM zone 30N. The analysis is not rendered if the field and the map view spatial references do not match.
        private static readonly SpatialReference Utm30N = SpatialReference.Create(32630);

        private ContinuousField _elevationField;
        private ContinuousFieldFunction _elevationFieldFunction;
        private ContinuousFieldFunction _slopeFunction;
        private ContinuousFieldFunction _aspectFunction;
        private BooleanFieldFunction _aboveSeaLevelSelection;

        private FieldAnalysis _lowlandSouthFacingSlopesAnalysis;
        private FieldAnalysis _uplandWestToNorthFacingSlopesAnalysis;

        private bool _analysisErrorReported;

        public AnalyzeTerrainSuitabilityFromSlopeAndAspect()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a blank map with the UTM zone 30N spatial reference. The basemap styles are published in Web Mercator,
            // which the slope and aspect operations do not support, so no basemap is set.
            MyMapView.Map = new Map(Utm30N);

            try
            {
                // Get the path to the locally stored elevation raster file.
                string rasterPath = DataManager.GetDataFolder("aa97788593e34a32bcaae33947fdc271", "arran.tif");

                // Create a continuous field from the elevation raster file, projecting its content into UTM zone 30N.
                _elevationField = await ContinuousField.CreateAsync(new[] { rasterPath }, 0, Utm30N);

                // Create a continuous field function from the elevation field.
                _elevationFieldFunction = ContinuousFieldFunction.Create(_elevationField);

                // Derive the slope and the aspect of the terrain from the elevation field function.
                _slopeFunction = _elevationFieldFunction.Slope();
                _aspectFunction = _elevationFieldFunction.Aspect();

                // Create a boolean field function that is true for the land above sea level, used to exclude the sea.
                _aboveSeaLevelSelection = _elevationFieldFunction.IsGreaterThanOrEqualTo(0);

                // Create an analysis overlay to display the scenario analyses and add it to the map view.
                var analysisOverlay = new AnalysisOverlay();
                MyMapView.AnalysisOverlays.Add(analysisOverlay);

                // Find south-facing slopes on flat to moderately sloped lowland terrain.
                _lowlandSouthFacingSlopesAnalysis = CreateScenarioAnalysis(
                    slopeMin: 0, // Flat terrain.
                    slopeMax: 20, // Moderate slopes.
                    aspectStart: 112.5f, // East-south-east.
                    aspectEnd: 247.5f, // West-south-west.
                    elevationMin: 0,
                    elevationMax: 300,
                    color: Color.FromArgb(76, 175, 80)); // Green.

                // Find westward through northward facing slopes on moderate to very steep terrain at higher elevations.
                _uplandWestToNorthFacingSlopesAnalysis = CreateScenarioAnalysis(
                    slopeMin: 20, // Moderate slopes.
                    slopeMax: 80, // Very steep slopes.
                    aspectStart: 202.5f, // South-south-west.
                    aspectEnd: 67.5f, // East-north-east.
                    elevationMin: 300,
                    elevationMax: 850,
                    color: Color.FromArgb(156, 39, 176)); // Purple.

                analysisOverlay.Analyses.Add(_lowlandSouthFacingSlopesAnalysis);
                analysisOverlay.Analyses.Add(_uplandWestToNorthFacingSlopesAnalysis);

                // Report the computation progress of the analysis that is currently selected.
                MyMapView.AnalysisViewStateChanged += OnAnalysisViewStateChanged;

                // Show the scenario that is selected when the sample opens.
                UpdateVisibleScenarioAnalysis();

                // Zoom to the extent of the elevation data.
                await MyMapView.SetViewpointCenterAsync(_elevationField.Extent.GetCenter(), 200000);
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.Message, "Error").ShowAsync();
            }
        }

        // Create a field analysis that highlights the terrain matching the slope, aspect, and elevation ranges of a scenario.
        private FieldAnalysis CreateScenarioAnalysis(float slopeMin, float slopeMax, float aspectStart, float aspectEnd, float elevationMin, float elevationMax, Color color)
        {
            BooleanFieldFunction scenarioFieldFunction = CreateScenarioFieldFunction(slopeMin, slopeMax, aspectStart, aspectEnd, elevationMin, elevationMax);

            // Draw the terrain that does not match the scenario (false) in white and the terrain that matches it (true) in the scenario color.
            Colormap colormap = Colormap.Create(new[] { Color.White, color });

            // The field analysis evaluates the function and keeps the display up to date, so there is no need to call EvaluateAsync.
            return new FieldAnalysis(scenarioFieldFunction, new ColormapRenderer(colormap)) { IsVisible = false };
        }

        // Create the boolean field function that is true for the terrain matching a scenario's slope, aspect, and elevation ranges.
        private BooleanFieldFunction CreateScenarioFieldFunction(float slopeMin, float slopeMax, float aspectStart, float aspectEnd, float elevationMin, float elevationMax)
        {
            // Keep the terrain that falls within the scenario's slope range.
            BooleanFieldFunction slopeRangeMask = _slopeFunction.IsGreaterThanOrEqualTo(slopeMin)
                .LogicalAnd(_slopeFunction.IsLessThanOrEqualTo(slopeMax));

            // Operator overloads (>=, <=, &, |) can be used in place of IsGreaterThanOrEqualTo, IsLessThanOrEqualTo, LogicalAnd, and LogicalOr.
            // An aspect range that crosses due north, such as 202.5 to 67.5 degrees, is expressed as two ranges either side of 0 degrees.
            BooleanFieldFunction aspectRangeMask = aspectStart <= aspectEnd
                ? (_aspectFunction >= aspectStart) & (_aspectFunction <= aspectEnd)
                : ((_aspectFunction >= aspectStart) & (_aspectFunction < 360)) | ((_aspectFunction >= 0) & (_aspectFunction <= aspectEnd));

            // Keep the terrain that falls within the scenario's elevation range.
            BooleanFieldFunction elevationRangeMask = (_elevationFieldFunction >= elevationMin) & (_elevationFieldFunction <= elevationMax);

            // Combine the slope, aspect, and elevation masks, then mask out everything below sea level.
            return slopeRangeMask
                .LogicalAnd(aspectRangeMask)
                .LogicalAnd(elevationRangeMask)
                .Mask(_aboveSeaLevelSelection);
        }

        // Get the field analysis for the scenario that is currently selected.
        private FieldAnalysis SelectedScenarioAnalysis => LowlandSouthFacingSlopesRadioButton.IsChecked == true
            ? _lowlandSouthFacingSlopesAnalysis
            : _uplandWestToNorthFacingSlopesAnalysis;

        private void OnScenarioSelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateVisibleScenarioAnalysis();
        }

        private void UpdateVisibleScenarioAnalysis()
        {
            // The radio buttons raise their Checked event while the sample is loading, before the analyses are created.
            if (_lowlandSouthFacingSlopesAnalysis == null) return;

            // Show only the analysis for the selected scenario.
            bool showLowlandSouthFacingSlopes = LowlandSouthFacingSlopesRadioButton.IsChecked == true;
            _lowlandSouthFacingSlopesAnalysis.IsVisible = showLowlandSouthFacingSlopes;
            _uplandWestToNorthFacingSlopesAnalysis.IsVisible = !showLowlandSouthFacingSlopes;
        }

        private async void OnAnalysisViewStateChanged(object sender, AnalysisViewStateChangedEventArgs e)
        {
            // Ignore state changes reported for the analysis that is not currently displayed.
            if (e.Analysis != SelectedScenarioAnalysis) return;

            // Show a progress bar while the analysis is being computed.
            AnalysisProgressBar.Visibility = e.AnalysisViewState.Status == AnalysisViewStatus.Updating ? Visibility.Visible : Visibility.Collapsed;

            // Report the first error encountered while displaying an analysis.
            if (e.AnalysisViewState.Status == AnalysisViewStatus.Error && !_analysisErrorReported)
            {
                _analysisErrorReported = true;
                await new MessageDialog2(e.AnalysisViewState.Error?.Message, "Error displaying analysis").ShowAsync();
            }
        }
    }
}
