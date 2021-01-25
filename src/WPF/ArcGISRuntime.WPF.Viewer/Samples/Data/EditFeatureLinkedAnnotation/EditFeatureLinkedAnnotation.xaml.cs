// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.EditFeatureLinkedAnnotation
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Edit features with feature-linked annotation",
        "Data",
        "Edit feature attributes which are linked to annotation through an expression.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("74c0c9fa80f4498c9739cc42531e9948")]
    public partial class EditFeatureLinkedAnnotation
    {
        private Feature _selectedFeature;
        private bool _selectedPolyline;

        public EditFeatureLinkedAnnotation()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // NOTE: to be a writable geodatabase, this geodatabase must be generated from a service with a GeodatabaseSyncTask. See the "generate geodatabase" sample.
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
                AnnotationLayer parceAnnotationLayer = new AnnotationLayer(geodatabase.GeodatabaseAnnotationTable("ParcelLinesAnno_1"));

                // Create a map with the layers.
                MyMapView.Map = new Map(BasemapStyle.ArcGISLightGray);
                MyMapView.Map.OperationalLayers.AddRange(new List<Layer> { addressFeatureLayer, parcelFeatureLayer, addressAnnotationLayer, parceAnnotationLayer });

                // Zoom to the extent of the parcels.
                await parcelFeatureLayer.LoadAsync();
                MyMapView.SetViewpoint(new Viewpoint(parcelFeatureLayer.FullExtent));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (_selectedFeature == null)
            {
                try
                {
                    // Select the features based on query parameters defined above.
                    var identifyResults = await MyMapView.IdentifyLayersAsync(e.Position, 10.0, false);
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
                            else if (_selectedFeature.Geometry is MapPoint point)
                            {
                                ShowEditableAttributes();
                            }
                            layer.SelectFeature(_selectedFeature);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error");
                }
            }
            else
            {
                var projPoint = GeometryEngine.Project(e.Location, _selectedFeature.Geometry.SpatialReference) as MapPoint;
                if (_selectedFeature.Geometry is Polyline line)
                {
                    
                    var nearestVertex = GeometryEngine.NearestVertex(line, projPoint);
                    var polylineBuilder = new PolylineBuilder(line);

                    var part = polylineBuilder.Parts[nearestVertex.PartIndex];
                    part.RemovePoint(nearestVertex.PointIndex);
                    part.AddPoint(projPoint);

                    _selectedFeature.Geometry = GeometryEngine.Project(polylineBuilder.ToGeometry(), _selectedFeature.Geometry.SpatialReference);
                    await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);

                }
                else if (_selectedFeature.Geometry is MapPoint point)
                {
                    // Change the geometry of the feature.
                    _selectedFeature.Geometry = projPoint;
                    await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);
                }
                // Clear the selection.
                (_selectedFeature.FeatureTable.Layer as FeatureLayer).ClearSelection();
                _selectedFeature = null;
            }
        }

        private void ShowEditableAttributes()
        {
            // Show entry UI.
            MyMapView.IsEnabled = false;
            AttributesBorder.Visibility = Visibility.Visible;

            // Populate entry fields with data from feature.
            AdressBox.Text = _selectedFeature.Attributes["AD_ADDRESS"].ToString();
            StreetNameBox.Text = _selectedFeature.Attributes["ST_STR_NAM"].ToString();
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            // Hide entry UI.
            MyMapView.IsEnabled = true;
            AttributesBorder.Visibility = Visibility.Collapsed;
        }

        private async void OkClick(object sender, RoutedEventArgs e)
        {
            try
            {
                _selectedFeature.Attributes["AD_ADDRESS"] = int.Parse(AdressBox.Text);
                _selectedFeature.Attributes["ST_STR_NAM"] = StreetNameBox.Text;
                await _selectedFeature.FeatureTable.UpdateFeatureAsync(_selectedFeature);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Error");
            }
            finally
            {
                // Hide entry UI.
                MyMapView.IsEnabled = true;
                AttributesBorder.Visibility = Visibility.Collapsed;
            }
        }
    }
}