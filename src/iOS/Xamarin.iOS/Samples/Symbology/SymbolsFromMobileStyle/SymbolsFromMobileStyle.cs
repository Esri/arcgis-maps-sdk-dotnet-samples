// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArcGISRuntime.Samples.Managers;
using CoreGraphics;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.SymbolsFromMobileStyle
{
    [Register("SymbolsFromMobileStyle")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Read symbols from mobile style",
        category: "Symbology",
        description: "Combine multiple symbols from a mobile style file into a single symbol.",
        instructions: "Select a symbol and a color from each of the category lists to create an emoji. A preview of the symbol is updated as selections are made. The size of the symbol can be set using the slider. Tap the map to create a point graphic using the customized emoji symbol, and tap \"Reset\" to clear all graphics from the display.",
        tags: new[] { "advanced symbology", "mobile style", "multilayer", "stylx" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("1bd036f221f54a99abc9e46ff3511cbf")]
    public class SymbolsFromMobileStyle : UIViewController
    {
        // A map view to display a map and graphics.
        private MapView _myMapView;

        // A dialog (popover) to show available symbols from a mobile style.
        private SymbolSelectionViewController _selectSymbolController;

        public SymbolsFromMobileStyle()
        {
            Title = "Read symbols from a mobile style";
        }

        private void Initialize()
        {
            // Create new Map with a topographic basemap.
            Map myMap = new Map(Basemap.CreateTopographic());

            // Display the map in the map view.
            _myMapView.Map = myMap;

            // Create an overlay to display point graphics and add it to the map view.
            GraphicsOverlay overlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(overlay);

            // Handle the tap event on the map view to allow the user to place graphics.
            _myMapView.GeoViewTapped += OnGeoViewTapped;

            // Get the full path to the downloaded mobile style file (.stylx).
            string mobileStyleFilePath = DataManager.GetDataFolder("1bd036f221f54a99abc9e46ff3511cbf", "emoji-mobile.stylx");

            // Create the dialog for selecting symbol layers from the style.
            _selectSymbolController = new SymbolSelectionViewController(mobileStyleFilePath);
        }

        // A handler for the GeoViewTapped event
        private void OnGeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Get the current symbol from the symbol selector dialog.
            MultilayerPointSymbol faceSymbol = _selectSymbolController.SelectedSymbol;

            // Create a graphic using the tap location and the selected symbol.
            Graphic tapGraphic = new Graphic(e.Location, faceSymbol);

            // Add the grapic to the first (and only) graphics overlay in the map view.
            _myMapView.GraphicsOverlays.First().Graphics.Add(tapGraphic);
        }

        public override void LoadView()
        {
            // Create the user interface.
            View = new UIView { BackgroundColor = UIColor.White };

            // Create a new map view control.
            _myMapView = new MapView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Create a toolbar and add controls for showing the symbol selector and clearing graphics from the map view.
            UIToolbar toolbar = new UIToolbar
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Items = new[]
                {
                    new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                    new UIBarButtonItem("Choose symbol", UIBarButtonItemStyle.Plain, SelectSymbolClicked),
                    new UIBarButtonItem("Clear", UIBarButtonItemStyle.Plain, ClearGraphicsClicked)
                }
            };

            // Add the map view and toolbar to the main UI.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }

        // A handler for the "clear" button click event.
        private void ClearGraphicsClicked(object sender, EventArgs e)
        {
            // Clear all graphics from the first (and only) graphics overlay in the map view.
            _myMapView.GraphicsOverlays.First()?.Graphics.Clear();
        }

        // A handler for the "choose symbol" button click event.
        private void SelectSymbolClicked(object sender, EventArgs e)
        {
            // Create a navigation controller to show the symbol selection dialog as a popover.
            UINavigationController controller = new UINavigationController(_selectSymbolController)
            {
                ModalPresentationStyle = UIModalPresentationStyle.Popover,
                PreferredContentSize = new CGSize(320, 400)
            };
            UIPopoverPresentationController pc = controller.PopoverPresentationController;
            if (pc != null)
            {
                pc.BarButtonItem = (UIBarButtonItem)sender;
                pc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
                pc.Delegate = new PopoverDelegate();
            }

            // Show the symbol selection popover.
            PresentViewController(controller, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Initialize the map and UI controls.
            Initialize();
        }

        // A custom delegate class to force a popover to display on iPhone.
        private class PopoverDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }
    }

    // A view controller that displays available symbols from a mobile style for creating a multilayer point symbol.
    public class SymbolSelectionViewController : UIViewController
    {
        // A public property to expose the selected symbol.
        public MultilayerPointSymbol SelectedSymbol { get; private set; }

        // The stack view that contains the symbol selection controls.
        private UIStackView _outerStackView;

        // A picker and model for selecting symbol layers from three categories: eyes, mouth, and hat.
        private UIPickerView _symbolLayersPicker;
        private MultiSymbolPickerModel _symbolLayersPickerModel;

        // A segmented control and color list for selecting one of three colors for the background symbol (face).
        private UISegmentedControl _colorSegments;
        private List<System.Drawing.Color> _colorList;

        // A slider for specifying the symbol size.
        private UISlider _sizeSlider;

        // An image view to show a preview of the currently designed symbol.
        private UIImageView _symbolPreviewImageView;

        // The file path to the mobile style file (.stylx) and the symbol style object.
        private readonly string _mobileStyleFilePath;
        private SymbolStyle _emojiStyle;

        // The unique identifier (key) for the background face symbol in the mobile style.
        private readonly string _baseSymbolKey = "Face1";

        // Lists that contain the available symbol layers for each category of symbol.
        private List<SymbolLayerInfo> _eyeSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };
        private List<SymbolLayerInfo> _mouthSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };
        private List<SymbolLayerInfo> _hatSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };

        // Take the file path to the mobile style file in the constructor.
        public SymbolSelectionViewController(string stylxPath)
        {
            Title = "Select symbol layers";

            // Store the path to the mobile style file.
            _mobileStyleFilePath = stylxPath;

            // Call a function that will fill the symbol lists for each category and show them in the picker control.
            FillSymbolControls();
        }

        // A function to set up the symbol controls with choices for the user: lists of symbol layers, colors, size slider.
        private async void FillSymbolControls()
        {
            // Call a function to read the style file and build lists of symbols for each category.
            await ReadMobileStyle();

            // Create a picker control to show the available symbol layers.
            _symbolLayersPicker = new UIPickerView();

            // Create an instance of a custom picker model that takes the category lists of available symbols.
            _symbolLayersPickerModel = new MultiSymbolPickerModel(_eyeSymbolInfos, _mouthSymbolInfos, _hatSymbolInfos);

            // Handle the selection event for symbols in the picker by updating the current symbol.
            _symbolLayersPickerModel.SymbolSelected += async (sender, e) => await UpdateSymbol();

            // Assign the symbol layers picker model to the picker control.
            _symbolLayersPicker.Model = _symbolLayersPickerModel;

            // Create a list of the available colors for the symbol.
            _colorList = new List<System.Drawing.Color>
            {
                System.Drawing.Color.Yellow,
                System.Drawing.Color.LightGreen,
                System.Drawing.Color.Pink
            };

            // Create a segmented control to display each available color as a button.
            _colorSegments = new UISegmentedControl();
            _colorSegments.InsertSegment("Yellow", 0, false);
            _colorSegments.Subviews[0].BackgroundColor = UIColor.Yellow;
            _colorSegments.Subviews[0].TintColor = UIColor.Yellow;
            _colorSegments.InsertSegment("Green", 1, false);
            _colorSegments.Subviews[1].BackgroundColor = UIColor.Green;
            _colorSegments.Subviews[1].TintColor = UIColor.Green;
            _colorSegments.InsertSegment("Pink", 2, false);
            _colorSegments.Subviews[2].BackgroundColor = UIColor.FromRGB(255,192,203);
            _colorSegments.Subviews[2].TintColor = UIColor.FromRGB(255, 192, 203);

            // Set the color segments text color to black so it's more visible.
            UITextAttributes textAttributes = new UITextAttributes
            {
                TextColor = UIColor.Black
            };
            _colorSegments.SetTitleTextAttributes(textAttributes, UIControlState.Normal);

            // Handle the value changed event for the segmented control to update the current symbol.
            _colorSegments.ValueChanged += async(sender, e) =>
            {
                await UpdateSymbol();
            };

            _colorSegments.TranslatesAutoresizingMaskIntoConstraints = false;
            _colorSegments.SelectedSegment = 0;
            _colorSegments.Frame = new CGRect(0, 0, 400, 20);

            // Create a slider control for specifying the symbol size. Restrict values to be between 8 and 60.
            _sizeSlider = new UISlider
            {
                MinValue = 8,
                MaxValue = 60,
                Value = 20
            };

            // Create an image view to show a preview of the current symbol.
            _symbolPreviewImageView = new UIImageView(new CoreGraphics.CGRect(0, 0, 80, 80))
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };


            // Update the current symbol (will be the default yellow circle if this is the first time the popover was opened).
            await UpdateSymbol();
        }

        // A function to update the current multilayer symbol based on selections in the dialog.
        private async Task UpdateSymbol()
        {
            // Call a function to read the current settings and create the appropriate symbol. 
            // Assign the symbol to the public SelectedSymbol property.
            SelectedSymbol = await GetCurrentSymbol();
            if (SelectedSymbol != null)
            {
                // Create an image of the symbol swatch to display as a preview.
                RuntimeImage swatch = await SelectedSymbol.CreateSwatchAsync(80, 80, 96, System.Drawing.Color.White);
                UIImage symbolImage = await swatch.ToImageSourceAsync();
                _symbolPreviewImageView.Image = symbolImage;
            }
        }

        // A function that reads a mobile style file and builds a list for each of three symbol categories.
        private async Task ReadMobileStyle()
        {
            // Open a mobile style file.
            _emojiStyle = await SymbolStyle.OpenAsync(_mobileStyleFilePath);

            // Get the default style search parameters.
            SymbolStyleSearchParameters searchParams = await _emojiStyle.GetDefaultSearchParametersAsync();

            // Search the style with the default parameters to return all symbol results.
            IList<SymbolStyleSearchResult> styleResults = await _emojiStyle.SearchSymbolsAsync(searchParams);

            // Loop through the results and put symbols into the appropriate list according to category.
            foreach (SymbolStyleSearchResult result in styleResults)
            {
                // Get the symbol from the result.
                MultilayerPointSymbol multiLayerSym = result.Symbol as MultilayerPointSymbol;

                // Create an image from the symbol swatch.
                RuntimeImage swatch = await multiLayerSym.CreateSwatchAsync();
                UIImage symbolImage = await swatch.ToImageSourceAsync();

                // Create an instance of the custom SymbolLayerInfo class to store info about this symbol: name, swatch image, unique ID (key).
                SymbolLayerInfo symbolInfo = new SymbolLayerInfo(result.Name, symbolImage, result.Key);

                // Check the category for this result and place it into the correct list.
                switch (result.Category)
                {
                    case "Eyes":
                        {
                            // 
                            _eyeSymbolInfos.Add(symbolInfo);
                            break;
                        }
                    case "Mouth":
                        {
                            _mouthSymbolInfos.Add(symbolInfo);
                            break;
                        }
                    case "Hat":
                        {
                            _hatSymbolInfos.Add(symbolInfo);
                            break;
                        }
                }
            }
        }

        // A function that reads all values in the dialog (selected layers, color, size) to build the multilayer point symbol.
        private async Task<MultilayerPointSymbol> GetCurrentSymbol()
        {
            // Create a list of keys that identify the selected symbol layers.
            List<string> symbolKeys = new List<string>
            {
                _baseSymbolKey,
                _symbolLayersPickerModel.SelectedSymbolKey1,
                _symbolLayersPickerModel.SelectedSymbolKey2, 
                _symbolLayersPickerModel.SelectedSymbolKey3
            };

            // Use the list of keys to return symbols from the mobile style (they will be combined and returned as a multilayer point symbol).
            MultilayerPointSymbol faceSymbol = await _emojiStyle.GetSymbolAsync(symbolKeys) as MultilayerPointSymbol;

            // Loop through all symbol layers and lock the color (changing the color property on the symbol will not affect these layers).
            foreach (SymbolLayer lyr in faceSymbol.SymbolLayers)
            {
                lyr.IsColorLocked = true;
            }

            // Unlock the color for the base (first) layer. Changing the symbol color will change this layer's color.
            faceSymbol.SymbolLayers.First().IsColorLocked = false;

            // Get the System.Drawing.Color from the color list that corresponds to the selected segment in the color control.
            System.Drawing.Color selectedUIColor = _colorList[(int)_colorSegments.SelectedSegment];

            // Set the symbol color using the selected color.
            faceSymbol.Color = selectedUIColor; 

            // Set the symbol size from the slider.
            faceSymbol.Size = _sizeSlider.Value;

            // Return the multilayer point symbol.
            return faceSymbol;
        }

        public override void LoadView()
        {
            // Create the UI for the symbol selection.
            View = new UIView { BackgroundColor = UIColor.White };

            // A vertical stack view to contain all the controls.
            _outerStackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 2,
                Distribution = UIStackViewDistribution.EqualSpacing,
                Alignment = UIStackViewAlignment.Center
            };

            // A vertical stack view to contain the symbol layer picker.
            UIStackView pickersView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Distribution = UIStackViewDistribution.EqualSpacing,
                Alignment = UIStackViewAlignment.Top
            };

            // Add the symbol layer picker to the stack view.
            pickersView.AddArrangedSubview(_symbolLayersPicker);

            // Add the picker stack view to the outer stack view.
            _outerStackView.AddArrangedSubview(pickersView);

            // Create a label to show the size of the symbol (specified with the slider).
            UILabel sizeLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = $"Size: {_sizeSlider.Value:0}"
            };

            // Handle the slider value changed event to update the label with the specified size.
            _sizeSlider.ValueChanged += async (sender, e) => 
            {
                sizeLabel.Text = $"Size: {_sizeSlider.Value:0}";
                await UpdateSymbol(); 
            };

            _sizeSlider.TranslatesAutoresizingMaskIntoConstraints = false;

            // Create a vertical stack view to contain the color segmented control, size slider, and size label.
            UIStackView colorSizeStack = new UIStackView(new UIView[] { _colorSegments, _sizeSlider, sizeLabel })
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Center,
                Distribution = UIStackViewDistribution.Fill
            };

            // Add the color and size stack view to the outer stack view.
            _outerStackView.AddArrangedSubview(colorSizeStack);

            // Add the preview image view to the outer stack view.
            _outerStackView.AddArrangedSubview(_symbolPreviewImageView);

            // Add the outer stack view to the main view.
            View.AddSubview(_outerStackView);

            // Set constraints on the outer stack view.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _outerStackView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 8),
                _outerStackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -8),
                _outerStackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8),
                _outerStackView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor, -8),
                _sizeSlider.LeftAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeftAnchor, 20),
                _sizeSlider.RightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.RightAnchor, -20)
            });
        }

        // Handle changes to display orientation.
        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                _outerStackView.Axis = UILayoutConstraintAxis.Horizontal;
            }
            else
            {
                _outerStackView.Axis = UILayoutConstraintAxis.Vertical;
            }
        }
    }

    // A class to contain information about a symbol layer.
    public class SymbolLayerInfo
    {
        // An image of the symbol.
        public UIImage Image;

        // The name of the symbol in the mobile style file.
        public string Name;

        // A unique key that identifies the symbol in the mobile style.
        public string Key;

        // Take all the symbol info property values in the constructor.
        public SymbolLayerInfo(string name, UIImage image, string key)
        {
            Name = name;
            Image = image;
            Key = key;
        }
    }

    // A class that defines a view model for showing three symbol categories in a picker control.
    public class MultiSymbolPickerModel : UIPickerViewModel
    {
        // Event raised when the selected symbol changes.
        public event EventHandler SymbolSelected;

        // Lists of symbol info for each symbol category.
        private List<SymbolLayerInfo> _symbolLayerItems1;
        private List<SymbolLayerInfo> _symbolLayerItems2;
        private List<SymbolLayerInfo> _symbolLayerItems3;

        // Constructor takes the three symbol info lists to display.
        public MultiSymbolPickerModel(List<SymbolLayerInfo> symbolItems1, List<SymbolLayerInfo> symbolItems2, List<SymbolLayerInfo> symbolItems3)
        {
            _symbolLayerItems1 = symbolItems1;
            _symbolLayerItems2 = symbolItems2;
            _symbolLayerItems3 = symbolItems3;
        }

        // Property to expose the unique key for the currently selected symbol in category one.
        public string SelectedSymbolKey1 { get; private set; } = "";

        // Property to expose the unique key for the currently selected symbol in category two.
        public string SelectedSymbolKey2 { get; private set; } = "";

        // Property to expose the unique key for the currently selected symbol in category three.
        public string SelectedSymbolKey3 { get; private set; } = "";

        // Return the number of picker components (three sections).
        public override nint GetComponentCount(UIPickerView pickerView)
        {
            return 3;
        }

        // Return the number of rows in each of the sections (number of symbols in each category).
        public override nint GetRowsInComponent(UIPickerView pickerView, nint component)
        {
            int rowCount = 0;
            switch (component)
            {
                case 0:
                    rowCount = _symbolLayerItems1.Count;
                    break;
                case 1:
                    rowCount = _symbolLayerItems2.Count;
                    break;
                case 2:
                    rowCount = _symbolLayerItems3.Count;
                    break;
            }

            return rowCount;
        }

        // Get the title to display in each picker component.
        public override string GetTitle(UIPickerView pickerView, nint row, nint component)
        {
            return "";
        }

        // Get the view to display for each component and row in the picker.
        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            int idx = Convert.ToInt32(row);
            UIImage image = null;

            // Get the symbol image for the current row and component (each component shows a different category).
            switch (component)
            {
                case 0:
                    image = _symbolLayerItems1[idx].Image;
                    break;
                case 1:
                    image = _symbolLayerItems2[idx].Image;
                    break;
                case 2:
                    image = _symbolLayerItems3[idx].Image;
                    break;
            }

            // Return the image view with the symbol image.
            return new UIImageView(image);
        }

        // Handle the selection event for the picker.
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            // Get the selected symbol key value for each category.
            SelectedSymbolKey1 = _symbolLayerItems1[(int)pickerView.SelectedRowInComponent(0)].Key;
            SelectedSymbolKey2 = _symbolLayerItems2[(int)pickerView.SelectedRowInComponent(1)].Key;
            SelectedSymbolKey3 = _symbolLayerItems3[(int)pickerView.SelectedRowInComponent(2)].Key;

            // Raise an event to notify the selection has changed.
            SymbolSelected?.Invoke(this, new EventArgs());
        }

        // Return the desired width for each component in the picker.
        public override nfloat GetComponentWidth(UIPickerView pickerView, nint component)
        {
            return 60f;
        }

        // Return the desired height for rows in the picker.
        public override nfloat GetRowHeight(UIPickerView pickerView, nint component)
        {
            return 30f;
        }
    }
}
