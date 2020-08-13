// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.CreateAndSaveKmlFile
{
    [Register("CreateAndSaveKmlFile")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Create and save KML file",
        category: "Layers",
        description: "Construct a KML document and save it as a KMZ file.",
        instructions: "Tap on one of the buttons in the middle row to start adding a geometry. Tap on the map view to place vertices. Tap the \"Complete Sketch\" button to add the geometry to the KML document as a new KML placemark. Use the style interface to edit the style of the placemark. If you do not wish to set a style, tap the \"Don't Apply Style\" button. When you are finished adding KML nodes, tap on the \"Save KMZ file\" button to save the active KML document as a .kmz file on your system. Use the \"Reset\" button to clear the current KML document and start a new one.",
        tags: new[] { "KML", "KMZ", "Keyhole", "OGC", "Featured" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class CreateAndSaveKmlFile : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIToolbar _toolbar;
        private UIBarButtonItem _addButton;
        private UIBarButtonItem _resetButton;
        private UIBarButtonItem _saveButton;
        private UIBarButtonItem _doneButton;
        private UILabel _helpLabel;

        // KML objects
        private KmlDocument _kmlDocument;
        private KmlDataset _kmlDataset;
        private KmlLayer _kmlLayer;
        private KmlPlacemark _currentPlacemark;

        private List<string> _iconLinks;
        private List<UIColor> _colorList;

        public CreateAndSaveKmlFile()
        {
            Title = "Create and save KML file";
        }

        private void Initialize()
        {
            // Create the map.
            _myMapView.Map = new Map(Basemap.CreateImagery());

            // Load the KML document.
            ResetKml();

            // Set the links for icons.
            _iconLinks = new List<string>()
            {
                "https://static.arcgis.com/images/Symbols/Shapes/BlueCircleLargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BlueDiamondLargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BluePin1LargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BluePin2LargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BlueSquareLargeB.png",
                "https://static.arcgis.com/images/Symbols/Shapes/BlueStarLargeB.png"
            };

            // Set the colors for the color picker.
            _colorList = new List<UIColor>
            {
                UIColor.Black,UIColor.Blue,UIColor.Brown,UIColor.DarkGray,UIColor.Gray,UIColor.Green,UIColor.Magenta,UIColor.Orange,UIColor.Purple,UIColor.Red,UIColor.Yellow
            };
        }

        private void ResetKml()
        {
            // Clear any existing layers from the map.
            _myMapView.Map.OperationalLayers.Clear();

            // Reset the most recently placed placemark.
            _currentPlacemark = null;

            // Create a new KML document.
            _kmlDocument = new KmlDocument() { Name = "KML Sample Document" };

            // Create a KML dataset using the KML document.
            _kmlDataset = new KmlDataset(_kmlDocument);

            // Create the KML layer using the KML dataset.
            _kmlLayer = new KmlLayer(_kmlDataset);

            // Add the KML layer to the map.
            _myMapView.Map.OperationalLayers.Add(_kmlLayer);
        }

        private void AddClick(object sender, EventArgs e)
        {
            // Decide what type of placemark to add.
            UIAlertController prompt = UIAlertController.Create("Choose a geometry type", null, UIAlertControllerStyle.ActionSheet);
            prompt.AddAction(UIAlertAction.Create("Point", UIAlertActionStyle.Default, AddGeometry));
            prompt.AddAction(UIAlertAction.Create("Polyline", UIAlertActionStyle.Default, AddGeometry));
            prompt.AddAction(UIAlertAction.Create("Polygon", UIAlertActionStyle.Default, AddGeometry));

            // Needed to prevent crash on iPad.
            UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
            if (ppc != null)
            {
                ppc.BarButtonItem = _doneButton;
                ppc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
            }

            // Swap toolbar.
            _toolbar.Items = new[] { _doneButton };

            PresentViewController(prompt, true, null);
        }

        private async void AddGeometry(UIAlertAction obj)
        {
            try
            {
                // Create variables for the sketch creation mode and color.
                SketchCreationMode creationMode;

                // Set the creation mode and UI based on which button called this method.
                switch (obj.Title)
                {
                    case "Point":
                        creationMode = SketchCreationMode.Point;
                        _helpLabel.Text = "Tap to add a point.";
                        break;

                    case "Polyline":
                        creationMode = SketchCreationMode.Polyline;
                        _helpLabel.Text = "Tap to add a vertex.";
                        break;

                    case "Polygon":
                        creationMode = SketchCreationMode.Polygon;
                        _helpLabel.Text = "Tap to add a vertex.";
                        break;

                    default:
                        return;
                }

                // Get the user-drawn geometry.
                Geometry geometry = await _myMapView.SketchEditor.StartAsync(creationMode, true);

                // Project the geometry to WGS84 (WGS84 is required by the KML standard).
                Geometry projectedGeometry = GeometryEngine.Project(geometry, SpatialReferences.Wgs84);

                // Create a KmlGeometry using the new geometry.
                KmlGeometry kmlGeometry = new KmlGeometry(projectedGeometry, KmlAltitudeMode.ClampToGround);

                // Create a new placemark.
                _currentPlacemark = new KmlPlacemark(kmlGeometry);

                // Add the placemark to the KmlDocument.
                _kmlDocument.ChildNodes.Add(_currentPlacemark);

                // Enable the style editing UI.
                ChooseStyle();
            }
            catch (ArgumentException)
            {
                ShowMessage("Error", "Unsupported Geometry");
            }
            finally
            {
                // Re-add toolbar.
                _toolbar.Items = new[]
                {
                    _addButton,
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    _saveButton,
                    _resetButton
                };
                _helpLabel.Text = "";
            }
        }

        private void ChooseStyle()
        {
            // Start the UI for the user choosing the junction.
            if (_currentPlacemark.GraphicType == KmlGraphicType.Point)
            {
                // Build a UI alert controller for picking the icon.
                UIAlertController prompt = UIAlertController.Create(null, "Choose an icon.", UIAlertControllerStyle.ActionSheet);
                foreach (string link in _iconLinks)
                {
                    UIAlertAction action = UIAlertAction.Create(link, UIAlertActionStyle.Default, Icon_Select);
                    UIImage image = new UIImage(NSData.FromUrl(new NSUrl(link)));
                    action.SetValueForKey(image, new NSString("image"));
                    prompt.AddAction(action);
                }
                prompt.AddAction(UIAlertAction.Create("No Style", UIAlertActionStyle.Cancel, null));

                // Needed to prevent crash on iPad.
                UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
                if (ppc != null)
                {
                    ppc.BarButtonItem = _addButton;
                    ppc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                }

                PresentViewController(prompt, true, null);
            }
            else if (_currentPlacemark.GraphicType == KmlGraphicType.Polyline || _currentPlacemark.GraphicType == KmlGraphicType.Polygon)
            {
                // Build a UI alert controller for picking the color.
                UIAlertController prompt = UIAlertController.Create(null, "Choose a color.", UIAlertControllerStyle.ActionSheet);
                foreach (UIColor color in _colorList)
                {
                    UIAlertAction action = UIAlertAction.Create(color.ToString(), UIAlertActionStyle.Default, Color_Select);
                    action.SetValueForKey(color, new NSString("titleTextColor"));
                    prompt.AddAction(action);
                }
                prompt.AddAction(UIAlertAction.Create("No Style", UIAlertActionStyle.Cancel, null));

                // Needed to prevent crash on iPad.
                UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
                if (ppc != null)
                {
                    ppc.BarButtonItem = _addButton;
                    ppc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                }

                PresentViewController(prompt, true, null);
            }
        }

        private void Icon_Select(UIAlertAction obj)
        {
            // Get the Uri of the selected action.
            Uri uri = new Uri(obj.Title);

            // Create a style for the placemark.
            _currentPlacemark.Style = new KmlStyle();
            _currentPlacemark.Style.IconStyle = new KmlIconStyle(new KmlIcon(uri), 1.0);
        }

        private void Color_Select(UIAlertAction obj)
        {
            // Convert the UIColor to a System.Drawing.Color
            UIColor uiColor = obj.ValueForKey(new NSString("titleTextColor")) as UIColor;
            nfloat red, green, blue, alpha;
            uiColor.GetRGBA(out red, out green, out blue, out alpha);
            Color color = Color.FromArgb((int)(alpha * 255), (int)(red * 255), (int)(green * 255), (int)(blue * 255));

            // Create a style for the placemark.
            _currentPlacemark.Style = new KmlStyle();

            if (_currentPlacemark.GraphicType == KmlGraphicType.Polyline)
            {
                _currentPlacemark.Style.LineStyle = new KmlLineStyle(color, 8);
            }
            else if (_currentPlacemark.GraphicType == KmlGraphicType.Polygon)
            {
                _currentPlacemark.Style.PolygonStyle = new KmlPolygonStyle(color);
                _currentPlacemark.Style.PolygonStyle.IsFilled = true;
                _currentPlacemark.Style.PolygonStyle.IsOutlined = false;
            }
        }

        private void DoneClick(object sender, EventArgs e)
        {
            try
            {
                // Finish the sketch.
                _myMapView.SketchEditor.CompleteCommand.Execute(null);
            }
            catch (ArgumentException)
            {
            }
        }

        private void Reset_Click(object sender, EventArgs e)
        {
            ResetKml();
        }

        private async void Save_Click(object sender, EventArgs e)
        {
            try
            {
                string offlineDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

                // If temporary data folder doesn't exists, create it.
                if (!Directory.Exists(offlineDataFolder))
                {
                    Directory.CreateDirectory(offlineDataFolder);
                }

                string path = Path.Combine(offlineDataFolder, "sampledata.kmz");
                using (Stream stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None))
                {
                    // Write the KML document to the stream of the file.
                    await _kmlDocument.WriteToAsync(stream);
                }
                ShowMessage("Success", "File saved locally.");
            }
            catch (Exception)
            {
                ShowMessage("Error", "File not saved.");
            }
        }

        private void ShowMessage(string title, string message)
        {
            // Create alert for the user.
            UIAlertController alert = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            PresentViewController(alert, true, null);
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add);
            _doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done);

            _saveButton = new UIBarButtonItem();
            _saveButton.Title = "Save";

            _resetButton = new UIBarButtonItem();
            _resetButton.Title = "Reset";

            _toolbar = new UIToolbar();
            _toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            _toolbar.Items = new[]
            {
                _addButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _saveButton,
                _resetButton
            };

            _helpLabel = new UILabel
            {
                Text = "Press the '+' button to start.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, _toolbar, _helpLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(_toolbar.TopAnchor),

                _toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _helpLabel.HeightAnchor.ConstraintEqualTo(25)
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
            _addButton.Clicked += AddClick;
            _doneButton.Clicked += DoneClick;
            _saveButton.Clicked += Save_Click;
            _resetButton.Clicked += Reset_Click;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _addButton.Clicked -= AddClick;
            _doneButton.Clicked -= DoneClick;
            _saveButton.Clicked -= Save_Click;
            _resetButton.Clicked -= Reset_Click;
        }
    }
}