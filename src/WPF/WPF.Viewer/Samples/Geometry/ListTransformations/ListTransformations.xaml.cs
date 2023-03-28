// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.ListTransformations
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "List transformations by suitability",
        category: "Geometry",
        description: "Get a list of suitable transformations for projecting a geometry between two spatial references with different horizontal datums.",
        instructions: "Select a transformation from the list to see the result of projecting the point from EPSG:27700 to EPSG:3857 using that transformation. The result is shown as a red cross; you can visually compare the original blue point with the projected red cross.",
        tags: new[] { "datum", "geodesy", "projection", "spatial reference", "transformation" })]
    public partial class ListTransformations : INotifyPropertyChanged
    {
        // Point whose coordinates will be projected using a selected transform.
        private MapPoint _originalPoint;

        // Graphic representing the projected point.
        private Graphic _projectedPointGraphic;

        // GraphicsOverlay to hold the point graphics.
        private GraphicsOverlay _pointsOverlay;

        // Property to expose the list of datum transformations for binding to the list box.
        private IReadOnlyList<DatumTransformationListBoxItem> _datumTransformations;

        public IReadOnlyList<DatumTransformationListBoxItem> SuitableTransformationsList
        {
            get { return _datumTransformations; }
            set
            {
                _datumTransformations = value;
                OnPropertyChanged("SuitableTransformationsList");
            }
        }

        // Implement INotifyPropertyChanged to indicate when the list of transformations has been updated.
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public ListTransformations()
        {
            InitializeComponent();

            // Create the map, set the initial extent, and add the original point graphic.
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the map.
            Map myMap = new Map(BasemapStyle.ArcGISImagery);

            // Create a point in the Greenwich observatory courtyard in London, UK, the location of the prime meridian.
            _originalPoint = new MapPoint(538985.355, 177329.516, SpatialReference.Create(27700));

            // Set the initial extent to an extent centered on the point.
            myMap.InitialViewpoint = new Viewpoint(_originalPoint, 5000);

            // Add the map to the view.
            MyMapView.Map = myMap;

            // Create a graphics overlay to hold the original and projected points.
            _pointsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_pointsOverlay);

            // Add the point as a graphic with a blue square.
            SimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, Color.Blue, 15);
            Graphic originalGraphic = new Graphic(_originalPoint, markerSymbol);
            _pointsOverlay.Graphics.Add(originalGraphic);

            // Get the path to the projection engine data (if it exists).
            string peFolderPath = GetProjectionDataPath();
            if (!String.IsNullOrEmpty(peFolderPath))
            {
                TransformationCatalog.ProjectionEngineDirectory = peFolderPath;
                MessagesTextBox.Text = "Using projection data found at '" + peFolderPath + "'";
            }
            else
            {
                MessagesTextBox.Text = "Projection engine data not found.";
            }

            try
            {
                // Wait for the map to load so that it has a spatial reference.
                await myMap.LoadAsync();

                // Show the input and output spatial reference.
                InSpatialRefTextBox.Text = "In WKID = " + _originalPoint.SpatialReference.Wkid;
                OutSpatialRefTextBox.Text = "Out WKID = " + myMap.SpatialReference.Wkid;

                // Create a list of transformations to fill the UI list box.
                GetSuitableTransformations(_originalPoint.SpatialReference, myMap.SpatialReference,
                    UseExtentCheckBox.IsChecked == true);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        // Function to get suitable datum transformations for the specified input and output spatial references.
        private void GetSuitableTransformations(SpatialReference inSpatialRef, SpatialReference outSpatialRef,
            bool considerExtent)
        {
            // Get suitable transformations. Use the current extent to evaluate suitability, if requested.
            IReadOnlyList<DatumTransformation> transformations;
            if (considerExtent)
            {
                Envelope currentExtent =
                    MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;
                transformations =
                    TransformationCatalog.GetTransformationsBySuitability(inSpatialRef, outSpatialRef, currentExtent);
            }
            else
            {
                transformations = TransformationCatalog.GetTransformationsBySuitability(inSpatialRef, outSpatialRef);
            }

            // Get the default transformation for the specified input and output spatial reference.
            DatumTransformation defaultTransform = TransformationCatalog.GetTransformation(inSpatialRef, outSpatialRef);

            List<DatumTransformationListBoxItem> transformationItems = new List<DatumTransformationListBoxItem>();

            // Wrap the transformations in a class that includes a boolean to indicate if it's the default transformation.
            foreach (DatumTransformation transform in transformations)
            {
                DatumTransformationListBoxItem item = new DatumTransformationListBoxItem(transform)
                {
                    IsDefault = (transform.Name == defaultTransform.Name)
                };
                transformationItems.Add(item);
            }

            // Set the transformation list property that the list box binds to.
            SuitableTransformationsList = transformationItems;
        }

        private void TransformationsListBox_Selected(object sender, RoutedEventArgs e)
        {
            // Get the selected transform from the list box. Return if there isn't a selected item.
            DatumTransformationListBoxItem selectedListBoxItem =
                TransformationsListBox.SelectedItem as DatumTransformationListBoxItem;
            if (selectedListBoxItem == null)
            {
                return;
            }

            DatumTransformation selectedTransform = selectedListBoxItem.TransformationObject;

            try
            {
                // Project the original point using the selected transform.
                MapPoint projectedPoint =
                    (MapPoint)_originalPoint.Project(MyMapView.SpatialReference, selectedTransform);

                // Update the projected graphic (if it already exists), create it otherwise.
                if (_projectedPointGraphic != null)
                {
                    _projectedPointGraphic.Geometry = projectedPoint;
                }
                else
                {
                    // Create a symbol to represent the projected point (a cross to ensure both markers are visible).
                    SimpleMarkerSymbol projectedPointMarker =
                        new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.Red, 15);

                    // Create the point graphic and add it to the overlay.
                    _projectedPointGraphic = new Graphic(projectedPoint, projectedPointMarker);
                    _pointsOverlay.Graphics.Add(_projectedPointGraphic);
                }

                MessagesTextBox.Text = "Projected point using transform: " + selectedTransform.Name;
            }
            catch (ArcGISRuntimeException ex)
            {
                // Exception if a transformation is missing grid files.
                MessagesTextBox.Text = "Error using selected transformation: " + ex.Message;

                // Remove the projected point graphic (if it exists).
                if (_projectedPointGraphic != null && _pointsOverlay.Graphics.Contains(_projectedPointGraphic))
                {
                    _pointsOverlay.Graphics.Remove(_projectedPointGraphic);
                    _projectedPointGraphic = null;
                }
            }
        }

        private void UseExtentCheckBox_CheckChanged(object sender, RoutedEventArgs e)
        {
            // Recreate the contents of the datum transformations list box.
            GetSuitableTransformations(_originalPoint.SpatialReference, MyMapView.Map.SpatialReference,
                UseExtentCheckBox.IsChecked == true);
        }

        private string GetProjectionDataPath()
        {
            // Return the projection data path; note that this is not valid by default.
            //You must manually download the projection engine data and update the path returned here.
            return "";
        }
    }

    // A class that wraps a DatumTransformation object and adds a property that indicates if it's the default transformation.
    public class DatumTransformationListBoxItem
    {
        // Datum transformation object.
        public DatumTransformation TransformationObject { get; set; }

        // Whether or not this transformation is the default (for the specified in/out spatial reference).
        public bool IsDefault { get; set; }

        // Constructor that takes the DatumTransformation object to wrap.
        public DatumTransformationListBoxItem(DatumTransformation transformation)
        {
            TransformationObject = transformation;
        }
    }
}