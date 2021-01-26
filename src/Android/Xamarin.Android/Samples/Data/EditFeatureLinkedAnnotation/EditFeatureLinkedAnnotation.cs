// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Widget;
using ArcGISRuntime;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.EditFeatureLinkedAnnotation
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Edit features with feature-linked annotation",
        category: "Data",
        description: "Edit feature attributes which are linked to annotation through an expression.",
        instructions: "Pan and zoom the map to see that the text on the map is annotation, not labels. Tap one of the address points to update the house number (AD_ADDRESS) and street name (ST_STR_NAM). Tap one of the dashed parcel polylines and tap another location to change its geometry. NOTE: Selection is only enabled for points and straight (single segment) polylines.",
        tags: new[] { "annotation", "attributes", "feature-linked annotation", "features", "fields" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("74c0c9fa80f4498c9739cc42531e9948")]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("EditAttributeLayout.xml")]
    public class EditFeatureLinkedAnnotation : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;

        private Feature _selectedFeature;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Edit features with feature-linked annotation";

            CreateLayout();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // NOTE: to be a writable geodatabase, this geodatabase must be generated from a service with a GeodatabaseSyncTask. See the "Generate geodatabase" sample.
            try
            {
                // Create and load geodatabase.
                string geodatabasePath = DataManager.GetDataFolder("74c0c9fa80f4498c9739cc42531e9948", "loudoun_anno.geodatabase");
                Geodatabase geodatabase = await Geodatabase.OpenAsync(geodatabasePath);

                // Create feature layers from tables in the geodatabase.
                FeatureLayer addressFeatureLayer = new FeatureLayer(geodatabase.GeodatabaseFeatureTable("Loudoun_Address_Points_1"));
                FeatureLayer parcelFeatureLayer = new FeatureLayer(geodatabase.GeodatabaseFeatureTable("ParcelLines_1"));

                // Create annotation layers from tables in the geodatabase.
                AnnotationLayer addressAnnotationLayer = new AnnotationLayer(geodatabase.GeodatabaseAnnotationTable("Loudoun_Address_PointsAnno_1"));
                AnnotationLayer parcelAnnotationLayer = new AnnotationLayer(geodatabase.GeodatabaseAnnotationTable("ParcelLinesAnno_1"));

                // Create a map with the layers.
                _myMapView.Map = new Map(BasemapStyle.ArcGISLightGray);
                _myMapView.Map.OperationalLayers.AddRange(new List<Layer> { addressFeatureLayer, parcelFeatureLayer, addressAnnotationLayer, parcelAnnotationLayer });

                // Zoom to the extent of the parcels.
                await parcelFeatureLayer.LoadAsync();
                _myMapView.SetViewpoint(new Viewpoint(parcelFeatureLayer.FullExtent));
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
        }

        private void MapViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Check if there is already a selected feature.
            if (_selectedFeature == null)
            {
                _ = SelectFeature(e.Position);
            }
            else
            {
                // Project the user selected point.
                MapPoint projPoint = GeometryEngine.Project(e.Location, _selectedFeature.Geometry.SpatialReference) as MapPoint;

                // Update the geometry of the selected feature.
                _ = UpdateGeometry(projPoint);
            }
        }

        private async Task SelectFeature(PointF clickedPoint)
        {
            try
            {
                // Identify across all layers.
                IReadOnlyList<IdentifyLayerResult> identifyResults = await _myMapView.IdentifyLayersAsync(clickedPoint, 10.0, false);
                foreach (IdentifyLayerResult result in identifyResults)
                {
                    if (result.LayerContent is FeatureLayer layer)
                    {
                        _selectedFeature = result.GeoElements.FirstOrDefault() as Feature;
                        if (_selectedFeature.Geometry is Polyline line)
                        {
                            // No support for curved lines.
                            if (line.Parts.Count > 1)
                            {
                                _selectedFeature = null;
                                return;
                            }
                        }
                        else if (_selectedFeature.Geometry is MapPoint)
                        {
                            // Open attribute editor for point features.
                            ShowEditableAttributes();
                        }

                        // Select the feature.
                        layer.SelectFeature(_selectedFeature);
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
            }
        }

        private async Task UpdateGeometry(MapPoint point)
        {
            if (_selectedFeature.Geometry is Polyline line)
            {
                // Get the nearest point on the selected line.
                ProximityResult nearestVertex = GeometryEngine.NearestVertex(line, point);

                // Create a new polyline.
                PolylineBuilder polylineBuilder = new PolylineBuilder(line);
                Part part = polylineBuilder.Parts[nearestVertex.PartIndex];

                // Replace the nearest point with the new point.
                part.SetPoint(nearestVertex.PointIndex, point);

                // Update the geometry of the feature.
                _selectedFeature.Geometry = GeometryEngine.Project(polylineBuilder.ToGeometry(), _selectedFeature.Geometry.SpatialReference);
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);
            }
            else if (_selectedFeature.Geometry is MapPoint)
            {
                // Update the geometry of the feature.
                _selectedFeature.Geometry = point;
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);
            }

            // Clear the selection.
            (_selectedFeature.FeatureTable.Layer as FeatureLayer).ClearSelection();
            _selectedFeature = null;
        }

        private void ShowEditableAttributes()
        {
            // Create the view for the editing UI.
            Android.Views.View editAttributesView = LayoutInflater.Inflate(Resource.Layout.EditAttributeLayout, null);

            // Create an alert dialog.
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);
            dialogBuilder.SetTitle("Edit feature attributes");
            dialogBuilder.SetView(editAttributesView);
            EditText addressText = editAttributesView.FindViewById<EditText>(Resource.Id.addressNumberEditText);
            EditText streetText = editAttributesView.FindViewById<EditText>(Resource.Id.streetEditText);

            // Populate entry fields with data from feature.
            addressText.Text = _selectedFeature.Attributes["AD_ADDRESS"].ToString();
            streetText.Text = _selectedFeature.Attributes["ST_STR_NAM"].ToString();

            // Add buttons to finish the dialog
            dialogBuilder.SetNegativeButton("Cancel", (s, e) => { });
            dialogBuilder.SetPositiveButton("Ok", async (s, e) =>
            {
                try
                {
                    // Update the feature attributes with the user input.
                    _selectedFeature.Attributes["AD_ADDRESS"] = int.Parse(addressText.Text);
                    _selectedFeature.Attributes["ST_STR_NAM"] = streetText.Text;
                    await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);
                }
                catch (Exception ex)
                {
                    new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle(ex.GetType().Name).Show();
                }
            });

            dialogBuilder.Show();
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);

            // Add an event handler for the map view.
            _myMapView.GeoViewTapped += MapViewTapped;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Unhook the event handler.
            _myMapView.GeoViewTapped -= MapViewTapped;
        }
    }
}