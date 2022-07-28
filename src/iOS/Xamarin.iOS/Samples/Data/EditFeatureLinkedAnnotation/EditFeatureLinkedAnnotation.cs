// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.EditFeatureLinkedAnnotation
{
    [Register("EditFeatureLinkedAnnotation")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Edit features with feature-linked annotation",
        category: "Data",
        description: "Edit feature attributes which are linked to annotation through an expression.",
        instructions: "Pan and zoom the map to see that the text on the map is annotation, not labels. Tap one of the address points to update the house number (AD_ADDRESS) and street name (ST_STR_NAM). Tap one of the dashed parcel polylines and tap another location to change its geometry. NOTE: Selection is only enabled for points and straight (single segment) polylines.",
        tags: new[] { "annotation", "attributes", "feature-linked annotation", "features", "fields" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("74c0c9fa80f4498c9739cc42531e9948")]
    public class EditFeatureLinkedAnnotation : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIAlertController alertController;

        private Feature _selectedFeature;

        public EditFeatureLinkedAnnotation()
        {
            Title = "Edit features with feature-linked annotation";
        }

        private async void Initialize()
        {
            // NOTE: to be a writable geodatabase, this geodatabase must be generated from a service with a GeodatabaseSyncTask. See the "Generate geodatabase" sample.
            try
            {
                // Create and load geodatabase.
                string geodatabasePath = DataManager.GetDataFolder("74c0c9fa80f4498c9739cc42531e9948", "loudoun_anno.geodatabase");
                Geodatabase geodatabase = await Geodatabase.OpenAsync(geodatabasePath);

                // Create feature layers from tables in the geodatabase.
                FeatureLayer addressFeatureLayer = new FeatureLayer(geodatabase.GetGeodatabaseFeatureTable("Loudoun_Address_Points_1"));
                FeatureLayer parcelFeatureLayer = new FeatureLayer(geodatabase.GetGeodatabaseFeatureTable("ParcelLines_1"));

                // Create annotation layers from tables in the geodatabase.
                AnnotationLayer addressAnnotationLayer = new AnnotationLayer(geodatabase.GetGeodatabaseAnnotationTable("Loudoun_Address_PointsAnno_1"));
                AnnotationLayer parcelAnnotationLayer = new AnnotationLayer(geodatabase.GetGeodatabaseAnnotationTable("ParcelLinesAnno_1"));

                // Create a map with the layers.
                _myMapView.Map = new Map(BasemapStyle.ArcGISLightGray);
                _myMapView.Map.OperationalLayers.AddRange(new List<Layer> { addressFeatureLayer, parcelFeatureLayer, addressAnnotationLayer, parcelAnnotationLayer });

                // Zoom to the extent of the parcels.
                await parcelFeatureLayer.LoadAsync();
                _myMapView.SetViewpoint(new Viewpoint(parcelFeatureLayer.FullExtent));
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "Ok", null).Show();
            }
        }

        private void MapView_Tapped(object sender, GeoViewInputEventArgs e)
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

        private async Task SelectFeature(CGPoint tappedPoint)
        {
            try
            {
                // Identify across all layers.
                IReadOnlyList<IdentifyLayerResult> identifyResults = await _myMapView.IdentifyLayersAsync(tappedPoint, 10.0, false);
                foreach (IdentifyLayerResult result in identifyResults)
                {
                    if (result.LayerContent is FeatureLayer layer)
                    {
                        _selectedFeature = result.GeoElements.First() as Feature;
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
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "Ok", null).Show();
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
            // Create the alert controller.
            alertController = UIAlertController.Create(null, "Edit feature attributes", UIAlertControllerStyle.Alert);
            alertController.AddTextField(new Action<UITextField>(SetAddressText));
            alertController.AddTextField(new Action<UITextField>(SetStreetText));
            alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, OkClick));
            alertController.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Show the alert.
            PresentViewController(alertController, true, null);
        }

        private void SetAddressText(UITextField field)
        {
            field.Text = _selectedFeature.Attributes["AD_ADDRESS"].ToString();
        }

        private void SetStreetText(UITextField field)
        {
            field.Text = _selectedFeature.Attributes["ST_STR_NAM"].ToString();
        }

        private async void OkClick(UIAlertAction action)
        {
            try
            {
                // Update the feature attributes with the user input.
                _selectedFeature.Attributes["AD_ADDRESS"] = int.Parse(alertController.TextFields[0].Text);
                _selectedFeature.Attributes["ST_STR_NAM"] = alertController.TextFields[1].Text;
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "Ok", null).Show();
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView() { TranslatesAutoresizingMaskIntoConstraints = false };

            UILabel helpLabel = new UILabel
            {
                Text = "1. Tap to select a feature.\n2. For MapPoint features, edit the feature attributes.\n3. Tap again to move the feature.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Left,
                BackgroundColor = ApplicationTheme.BackgroundColor,
                TextColor = ApplicationTheme.ForegroundColor,
                Lines = 3,
                TranslatesAutoresizingMaskIntoConstraints = false,
            };

            // Add the views.
            View.AddSubviews(_myMapView, helpLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                helpLabel.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                helpLabel.HeightAnchor.ConstraintEqualTo(80),

                _myMapView.TopAnchor.ConstraintEqualTo(helpLabel.BottomAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MapView_Tapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MapView_Tapped;
        }
    }
}