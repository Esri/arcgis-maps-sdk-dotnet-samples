// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Drawing;
using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ListTransformations
{
    [Register("ListTransformations")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "List transformations by suitability",
        "Geometry",
        "Get a list of suitable transformations for projecting a geometry between two spatial references with different horizontal datums.",
        "Select a transformation from the list to see the result of projecting the point from EPSG:27700 to EPSG:3857 using that transformation. The result is shown as a red cross; you can visually compare the original blue point with the projected red cross.",
        "datum", "geodesy", "projection", "spatial reference", "transformation")]
    public class ListTransformations : UIViewController
    {
        // Hold references to UI controls.
        private UILabel _inWkidLabel;
        private UILabel _outWkidLabel;
        private UIPickerView _transformationsPicker;
        private UISwitch _useExtentSwitch;
        private MapView _myMapView;
        private UIStackView _transformToolsView;
        private UIStackView _outerStackView;

        // Point whose coordinates will be projected using a selected transform.
        private MapPoint _originalPoint;

        // Graphic representing the projected point.
        private Graphic _projectedPointGraphic;

        // GraphicsOverlay to hold the point graphics.
        private GraphicsOverlay _pointsOverlay;

        // Text view to display messages to the user (exceptions, etc.).
        private UITextView _messagesTextView;

        public ListTransformations()
        {
            Title = "List transformations by suitability";
        }

        private void Initialize()
        {
            // Create the map and add it to the map view control.
            Map myMap = new Map(Basemap.CreateImageryWithLabels());

            // Create a point in the Greenwich observatory courtyard in London, UK, the location of the prime meridian. 
            _originalPoint = new MapPoint(538985.355, 177329.516, SpatialReference.Create(27700));

            // Set the initial extent to an extent centered on the point.
            myMap.InitialViewpoint = new Viewpoint(_originalPoint, 5000);

            // Handle the map loading to fill the UI controls.
            myMap.Loaded += MyMap_Loaded;

            // Add the map to the map view.
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
            if (!String.IsNullOrEmpty(peFolderPath))
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
            // Unsubscribe from event.
            ((Map) sender).Loaded -= MyMap_Loaded;

            // Get the map's spatial reference.
            SpatialReference mapSpatialReference = ((Map) sender).SpatialReference;

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

            // Handle the selection event to work with the selected transformation, avoiding duplicate subscriptions.
            if (_transformationsPicker?.Model != null && _transformationsPicker.Model is TransformationsPickerModel tm)
            {
                tm.TransformationSelected -= TransformationsPicker_TransformationSelected;
            }

            pickerModel.TransformationSelected += TransformationsPicker_TransformationSelected;

            // Apply the model to the picker.
            _transformationsPicker.Model = pickerModel;
        }

        // Handle selection events in the transformation picker.
        private void TransformationsPicker_TransformationSelected(object sender, TransformationSelectionEventArgs e)
        {
            // Get the selected transform from the event arguments. Return if none is selected.
            DatumTransformation selectedTransform = e.Transformation;
            if (selectedTransform == null)
            {
                return;
            }

            try
            {
                // Project the original point using the selected transform.
                MapPoint projectedPoint = (MapPoint) GeometryEngine.Project(_originalPoint, _myMapView.SpatialReference, selectedTransform);

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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _outerStackView = new UIStackView();
            _outerStackView.TranslatesAutoresizingMaskIntoConstraints = false;
            _outerStackView.Axis = UILayoutConstraintAxis.Vertical;
            _outerStackView.Distribution = UIStackViewDistribution.FillEqually;

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _transformToolsView = new UIStackView();
            _transformToolsView.TranslatesAutoresizingMaskIntoConstraints = false;
            _transformToolsView.Axis = UILayoutConstraintAxis.Vertical;
            _transformToolsView.Spacing = 8;
            _transformToolsView.LayoutMarginsRelativeArrangement = true;
            _transformToolsView.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);

            _inWkidLabel = new UILabel();
            _inWkidLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _outWkidLabel = new UILabel();
            _outWkidLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            UIStackView labelsRow = new UIStackView(new[] {_inWkidLabel, _outWkidLabel});
            labelsRow.TranslatesAutoresizingMaskIntoConstraints = false;
            labelsRow.Axis = UILayoutConstraintAxis.Horizontal;
            labelsRow.Distribution = UIStackViewDistribution.FillEqually;
            _transformToolsView.AddArrangedSubview(labelsRow);

            _useExtentSwitch = new UISwitch();
            _useExtentSwitch.TranslatesAutoresizingMaskIntoConstraints = false;
            UILabel useExtentSwitchLabel = new UILabel();
            useExtentSwitchLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            useExtentSwitchLabel.Text = "Use extent";

            UIStackView switchRow = new UIStackView(new UIView[] {_useExtentSwitch, useExtentSwitchLabel});
            switchRow.TranslatesAutoresizingMaskIntoConstraints = false;
            switchRow.Axis = UILayoutConstraintAxis.Horizontal;
            switchRow.Spacing = 8;
            _transformToolsView.AddArrangedSubview(switchRow);

            _transformationsPicker = new UIPickerView();
            _transformationsPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            _transformationsPicker.SetContentCompressionResistancePriority((float) UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Vertical);
            _transformToolsView.AddArrangedSubview(_transformationsPicker);

            _messagesTextView = new UITextView();
            _messagesTextView.TranslatesAutoresizingMaskIntoConstraints = false;
            _messagesTextView.SetContentCompressionResistancePriority((float) UILayoutPriority.Required, UILayoutConstraintAxis.Vertical);
            _messagesTextView.ScrollEnabled = false;
            _transformToolsView.AddArrangedSubview(_messagesTextView);

            _outerStackView.AddArrangedSubview(_myMapView);
            _outerStackView.AddArrangedSubview(_transformToolsView);

            // Add the views.
            View.AddSubviews(_outerStackView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _outerStackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _outerStackView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                _outerStackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                _outerStackView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            // Reset constraints.
            _outerStackView.RemoveFromSuperview();
            View.AddSubview(_outerStackView);

            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                _outerStackView.Axis = UILayoutConstraintAxis.Horizontal;
                _outerStackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
                _outerStackView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
                _outerStackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor).Active = true;
                _outerStackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
                _transformToolsView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
            }
            else
            {
                _outerStackView.Axis = UILayoutConstraintAxis.Vertical;
                _outerStackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
                _outerStackView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor).Active = true;
                _outerStackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor).Active = true;
                _outerStackView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _useExtentSwitch.ValueChanged += UseExtentSwitch_ValueChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _useExtentSwitch.ValueChanged -= UseExtentSwitch_ValueChanged;

            if (_transformationsPicker?.Model != null && _transformationsPicker.Model is TransformationsPickerModel tm)
            {
                tm.TransformationSelected -= TransformationsPicker_TransformationSelected;
            }
        }
    }

    // Class that defines a view model for showing available datum transformations in a picker control.
    public class TransformationsPickerModel : UIPickerViewModel
    {
        // Event raised when the selected transformation changes.
        public event EventHandler<TransformationSelectionEventArgs> TransformationSelected;

        // List of datum transformation values.
        private readonly IReadOnlyList<DatumTransformation> _datumTransformations;

        // Store the default transformation.
        private readonly DatumTransformation _defaultTransformation;

        // Store the selected transformation.
        private DatumTransformation _selectedTransformation;

        // Constructor that takes the datum transformations list to display.
        public TransformationsPickerModel(IReadOnlyList<DatumTransformation> transformationList, DatumTransformation defaultTransform)
        {
            _datumTransformations = transformationList;
            _defaultTransformation = defaultTransform;
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
            return _datumTransformations[(int) row].Name;
        }

        // Handle the selection event for the picker.
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            // Get the selected datum transformation factor.
            _selectedTransformation = _datumTransformations[(int) pickerView.SelectedRowInComponent(0)];

            // Raise the selection event (with the new transformation) so listeners can handle it.
            EventHandler<TransformationSelectionEventArgs> selectionEventHandler = TransformationSelected;
            if (selectionEventHandler != null)
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
            DatumTransformation thisTransform = _datumTransformations[(int) row];

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
                labelColor = UIColor.Black;
            }

            // Create a label to display the transform.
            return new UILabel(new RectangleF(0, 0, 260f, 30f))
            {
                TextColor = labelColor,
                Font = UIFont.SystemFontOfSize(16f),
                TextAlignment = UITextAlignment.Center,
                Text = thisTransform.Name
            };
        }
    }

    // Event arguments to return when a new datum transformation is selected in the picker.
    public class TransformationSelectionEventArgs : EventArgs
    {
        // Selected datum transformation.
        public DatumTransformation Transformation { get; }

        public TransformationSelectionEventArgs(DatumTransformation transformation)
        {
            Transformation = transformation;
        }
    }
}