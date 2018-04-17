// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Collections.Generic;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Xamarin.Forms;

#if WINDOWS_UWP
using Colors = Windows.UI.Colors;
#else

using Colors = System.Drawing.Color;

#endif

namespace ArcGISRuntime.Samples.ListTransformations
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "List transformations by suitability",
        "GeometryEngine",
        "This sample demonstrates how to use the TransformationCatalog to get a list of available DatumTransformations that can be used to project a Geometry between two different SpatialReferences, and how to use one of the transformations to perform the GeometryEngine.project operation. The TransformationCatalog is also used to set the location of files upon which grid-based transformations depend, and to find the default transformation used for the two SpatialReferences.",
        "Tap on a listed transformation to re-project the point geometry (shown with a blue square) using the selected transformation. The reprojected geometry will be shown in red. If there are grid-based transformations for which projection engine files are not available locally, these will be shown in gray in the list. The default transformation is shown in bold. To download the additional transformation data, log on to your developers account (http://developers.arcgis.com), click the 'Download APIs' button on the dashboard page, and download the 'Coordinate System Data' archive from the 'Supplemental ArcGIS Runtime Data' tab. Unzip the archive to the 'SampleData' sub-folder of the ApplicationData directory, which can be found for each platform at run time with System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData).",
        "Featured")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class ListTransformations : ContentPage
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
            get
            {
                return _datumTransformations;
            }
            set
            {
                _datumTransformations = value;
                OnPropertyChanged("SuitableTransformationsList");
            }
        }

        public ListTransformations()
        {
            InitializeComponent();

            Title = "List transformations";

            // Bind the view to this page.
            BindingContext = this;

            // Create a new map, add a point graphic, and fill the datum transformations list.
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map.
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Create a point in the Greenwich observatory courtyard in London, UK, the location of the prime meridian. 
            _originalPoint = new MapPoint(538985.355, 177329.516, SpatialReference.Create(27700));

            // Set the initial extent to an extent centered on the point.
            Viewpoint initialViewpoint = new Viewpoint(_originalPoint, 5000);
            myMap.InitialViewpoint = initialViewpoint;

            // Load the map and add the map to the map view.
            await myMap.LoadAsync();
            MyMapView.Map = myMap;

            // Create a graphics overlay to hold the original and projected points.
            _pointsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_pointsOverlay);

            // Add the point as a graphic with a blue square.
            SimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, Colors.Blue, 15);
            Graphic originalGraphic = new Graphic(_originalPoint, markerSymbol);
            _pointsOverlay.Graphics.Add(originalGraphic);

            // Get the path to the projection engine data (if it exists).
            string peFolderPath = GetProjectionDataPath();
            if (!string.IsNullOrEmpty(peFolderPath))
            {
                TransformationCatalog.ProjectionEngineDirectory = peFolderPath;
                MessagesTextBox.Text = "Using projection data found at '" + peFolderPath + "'";
            }
            else
            {
                MessagesTextBox.Text = "Projection engine data not found.";
            }

            // Show the input and output spatial reference.
            InSpatialRefTextBox.Text = "In WKID = " + _originalPoint.SpatialReference.Wkid;
            OutSpatialRefTextBox.Text = "Out WKID = " + myMap.SpatialReference.Wkid;

            // Create a list of transformations to fill the UI list box.
            GetSuitableTransformations(_originalPoint.SpatialReference, myMap.SpatialReference, UseExtentSwitch.IsToggled);
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

        private void TransformationsListBox_ItemSelected(object sender, SelectedItemChangedEventArgs e)
        {
            // Get the selected transform from the list box. Return if there isn't a selected item.
            DatumTransformationListBoxItem selectedListBoxItem = TransformationsListBox.SelectedItem as DatumTransformationListBoxItem;
            if (selectedListBoxItem == null) { return; }

            // Get the datum transformation object from the list box item.
            DatumTransformation selectedTransform = selectedListBoxItem.TransformationObject;

            try
            {
                // Project the original point using the selected transform.
                MapPoint projectedPoint = (MapPoint)GeometryEngine.Project(_originalPoint, MyMapView.SpatialReference, selectedTransform);

                // Update the projected graphic (if it already exists), create it otherwise.
                if (_projectedPointGraphic != null)
                {
                    _projectedPointGraphic.Geometry = projectedPoint;
                }
                else
                {
                    // Create a symbol to represent the projected point (a cross to ensure both markers are visible).
                    SimpleMarkerSymbol projectedPointMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Colors.Red, 15);

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
                Label transformNameLabel = new Label
                {
                    // Show these with gray text.
                    TextColor = Color.Gray,
                    BackgroundColor = Color.White
                };
                transformNameLabel.SetBinding(Label.TextProperty, "TransformationObject.Name");

                return new ViewCell { View = transformNameLabel };
            });

            // Create the data template for available (but non-default) transformations.
            _availableTransformTemplate = new DataTemplate(() =>
            {
                Label transformNameLabel = new Label
                {
                    // Show these with black text.
                    TextColor = Color.Black,
                    BackgroundColor = Color.White
                };
                transformNameLabel.SetBinding(Label.TextProperty, "TransformationObject.Name");

                return new ViewCell { View = transformNameLabel };
            });

            // Create the data template for the default transformation.
            _defaultTransformTemplate = new DataTemplate(() =>
            {
                Label transformNameLabel = new Label
                {
                    // Show these with bold blue text.
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Color.Blue,
                    BackgroundColor = Color.White
                };
                transformNameLabel.SetBinding(Label.TextProperty, "TransformationObject.Name");

                return new ViewCell { View = transformNameLabel };
            });
        }

        // Logic that is called when a template is needed for a list view item.
        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            DataTemplate selectedTemplate = null;

            // Get the current list item being created. Return if the item is null.
            DatumTransformationListBoxItem transformationItem = item as DatumTransformationListBoxItem;
            if(item == null) { return null; }

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