// Copyright 2018 Esri.
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
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace ArcGISRuntime.Samples.ListTransformations
{
    [Activity]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "List transformations by suitability",
        "GeometryEngine",
        "This sample demonstrates how to use the TransformationCatalog to get a list of available DatumTransformations that can be used to project a Geometry between two different SpatialReferences, and how to use one of the transformations to perform the GeometryEngine.project operation. The TransformationCatalog is also used to set the location of files upon which grid-based transformations depend, and to find the default transformation used for the two SpatialReferences.",
        "Tap on a listed transformation to re-project the point geometry (shown with a blue square) using the selected transformation. The reprojected geometry will be shown in red. If there are grid-based transformations for which projection engine files are not available locally, these will be shown in gray in the list. The default transformation is shown in bold. To download the additional transformation data, log on to your developers account (http://developers.arcgis.com), click the 'Download APIs' button on the dashboard page, and download the 'Coordinate System Data' archive from the 'Supplemental ArcGIS Runtime Data' tab. Unzip the archive to the 'SampleData' sub-folder of the ApplicationData directory, which can be found for each platform at run time with System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData).",
        "Featured")]
    public class ListTransformations : Activity
    {
        // Map view control to display a map in the app.
        private MapView _myMapView = new MapView();
        
        // Point whose coordinates will be projected using a selected transform.
        private MapPoint _originalPoint;

        // Graphic representing the projected point.
        private Graphic _projectedPointGraphic;

        // GraphicsOverlay to hold the point graphics.
        private GraphicsOverlay _pointsOverlay;

        // Text view to display messages to the user (exceptions, etc.).
        private TextView _messagesTextView;

        // Labels to display the input/output spatial references (WKID).
        private TextView _inWkidLabel;
        private TextView _outWkidLabel;

        // Spinner to display the datum transformations suitable for the input/output spatial references.
        private Spinner _transformationsPicker;

        // Switch to toggle suitable transformations for the current extent.
        private Switch _useExtentSwitch;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "List transformations by suitability";

            // Create the UI.
            CreateLayout();

            // Create a new map, add a point graphic, and fill the datum transformations list.
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map and add it to the map view control.
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Create a point in the Greenwich observatory courtyard in London, UK, the location of the prime meridian. 
            _originalPoint = new MapPoint(538985.355, 177329.516, SpatialReference.Create(27700));

            // Set the initial extent to an extent centered on the point.
            Viewpoint initialViewpoint = new Viewpoint(_originalPoint, 5000);
            myMap.InitialViewpoint = initialViewpoint;

            // Handle the map loading to fill the UI controls.
            myMap.Loaded += MyMap_Loaded;

            // Load the map and add the map to the map view.
            await myMap.LoadAsync();
            _myMapView.Map = myMap;

            // Create a graphics overlay to hold the original and projected points.
            _pointsOverlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(_pointsOverlay);

            // Add the point as a graphic with a blue square.
            SimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Square, Color.Blue, 15);
            Graphic originalGraphic = new Graphic(_originalPoint, markerSymbol);
            _pointsOverlay.Graphics.Add(originalGraphic);

            // Get the path to the projection engine data (if it exists).
            string peFolderPath = GetProjectionDataPath();
            if (!string.IsNullOrEmpty(peFolderPath))
            {
                TransformationCatalog.ProjectionEngineDirectory = peFolderPath;
                _messagesTextView.Text = "Using projection data found at '" + peFolderPath + "'";
            }
            else
            {
                _messagesTextView.Text = "Projection engine data not found.";
            }
        }

        private void MyMap_Loaded(object sender, EventArgs e)
        {
            // Get the map's spatial reference.
            SpatialReference mapSpatialReference = (sender as Map).SpatialReference;

            // Run on the UI thread.
            RunOnUiThread(() =>
            {
                // Show the input and output spatial reference (WKID) in the labels.
                _inWkidLabel.Text = "In WKID = " + _originalPoint.SpatialReference.Wkid;
                _outWkidLabel.Text = "Out WKID = " + mapSpatialReference.Wkid;

                // Call a function to create a list of transformations to fill the picker.
                GetSuitableTransformations(_originalPoint.SpatialReference, mapSpatialReference, _useExtentSwitch.Checked);
            });
        }

        private void CreateLayout()
        {
            // View for the input/output wkid labels.
            LinearLayout wkidLabelsStackView = new LinearLayout(this) { Orientation = Orientation.Horizontal };
            wkidLabelsStackView.SetPadding(10, 10, 0, 10);

            // Create a label for the input spatial reference.
            _inWkidLabel = new TextView(this)
            {
                Text = "In WKID = ",
                TextAlignment = TextAlignment.ViewStart
            };

            // Create a label for the output spatial reference.
            _outWkidLabel = new TextView(this)
            {
                Text = "Out WKID = ",
                TextAlignment = TextAlignment.ViewStart
            };

            // Create some horizontal space between the labels.
            Space space = new Space(this);
            space.SetMinimumWidth(30);

            // Add the Wkid labels to the stack view.
            wkidLabelsStackView.AddView(_inWkidLabel);
            wkidLabelsStackView.AddView(space);
            wkidLabelsStackView.AddView(_outWkidLabel);

            // Create the 'use extent' switch.
            _useExtentSwitch = new Switch(this)
            {
                Checked = false,
                Text = "Use extent"
            };

            // Handle the checked change event for the switch.
            _useExtentSwitch.CheckedChange += UseExtentSwitch_CheckedChange;            

            // Create a picker (Spinner) for datum transformations.
            _transformationsPicker = new Spinner(this);
            _transformationsPicker.SetPadding(5, 10, 0, 10);

            // Handle the selection event to work with the selected transformation.
            _transformationsPicker.ItemSelected += TransformationsPicker_ItemSelected;

            // Create a text view to show messages.
            _messagesTextView = new TextView(this);

            // Create a new vertical layout for the app UI.
            LinearLayout mainLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a layout for the app tools.
            LinearLayout toolsLayout = new LinearLayout(this) { Orientation = Orientation.Vertical };
            toolsLayout.SetPadding(10, 0, 0, 0);
            toolsLayout.SetMinimumHeight(320);

            // Add the transformation UI controls to the tools layout.
            toolsLayout.AddView(wkidLabelsStackView);
            toolsLayout.AddView(_useExtentSwitch);
            toolsLayout.AddView(_transformationsPicker);
            toolsLayout.AddView(_messagesTextView);

            // Add the tools layout and map view to the main layout.
            mainLayout.AddView(toolsLayout);
            mainLayout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(mainLayout);
        }

        // Function to get suitable datum transformations for the specified input and output spatial references.
        private void GetSuitableTransformations(SpatialReference inSpatialRef, SpatialReference outSpatialRef, bool considerExtent)
        {
            // Get suitable transformations. Use the current extent to evaluate suitability, if requested.
            IReadOnlyList<DatumTransformation> transformations;
            if (considerExtent)
            {
                Envelope currentExtent = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry as Envelope;
                transformations = TransformationCatalog.GetTransformationsBySuitability(inSpatialRef, outSpatialRef, currentExtent);
            }
            else
            {
                transformations = TransformationCatalog.GetTransformationsBySuitability(inSpatialRef, outSpatialRef);
            }

            // Get the default transformation for the specified input and output spatial reference.
            DatumTransformation defaultTransform = TransformationCatalog.GetTransformation(inSpatialRef, outSpatialRef);

            // Create a list of transformations.
            List<DatumTransformation> transformsList = new List<DatumTransformation>();
            foreach(DatumTransformation transformation in transformations)
            {
                transformsList.Add(transformation);
            }

            // Create an adapter for showing the spinner list.
            TransformationsAdapter transformationsAdapter = new TransformationsAdapter(this, transformsList);
            transformationsAdapter.DefaultTransformation = defaultTransform;
            
            // Apply the adapter to the spinner.
            _transformationsPicker.Adapter = transformationsAdapter;
        }

        private void TransformationsPicker_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // Get the selected transform from the spinner. Return if none is selected.
            TransformationsAdapter adapter = _transformationsPicker.Adapter as TransformationsAdapter;            
            DatumTransformation selectedTransform = adapter[e.Position];
            if (selectedTransform == null) { return; }

            try
            {
                // Project the original point using the selected transform.
                MapPoint projectedPoint = (MapPoint)GeometryEngine.Project(_originalPoint, _myMapView.SpatialReference, selectedTransform);

                // Update the projected graphic (if it already exists), create it otherwise.
                if (_projectedPointGraphic != null)
                {
                    _projectedPointGraphic.Geometry = projectedPoint;
                }
                else
                {
                    // Create a symbol to represent the projected point (a cross to ensure both markers are visible).
                    SimpleMarkerSymbol projectedPointMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Cross, Color.Red, 15);

                    // Create the point graphic and add it to the overlay.
                    _projectedPointGraphic = new Graphic(projectedPoint, projectedPointMarker);
                    _pointsOverlay.Graphics.Add(_projectedPointGraphic);
                }

                _messagesTextView.Text = "Projected point using transform: " + selectedTransform.Name;
            }
            catch (ArcGISRuntimeException ex)
            {
                // Exception if a transformation is missing grid files.
                _messagesTextView.Text = "Error using selected transformation: " + ex.Message;

                // Remove the projected point graphic (if it exists).
                if (_projectedPointGraphic != null && _pointsOverlay.Graphics.Contains(_projectedPointGraphic))
                {
                    _pointsOverlay.Graphics.Remove(_projectedPointGraphic);
                    _projectedPointGraphic = null;
                }
            }
        }

        private void UseExtentSwitch_CheckedChange(object sender, CompoundButton.CheckedChangeEventArgs e)
        {
            // Call a function to create a list of transformations to fill the picker.
            GetSuitableTransformations(_originalPoint.SpatialReference, _myMapView.Map.SpatialReference, _useExtentSwitch.Checked);
        }

        private string GetProjectionDataPath()
        {
            // Return the projection data path; note that this is not valid by default. 
            // You must manually download the projection engine data and update the path returned here. 
            return "";
        }
    }

    // An Adapter class to provide a list of datum transformations for display in a Spinner control.
    public class TransformationsAdapter : BaseAdapter<DatumTransformation>
    {
        // Property to expose the default datum transformation (will be displayed with different text color).
        public DatumTransformation DefaultTransformation { get; set; }

        // Fields to store the list of transformations and the current context.
        private List<DatumTransformation> _transformations;
        private Activity _context;

        // Constructor for the adapter. Store the context and the list of transformations to display.
        public TransformationsAdapter(Activity context, List<DatumTransformation> items) : base()
        {
            _transformations = items;
            _context = context;
        }

        // Provide an ID for an item at a given position (just return the position).
        public override long GetItemId(int position)
        {
            return position;
        }

        // Provide the datum transformation at this position in the list.
        public override DatumTransformation this[int position]
        {
            get { return _transformations[position]; }
        }

        // Provide the number of items (datum transformations) in the list.
        public override int Count
        {
            get
            {
                return _transformations.Count;
            }
        }

        // Override the GetView method to provide a custom (formatted) text view for each transformation in the list.
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            // Create a new text view to display the transformation name with the proper formatting.
            TextView transformTextView = new TextView(_context);

            // Get the datum transformation being displayed.
            DatumTransformation thisTransform = _transformations[position];

            // Set the text with the transformation name.
            transformTextView.SetText(thisTransform.Name, TextView.BufferType.Normal);

            // Use white as the default text color (available transforms).
            transformTextView.SetTextColor(Android.Graphics.Color.White);

            // See if the transform is missing required projection engine files. If so, display the text in gray.
            if (thisTransform.IsMissingProjectionEngineFiles)
            {
                transformTextView.SetTextColor(Android.Graphics.Color.Gray);
            }

            // If this is the default transformation, show it in blue.
            if(thisTransform.Name == DefaultTransformation.Name)
            {
                transformTextView.SetTextColor(Android.Graphics.Color.Blue);
            }
            
            // Pass back the text view.
            return transformTextView;
        }
    }
}