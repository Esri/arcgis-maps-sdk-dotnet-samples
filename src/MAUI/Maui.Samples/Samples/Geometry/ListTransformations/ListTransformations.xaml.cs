// Copyright 2022 Esri.
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
using System.Collections.ObjectModel;

namespace ArcGIS.Samples.ListTransformations
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "List transformations by suitability",
        category: "Geometry",
        description: "Get a list of suitable transformations for projecting a geometry between two spatial references with different horizontal datums.",
        instructions: "Select a transformation from the list to see the result of projecting the point from EPSG:27700 to EPSG:3857 using that transformation. The result is shown as a red cross; you can visually compare the original blue point with the projected red cross.",
        tags: new[] { "datum", "geodesy", "projection", "spatial reference", "transformation" })]
    public partial class ListTransformations : ContentPage
    {
        // Point whose coordinates will be projected using a selected transform.
        private MapPoint _originalPoint;

        // Graphic representing the projected point.
        private Graphic _projectedPointGraphic;

        // GraphicsOverlay to hold the point graphics.
        private GraphicsOverlay _pointsOverlay;

        public ObservableCollection<DatumTransformationListBoxItem> SuitableTransformationsList { get; } = new ObservableCollection<DatumTransformationListBoxItem>();

        public ListTransformations()
        {
            InitializeComponent();

            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create the map.
            Map myMap = new Map(BasemapStyle.ArcGISImagery);

            // Create a point in the Greenwich observatory courtyard in London, UK, the location of the prime meridian.
            _originalPoint = new MapPoint(538985.355, 177329.516, SpatialReference.Create(27700));

            // Set the initial extent to an extent centered on the point.
            Viewpoint initialViewpoint = new Viewpoint(_originalPoint, 5000);
            myMap.InitialViewpoint = initialViewpoint;

            // Add the map to the map view.
            MyMapView.Map = myMap;

            // Create a graphics overlay to hold the original and projected points.
            _pointsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_pointsOverlay);

            // Add the point as a graphic with a blue square.
            SimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, System.Drawing.Color.Blue, 15);
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

                // Set up the UI.
                TransformationsListBox.ItemsSource = SuitableTransformationsList;

                // Create a list of transformations to fill the UI list box.
                GetSuitableTransformations(_originalPoint.SpatialReference, myMap.SpatialReference, UseExtentSwitch.IsToggled);
            }
            catch (Exception e)
            {
                await Application.Current.Windows[0].Page.DisplayAlert("Error", e.ToString(), "OK");
            }
        }

        // Function to get suitable datum transformations for the specified input and output spatial references.
        private void GetSuitableTransformations(SpatialReference inSpatialRef, SpatialReference outSpatialRef, bool considerExtent)
        {
            // Get suitable transformations. Use the current extent to evaluate suitability, if requested.
            IReadOnlyList<DatumTransformation> transformations;
            if (considerExtent)
            {
                Envelope currentExtent = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;
                transformations = TransformationCatalog.GetTransformationsBySuitability(inSpatialRef, outSpatialRef, currentExtent);
            }
            else
            {
                transformations = TransformationCatalog.GetTransformationsBySuitability(inSpatialRef, outSpatialRef);
            }

            // Get the default transformation for the specified input and output spatial reference.
            DatumTransformation defaultTransform = TransformationCatalog.GetTransformation(inSpatialRef, outSpatialRef);

            // Reset list.
            SuitableTransformationsList.Clear();

            // Wrap the transformations in a class that includes a boolean to indicate if it's the default transformation.
            foreach (DatumTransformation transform in transformations)
            {
                DatumTransformationListBoxItem item = new DatumTransformationListBoxItem(transform)
                {
                    IsDefault = (transform.Name == defaultTransform.Name)
                };
                SuitableTransformationsList.Add(item);
            }
        }

        void TransformationsListBox_SelectionChanged(System.Object sender, Microsoft.Maui.Controls.SelectionChangedEventArgs e)
        {
            // Get the selected transform from the list box. Return if there isn't a selected item.
            DatumTransformationListBoxItem selectedListBoxItem = TransformationsListBox.SelectedItem as DatumTransformationListBoxItem;
            if (selectedListBoxItem == null) { return; }

            // Get the datum transformation object from the list box item.
            DatumTransformation selectedTransform = selectedListBoxItem.TransformationObject;

            try
            {
                // Project the original point using the selected transform.
                MapPoint projectedPoint = (MapPoint)_originalPoint.Project(MyMapView.SpatialReference, selectedTransform);

                // Update the projected graphic (if it already exists), create it otherwise.
                if (_projectedPointGraphic != null)
                {
                    _projectedPointGraphic.Geometry = projectedPoint;
                }
                else
                {
                    // Create a symbol to represent the projected point (a cross to ensure both markers are visible).
                    SimpleMarkerSymbol projectedPointMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, System.Drawing.Color.Red, 15);

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

        private void UseExtentSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            // Recreate the contents of the datum transformations list box.
            GetSuitableTransformations(_originalPoint.SpatialReference, MyMapView.Map.SpatialReference, UseExtentSwitch.IsToggled);
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

    // Class to select the appropriate data template for datum transformation list items.
    public class TransformRowTemplateSelector : DataTemplateSelector
    {
        // Data templates for three types of datum transformations.
        // - Those without supporting projection engine data (making the transformation unavailable).
        private readonly DataTemplate _unavailableTransformTemplate;

        // - Available transformations (data required is either available by default, or has been stored on the device).
        private readonly DataTemplate _availableTransformTemplate;

        // - The default datum transformation for the context (input/output spatial reference, and possibly the extent).
        private readonly DataTemplate _defaultTransformTemplate;

        public TransformRowTemplateSelector()
        {
            // Create the data template for unavailable transformations.
            _unavailableTransformTemplate = new DataTemplate(() =>
            {
                Label transformNameLabel = new Label { Padding = new Thickness(4,4,4,0) };
                transformNameLabel.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#6A6A6A"), Color.FromArgb("#9F9F9F"));
                transformNameLabel.SetBinding(Label.TextProperty, "TransformationObject.Name");

                return transformNameLabel;
            });

            // Create the data template for available (but non-default) transformations.
            _availableTransformTemplate = new DataTemplate(() =>
            {
                Label transformNameLabel = new Label { Padding = new Thickness(4,4,4,0) };
                transformNameLabel.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#151515"), Colors.White);
                transformNameLabel.SetBinding(Label.TextProperty, "TransformationObject.Name");

                return transformNameLabel;
            });

            // Create the data template for the default transformation.
            _defaultTransformTemplate = new DataTemplate(() =>
            {
                Label transformNameLabel = new Label
                {
                    // Show these with bold blue text.
                    FontAttributes = FontAttributes.Bold,
                    Padding = new Thickness(4,4,4,0)
                };
                transformNameLabel.SetAppThemeColor(Label.TextColorProperty, Color.FromArgb("#00619B"), Color.FromArgb("#00A0FF"));
                transformNameLabel.SetBinding(Label.TextProperty, "TransformationObject.Name");

                return transformNameLabel;
            });
        }

        // Logic that is called when a template is needed for a list view item.
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            DataTemplate selectedTemplate = null;

            // Get the current list item being created. Return if the item is null.
            DatumTransformationListBoxItem transformationItem = item as DatumTransformationListBoxItem;
            if (transformationItem == null)
            {
                return null;
            }

            // Read the IsMissingProjectionEngineFiles property to select the available or unavailable data template.
            if (transformationItem.TransformationObject.IsMissingProjectionEngineFiles)
            {
                selectedTemplate = _unavailableTransformTemplate;
            }
            else if (!transformationItem.TransformationObject.IsMissingProjectionEngineFiles)
            {
                selectedTemplate = _availableTransformTemplate;
            }

            // See if this is the default transformation.
            if (transformationItem.IsDefault)
            {
                selectedTemplate = _defaultTransformTemplate;
            }

            // Return the selected template.
            return selectedTemplate;
        }
    }
}