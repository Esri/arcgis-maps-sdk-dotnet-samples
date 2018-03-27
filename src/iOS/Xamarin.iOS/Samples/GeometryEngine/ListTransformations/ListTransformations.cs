// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using UIKit;

namespace ArcGISRuntime.Samples.ListTransformations
{
    [Register("ListTransformations")]
	[ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "List transformations by suitability",
        "GeometryEngine",
        "This sample demonstrates how to use the TransformationCatalog to get a list of available DatumTransformations that can be used to project a Geometry between two different SpatialReferences, and how to use one of the transformations to perform the GeometryEngine.project operation. The TransformationCatalog is also used to set the location of files upon which grid-based transformations depend, and to find the default transformation used for the two SpatialReferences.",
        "Tap on a listed transformation to re-project the point geometry (shown with a blue square) using the selected transformation. The reprojected geometry will be shown in red. If there are grid-based transformations for which projection engine files are not available locally, these will be shown in gray in the list. The default transformation is shown in bold. To download the additional transformation data, log on to your developers account (http://developers.arcgis.com), click the 'Download APIs' button on the dashboard page, and download the 'Coordinate System Data' archive from the 'Supplemental ArcGIS Runtime Data' tab. Unzip the archive to the 'SampleData' sub-folder of the ApplicationData directory, which can be found for each platform at run time with System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData).",
        "Featured")]
    public class ListTransformations : UIViewController
    {
        // Map view control to display a map in the app.
        private MapView _myMapView = new MapView();

        // Stack view to contain the datum transformation UI.
        private UIStackView _transformToolsView = new UIStackView();

        // Store the height of each set of controls (may vary on different devices).
        private nfloat _mapViewHeight;
        private nfloat _transformToolsHeight;

        // Point whose coordinates will be projected using a selected transform.
        private MapPoint _originalPoint;

        // Graphic representing the projected point.
        private Graphic _projectedPointGraphic;

        // GraphicsOverlay to hold the point graphics.
        private GraphicsOverlay _pointsOverlay;

        // Text view to display messages to the user (exceptions, etc.).
        private UITextView _messagesTextView;

        // Labels to display the input/output spatial references (WKID).
        private UILabel _inWkidLabel;
        private UILabel _outWkidLabel;

        // Picker to display the datum transformations suitable for the input/output spatial references.
        private UIPickerView _transformationsPicker;

        // Switch to toggle suitable transformations for the current extent.
        private UISwitch _useExtentSwitch;

        public ListTransformations()
        {
            Title = "List datum transformations";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Get the height of the map view and the UI tools view (one half each).
            _mapViewHeight = (nfloat)(View.Bounds.Height / 2.0);
            _transformToolsHeight = (nfloat)(View.Bounds.Height / 2.0);

            // Create the UI.
            CreateLayout();

            // Create a new map, add a point graphic, and fill the datum transformations list.
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            base.ViewDidLayoutSubviews();

            // Place the MapView (top 2/3 of the view)
            _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, _mapViewHeight);

            // Place the edit tools (bottom 1/3 of the view)
            _transformToolsView.Frame = new CGRect(0, _mapViewHeight, View.Bounds.Width, _transformToolsHeight);
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
            InvokeOnMainThread(() => 
            {
                // Show the input and output spatial reference (WKID) in the labels.
                _inWkidLabel.Text = "In WKID = " + _originalPoint.SpatialReference.Wkid;
                _outWkidLabel.Text = "Out WKID = " + mapSpatialReference.Wkid;

                // Call a function to create a list of transformations to fill the picker.
                GetSuitableTransformations(_originalPoint.SpatialReference, mapSpatialReference, _useExtentSwitch.On);
            });
        }

        private void CreateLayout()
        {
            // Place the map view in the upper half of the display.
            _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, _mapViewHeight);

            // Place the transformations UI in the bottom half.
            _transformToolsView.Axis = UILayoutConstraintAxis.Vertical;
            _transformToolsView.Frame = new CGRect(0, _mapViewHeight, View.Bounds.Width, _transformToolsHeight);

            // View for the input/output wkid labels.
            UIStackView wkidLabelsStackView = new UIStackView(new CGRect(10, 5, View.Bounds.Width-10, 35))
            {
                Axis = UILayoutConstraintAxis.Horizontal
            };

            // Create a label for the input spatial reference.
            _inWkidLabel = new UILabel(new CGRect(5, 0, (View.Bounds.Width / 2) - 15, 30))
            {
                Text = "In WKID = ",
                TextAlignment = UITextAlignment.Left,
                TextColor = UIColor.Blue
            };
            
            // Create a label for the output spatial reference.
            _outWkidLabel = new UILabel(new CGRect((View.Bounds.Width / 2) + 5, 0, (View.Bounds.Width / 2) - 15, 30))
            {
                Text = "Out WKID = ",
                TextAlignment = UITextAlignment.Left,
                TextColor = UIColor.Blue
            };

            // Add the Wkid labels to the stack view.
            wkidLabelsStackView.Add(_inWkidLabel);
            wkidLabelsStackView.Add(_outWkidLabel);

            // Create a horizontal stack view for the 'use extent' switch and label.
            UIStackView extentSwitchRow = new UIStackView(new CGRect(20, 35, View.Bounds.Width - 20, 35))
            {
                Axis = UILayoutConstraintAxis.Horizontal
            };
            _useExtentSwitch = new UISwitch
            {
                On = false
            };
            _useExtentSwitch.ValueChanged += UseExtentSwitch_ValueChanged;

            // Create a label for the use extent switch.
            UILabel useExtentLabel = new UILabel(new CGRect(70, 0, View.Bounds.Width - 70, 30))
            {
                Text = "Use extent",
                TextAlignment = UITextAlignment.Left,
                TextColor = UIColor.Blue
            };

            // Add the switch and the label to the horizontal stack view.
            extentSwitchRow.Add(_useExtentSwitch);
            extentSwitchRow.Add(useExtentLabel);
                        
            // Create a picker for datum transformations.
            _transformationsPicker = new UIPickerView(new CGRect(20, 70, View.Bounds.Width-20, 120));
            
            // Create a text view to show messages.
            _messagesTextView = new UITextView(new CGRect(20, 200, View.Bounds.Width-20, 60));

            // Add the controls to the transform UI (stack view).
            _transformToolsView.AddSubviews(wkidLabelsStackView, extentSwitchRow, _transformationsPicker, _messagesTextView);

            // Add the map view and tools subviews to the view.
            View.AddSubviews(_myMapView, _transformToolsView);

            // Set the view background color.
            View.BackgroundColor = UIColor.White;
        }

        private void UseExtentSwitch_ValueChanged(object sender, EventArgs e)
        {
            // Recreate the contents of the datum transformations list box.
            GetSuitableTransformations(_originalPoint.SpatialReference, _myMapView.Map.SpatialReference, _useExtentSwitch.On);
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

            // Create a picker model to display the updated transformations.
            TransformationsPickerModel pickerModel = new TransformationsPickerModel(transformations, defaultTransform);

            // Handle the selection event to work with the selected transformation.
            pickerModel.TransformationSelected += TransformationsPicker_TransformationSelected;

            // Apply the model to the picker.
            _transformationsPicker.Model = pickerModel;
        }

        // Handle selection events in the transformation picker.
        private void TransformationsPicker_TransformationSelected(object sender, TransformationSelectionEventArgs e)
        {
            // Get the selected transform from the event arguments. Return if none is selected.
            DatumTransformation selectedTransform = e.Transformation;
            if(selectedTransform == null) { return; }

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

        private string GetProjectionDataPath()
        {
            // Return the projection data path; note that this is not valid by default. 
            //You must manually download the projection engine data and update the path returned here. 
            return "";
        }
    }

    // Class that defines a view model for showing available datum transformations in a picker control.
    public class TransformationsPickerModel : UIPickerViewModel
    {
        // Event raised when the selected transformation changes.
        public event EventHandler<TransformationSelectionEventArgs> TransformationSelected;

        // List of datum transformation values.
        private IReadOnlyList<DatumTransformation> _datumTransformations;

        // Store the default transformation.
        private DatumTransformation _defaultTransformation;

        // Store the selected transformation.
        private DatumTransformation _selectedTransformation;

        // Constructor that takes the datum transformations list to display.
        public TransformationsPickerModel(IReadOnlyList<DatumTransformation> transformationList, DatumTransformation defaultTransform)
        {
            _datumTransformations = transformationList;
            _defaultTransformation = defaultTransform;
        }

        // Property to expose the currently selected transformation value in the picker.
        public DatumTransformation SelectedDatumTransformation
        {
            get { return _selectedTransformation; }
        }

        // Return the number of picker components (just one).
        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 1;
        }

        // Return the number of rows in the section (the size of the transformations list).
        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return _datumTransformations.Count;
        }

        // Get the title to display in the picker component.
        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return _datumTransformations[(int)row].Name;
        }

        // Handle the selection event for the picker.
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            // Get the selected datum transformation factor.
            _selectedTransformation = _datumTransformations[(int)pickerView.SelectedRowInComponent(0)];

            // Raise the selection event (with the new transformation) so listeners can handle it.
            EventHandler<TransformationSelectionEventArgs> selectionEventHandler = TransformationSelected;
            if(selectionEventHandler != null)
            {
                TransformationSelectionEventArgs args = new TransformationSelectionEventArgs(_selectedTransformation);
                selectionEventHandler(this, args);
            }
        }

        // Return the desired width for each component in the picker.
        public override nfloat GetComponentWidth(UIPickerView picker, nint component)
        {
            return 280f;
        }

        // Return the desired height for rows in the picker.
        public override nfloat GetRowHeight(UIPickerView picker, nint component)
        {
            return 30f;
        }

        // Override GetView to create different label colors for each type of transformation.
        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            // Get the transformation being displayed.
            DatumTransformation thisTransform = _datumTransformations[(int)row];

            // See if this is the default transformation and if it's available (has required PE files).
            bool isDefault = thisTransform.Name == _defaultTransformation.Name;
            bool isNotAvailable = thisTransform.IsMissingProjectionEngineFiles;

            // Create the correct color for the transform type (available=black, default=blue, or unavailable=gray).
            UIColor labelColor = UIColor.Black;
            if (isNotAvailable)
            {
                labelColor = UIColor.Gray;
            }

            if (isDefault)
            {
                labelColor = UIColor.Blue;
            }

            // Create a label to display the transform.
            UILabel transformLabel = new UILabel(new RectangleF(0, 0, 260f, 30f))
            {
                TextColor = labelColor,
                Font = UIFont.SystemFontOfSize(16f),
                TextAlignment = UITextAlignment.Center,
                Text = thisTransform.Name
            };

            return transformLabel;
        }
    }

    // Event arguments to return when a new datum transformation is selected in the picker.
    public class TransformationSelectionEventArgs : EventArgs
    {
        // Selected datum transformation.
        public DatumTransformation Transformation { get; set; }

        public TransformationSelectionEventArgs(DatumTransformation transformation)
        {
            Transformation = transformation;
        }
    }
}