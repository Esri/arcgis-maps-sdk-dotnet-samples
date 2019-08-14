// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreImage;
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
using System.Linq;
using System.Reflection;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.CreateAndSaveKmlFile
{
    [Register("CreateAndSaveKmlFile")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Create and save KML file",
        "Layers",
        "Construct a KML document and save it as a KMZ file.",
        "")]
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

        // KML objects
        private KmlDocument _kmlDocument;

        private KmlDataset _kmlDataset;
        private KmlLayer _kmlLayer;
        private KmlPlacemark _currentPlacemark;

        public CreateAndSaveKmlFile()
        {
            Title = "Create and save KML file";
        }

        private void Initialize()
        {
            // Create the map.
            _myMapView.Map = new Map(Basemap.CreateImagery());

            ResetKml();
        }

        private void ResetKml()
        {
            // Clear any existing layers from the map.
            _myMapView.Map.OperationalLayers.Clear();

            // Reset the most recently placed placemark.
            _currentPlacemark = null;

            // Create a new KmlDocument.
            _kmlDocument = new KmlDocument() { Name = "KML Sample Document" };

            // Create a Kml dataset using the Kml document.
            _kmlDataset = new KmlDataset(_kmlDocument);

            // Create the kmlLayer using the kmlDataset.
            _kmlLayer = new KmlLayer(_kmlDataset);

            // Add the Kml layer to the map.
            _myMapView.Map.OperationalLayers.Add(_kmlLayer);
        }

        private void AddClick(object sender, EventArgs e)
        {
            // Decide what to add.
            UIAlertController prompt = UIAlertController.Create("Add facilities & barriers", "Tap to add facilities. Tap to build a polyline representing barriers. Press 'Done' to finish.", UIAlertControllerStyle.ActionSheet);
            prompt.AddAction(UIAlertAction.Create("Point", UIAlertActionStyle.Default, AddGeometry));
            prompt.AddAction(UIAlertAction.Create("Polyline", UIAlertActionStyle.Default, AddGeometry));
            prompt.AddAction(UIAlertAction.Create("Polygon", UIAlertActionStyle.Default, AddGeometry));

            // Needed to prevent crash on iPad.
            UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
            if (ppc != null)
            {
                ppc.BarButtonItem = (UIBarButtonItem)sender;
                ppc.PermittedArrowDirections = UIPopoverArrowDirection.Up;
            }

            PresentViewController(prompt, true, null);

            // Swap toolbar.
            _toolbar.Items = new[] { _doneButton };
        }

        private async void AddGeometry(UIAlertAction obj)
        {
            try
            {
                // Hide the base UI and enable the complete button.

                // Create variables for the sketch creation mode and color.
                SketchCreationMode creationMode;

                // Set the creation mode and UI based on which button called this method.
                switch (obj.Title)
                {
                    case "Point":
                        creationMode = SketchCreationMode.Point;
                        break;

                    case "Polyline":
                        creationMode = SketchCreationMode.Polyline;
                        break;

                    case "Polygon":
                        creationMode = SketchCreationMode.Polygon;
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

                // Choose whether to enable the icon picker or color picker.
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

                /*
                // Reset the UI.

                ShapesPanel.Visibility = Visibility.Visible;
                CompleteButton.Visibility = Visibility.Collapsed;
                InstructionsText.Text = "Select the type of feature you would like to add.";

                // Enable the save and reset buttons.
                SaveResetGrid.IsEnabled = true;
                */
            }
        }

        private void ChooseStyle()
        {
            //action.setValue(image, forKey: "image")
            // Start the UI for the user choosing the junction.
            if(_currentPlacemark.GraphicType == KmlGraphicType.Point)
            {
                UIAlertController prompt = UIAlertController.Create(null, "Choose icon.", UIAlertControllerStyle.ActionSheet);

                //foreach(UtilityTerminal terminal in terminals)
                List<string> iconLinks = new List<string>()
                {
                    "https://static.arcgis.com/images/Symbols/Shapes/BlueCircleLargeB.png",
                    "https://static.arcgis.com/images/Symbols/Shapes/BlueDiamondLargeB.png",
                    "https://static.arcgis.com/images/Symbols/Shapes/BluePin1LargeB.png",
                    "https://static.arcgis.com/images/Symbols/Shapes/BluePin2LargeB.png",
                    "https://static.arcgis.com/images/Symbols/Shapes/BlueSquareLargeB.png",
                    "https://static.arcgis.com/images/Symbols/Shapes/BlueStarLargeB.png"
                };
                foreach (string link in iconLinks)
                {
                    UIAlertAction action = UIAlertAction.Create(link, UIAlertActionStyle.Default, Icon_Select);
                    
                    UIImage image = new UIImage(NSData.FromUrl(new NSUrl(link)));
                    action.SetValueForKey(image, new NSString("image"));
                    prompt.AddAction(action);
                }

                // Needed to prevent crash on iPad.
                UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
                if (ppc != null)
                {
                    ppc.BarButtonItem = _doneButton;
                    ppc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                }

                PresentViewController(prompt, true, null);
            }
            else if(_currentPlacemark.GraphicType == KmlGraphicType.Polyline || _currentPlacemark.GraphicType == KmlGraphicType.Polygon)
            {
                UIAlertController prompt = UIAlertController.Create(null, "Choose color.", UIAlertControllerStyle.ActionSheet);
                List<UIColor> colorList = new List<UIColor> {
                    UIColor.Black,UIColor.Blue,UIColor.Brown,UIColor.Cyan,UIColor.DarkGray,UIColor.Gray,UIColor.Green,UIColor.LightGray,UIColor.Magenta,UIColor.Orange,UIColor.Purple,UIColor.Red,UIColor.White,UIColor.Yellow
                };

                foreach (UIColor color in colorList)
                {
                    UIAlertAction action = UIAlertAction.Create(color.ToString(), UIAlertActionStyle.Default, Color_Select);

                    action.SetValueForKey(color, new NSString("titleTextColor"));
                    prompt.AddAction(action);
                }

                // Needed to prevent crash on iPad.
                UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
                if (ppc != null)
                {
                    ppc.BarButtonItem = _doneButton;
                    ppc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                }

                PresentViewController(prompt, true, null);

            }
            

        }

        private void Icon_Select(UIAlertAction obj)
        {
            _currentPlacemark.Style = new KmlStyle();

            //UIImage image = obj.ValueForKey(new NSString("image")) as UIImage;

            Uri uri = new Uri(obj.Title);
            _currentPlacemark.Style.IconStyle = new KmlIconStyle(new KmlIcon(uri), 1.0);
        }
        private void Color_Select(UIAlertAction obj)
        {
            UIColor uiColor = obj.ValueForKey(new NSString("titleTextColor")) as UIColor;
            nfloat red, green, blue, alpha;
            uiColor.GetRGBA(out red, out green, out blue, out alpha);
            Color color = Color.FromArgb((int)(alpha*255), (int)(red * 255), (int)(green * 255), (int)(blue * 255));

            _currentPlacemark.Style = new KmlStyle();

            if (_currentPlacemark.GraphicType == KmlGraphicType.Polyline)
            {
                _currentPlacemark.Style.LineStyle = new KmlLineStyle(color, 8);
            }
            else if(_currentPlacemark.GraphicType == KmlGraphicType.Polygon)
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
            catch (Exception ex)
            {
                ShowMessage("Error", "File not saved.");
            }
        }

        private void ShowMessage(string title, string message)
        {
            // Create alert for the user.
            var okAlertController = UIAlertController.Create(title, message, UIAlertControllerStyle.Alert);
            okAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
            PresentViewController(okAlertController, true, null);
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = UIColor.White };

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

            // Add the views.
            View.AddSubviews(_myMapView, _toolbar);

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

    internal class ImageModel : UIPickerViewModel
    {
        public string[] names = new string[] {
            "Amy Burns",
            "Kevin Mullins",
            "Craig Dunn",
            "Joel Martinez",
            "Charles Petzold",
            "David Britch",
            "Mark McLemore",
            "Tom Opegenorth",
            "Joseph Hill",
            "Miguel De Icaza"
        };

        private UILabel personLabel;

        public ImageModel(UILabel personLabel)
        {
            this.personLabel = personLabel;
        }

        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 2;
        }

        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            return names.Length;
        }

        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            if (component == 0)
                return names[row];
            else
                return row.ToString();
        }

        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            personLabel.Text = $"This person is: {names[pickerView.SelectedRowInComponent(0)]},\n they are number {pickerView.SelectedRowInComponent(1)}";
        }

        public override nfloat GetComponentWidth(UIPickerView picker, nint component)
        {
            if (component == 0)
                return 240f;
            else
                return 40f;
        }

        public override nfloat GetRowHeight(UIPickerView picker, nint component)
        {
            return 40f;
        }
    }
}