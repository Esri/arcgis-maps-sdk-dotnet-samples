// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using ArcGISRuntime.Samples.Managers;
using CoreGraphics;

namespace ArcGISRuntimeXamarin.Samples.SymbolsFromMobileStyle
{
    [Register("SymbolsFromMobileStyle")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Read symbols from a mobile style",
        "Symbology",
        "Open a local mobile style file (.stylx) and read its contents.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("1bd036f221f54a99abc9e46ff3511cbf")]
    public class SymbolsFromMobileStyle : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

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
            _myMapView.GeoViewTapped += GeoViewTapped;

            // Get the full path to the downloaded mobile style file (.stylx).
            string mobileStyleFilePath = DataManager.GetDataFolder("1bd036f221f54a99abc9e46ff3511cbf", "emoji-mobile.stylx");

            // Create the dialog for selecting symbol layers from the style.
            _selectSymbolController = new SymbolSelectionViewController(mobileStyleFilePath);
        }

        private void GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            MultilayerPointSymbol faceSymbol = _selectSymbolController.SelectedSymbol;
            Graphic tapGraphic = new Graphic(e.Location, faceSymbol);
            _myMapView.GraphicsOverlays.First().Graphics.Add(tapGraphic);
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = UIColor.White };

            _myMapView = new MapView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

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

            // Add the views.
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

        private void ClearGraphicsClicked(object sender, EventArgs e)
        {
            _myMapView.GraphicsOverlays.First()?.Graphics.Clear();
        }

        private void SelectSymbolClicked(object sender, EventArgs e)
        {
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
                pc.Delegate = new PpDelegate();
            }

            PresentViewController(controller, true, null);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        // Force popover to display on iPhone.
        private class PpDelegate : UIPopoverPresentationControllerDelegate
        {
            public override UIModalPresentationStyle GetAdaptivePresentationStyle(
                UIPresentationController forPresentationController) => UIModalPresentationStyle.None;

            public override UIModalPresentationStyle GetAdaptivePresentationStyle(UIPresentationController controller,
                UITraitCollection traitCollection) => UIModalPresentationStyle.None;
        }
    }

    public class SymbolSelectionViewController : UIViewController
    {
        public MultilayerPointSymbol SelectedSymbol { get; private set; }

        // Hold references to the UI controls.
        private UIPickerView _symbolLayersPicker;
        private UISegmentedControl _colorSegments;
        private List<System.Drawing.Color> _colorList;
        private UISlider _sizeSlider;
        private MultiSymbolPickerModel _symbolLayersPickerModel;
        private UIStackView _outerStackView;
        private UIImageView _symbolPreviewImageView;
        private readonly string _mobileStyleFilePath;
        private SymbolStyle _emojiStyle;
        private readonly string _baseSymbolKey = "Face1";

        private List<SymbolLayerInfo> _eyeSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };
        private List<SymbolLayerInfo> _mouthSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };
        private List<SymbolLayerInfo> _hatSymbolInfos = new List<SymbolLayerInfo> { new SymbolLayerInfo("", null, "") };

        public SymbolSelectionViewController(string stylxPath)
        {
            Title = "Select symbol layers";

            _mobileStyleFilePath = stylxPath;

            FillSymbolPickers();
        }

        private async Task FillSymbolPickers()
        {
            await ReadMobileStyle();

            _symbolLayersPicker = new UIPickerView();
            _symbolLayersPickerModel = new MultiSymbolPickerModel(_eyeSymbolInfos, _mouthSymbolInfos, _hatSymbolInfos);
            _symbolLayersPickerModel.SymbolSelected += async (sender, e) => await UpdateSymbol();
            _symbolLayersPicker.Model = _symbolLayersPickerModel;

            _colorList = new List<System.Drawing.Color>
            {
                System.Drawing.Color.Yellow,
                System.Drawing.Color.LightGreen,
                System.Drawing.Color.Pink
            };

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

            _colorSegments.ValueChanged += (sender, e) =>
            {
                UpdateSymbol();
            };

            _colorSegments.TranslatesAutoresizingMaskIntoConstraints = false;
            _colorSegments.SelectedSegment = 0;
            _colorSegments.Frame = new CGRect(0, 0, 400, 20);

            _sizeSlider = new UISlider
            {
                MinValue = 8,
                MaxValue = 60,
                Value = 20
            };

            // Symbol preview image view.
            _symbolPreviewImageView = new UIImageView(new CoreGraphics.CGRect(0, 0, 80, 80))
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            await UpdateSymbol();
        }

        private async Task UpdateSymbol()
        {
            SelectedSymbol = await GetCurrentSymbol();
            if (SelectedSymbol != null)
            {
                RuntimeImage swatch = await SelectedSymbol.CreateSwatchAsync(80, 80, 96, System.Drawing.Color.White);
                UIImage symbolImage = await swatch.ToImageSourceAsync();
                _symbolPreviewImageView.Image = symbolImage;
            }
        }

        private async Task ReadMobileStyle()
        {
            // Open a mobile style file.
            _emojiStyle = await Esri.ArcGISRuntime.Symbology.SymbolStyle.OpenAsync(_mobileStyleFilePath);

            // Get the default style search parameters.
            SymbolStyleSearchParameters searchParams = await _emojiStyle.GetDefaultSearchParametersAsync();

            // Search the style with the default parameters to return all symbol results.
            IList<SymbolStyleSearchResult> styleResults = await _emojiStyle.SearchSymbolsAsync(searchParams);


            // Loop through the results and put symbols into the appropriate list according to category.
            foreach (SymbolStyleSearchResult result in styleResults)
            {
                MultilayerPointSymbol multiLayerSym = result.Symbol as MultilayerPointSymbol;
                RuntimeImage swatch = await multiLayerSym.CreateSwatchAsync();
                UIImage symbolImage = await swatch.ToImageSourceAsync();

                switch (result.Category)
                {
                    case "Eyes":
                        {
                            _eyeSymbolInfos.Add(new SymbolLayerInfo(result.Name, symbolImage, result.Key));
                            break;
                        }
                    case "Mouth":
                        {
                            _mouthSymbolInfos.Add(new SymbolLayerInfo(result.Name, symbolImage, result.Key));
                            break;
                        }
                    case "Hat":
                        {
                            _hatSymbolInfos.Add(new SymbolLayerInfo(result.Name, symbolImage, result.Key));
                            break;
                        }
                    case "Face":
                        {
                            break;
                        }
                }
            }
        }


        private async Task<MultilayerPointSymbol> GetCurrentSymbol()
        {
            if (_emojiStyle == null) { return null; }

            List<string> symbolKeys = new List<string>
            {
                _baseSymbolKey,
                _symbolLayersPickerModel.SelectedSymbolKey1,
                _symbolLayersPickerModel.SelectedSymbolKey2, 
                _symbolLayersPickerModel.SelectedSymbolKey3
            };

            MultilayerPointSymbol faceSymbol = await _emojiStyle.GetSymbolAsync(symbolKeys) as MultilayerPointSymbol;

            // Loop through all symbol layers and lock the color.
            foreach (SymbolLayer lyr in faceSymbol.SymbolLayers)
            {
                // Changing the color of the symbol will not affect this layer.
                lyr.IsColorLocked = true;
            }

            // Unlock the color for the base (first) layer. Changing the symbol color will change this layer's color.
            faceSymbol.SymbolLayers.First().IsColorLocked = false;

            // Set the symbol color using the selection.
            System.Drawing.Color selectedUIColor = _colorList[(int)_colorSegments.SelectedSegment];

            faceSymbol.Color = selectedUIColor; 

            // Set the symbol size from the slider.
            faceSymbol.Size = _sizeSlider.Value;

            return faceSymbol;
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = UIColor.White };

            // Stack view to contain all the controls.
            _outerStackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Spacing = 2,
                Distribution = UIStackViewDistribution.EqualSpacing,
                Alignment = UIStackViewAlignment.Center
            };

            // Stack view to contain the pickers (symbol layers and color).
            UIStackView pickersView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                TranslatesAutoresizingMaskIntoConstraints = false,
                Distribution = UIStackViewDistribution.EqualSpacing,
                Alignment = UIStackViewAlignment.Top
            };

            pickersView.AddArrangedSubview(_symbolLayersPicker);

            _outerStackView.AddArrangedSubview(pickersView);
           // _outerStackView.AddArrangedSubview(_colorSegments);

            UILabel sizeLabel = new UILabel
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Text = $"Size: {_sizeSlider.Value:0}"
            };

            _sizeSlider.TranslatesAutoresizingMaskIntoConstraints = false;
            _sizeSlider.ValueChanged += async (sender, e) => 
            {
                sizeLabel.Text = $"Size: {_sizeSlider.Value:0}";
                await UpdateSymbol(); 
            };
           // _sizeSlider.Frame = new CGRect(0, 0, 400, 30);
            UIStackView sizeStack = new UIStackView(new UIView[] { _sizeSlider })
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Horizontal,
                Alignment = UIStackViewAlignment.Center,
                Distribution = UIStackViewDistribution.Fill
            };

            UIStackView colorSizeStack = new UIStackView(new UIView[] { _colorSegments, sizeStack, sizeLabel })
            {
                TranslatesAutoresizingMaskIntoConstraints = false,
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Center,
                Distribution = UIStackViewDistribution.Fill
            };

            _outerStackView.AddArrangedSubview(colorSizeStack);
            _outerStackView.AddArrangedSubview(_symbolPreviewImageView);
            UIView spacer = new UIView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            spacer.SetContentHuggingPriority((float)UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Vertical);
            spacer.SetContentHuggingPriority((float)UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Horizontal);
            _outerStackView.AddArrangedSubview(spacer);

            // Add the views.
            View.AddSubview(_outerStackView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _outerStackView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 8),
                _outerStackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -8),
                _outerStackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8),
                _outerStackView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor, -8)
            });
        }

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

    public class SymbolLayerInfo
    {
        public UIImage Image { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }

        public SymbolLayerInfo(string name, UIImage image, string key)
        {
            Name = name;
            Image = image;
            Key = key;
        }
    }

    // Class that defines a view model for showing three symbol categories in a picker control.
    public class MultiSymbolPickerModel : UIPickerViewModel
    {
        // Event raised when the selected symbol changes.
        public event EventHandler SymbolSelected;

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

        // Property to expose the currently selected red value in the picker.
        public string SelectedSymbolKey1 { get; private set; } = "";

        // Property to expose the currently selected green value in the picker.
        public string SelectedSymbolKey2 { get; private set; } = "";

        // Property to expose the currently selected blue value in the picker.
        public string SelectedSymbolKey3 { get; private set; } = "";

        // Return the number of picker components (three sections: red, green, and blue values).
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

        public override UIView GetView(UIPickerView pickerView, nint row, nint component, UIView view)
        {
            int idx = Convert.ToInt32(row);
            UIImage image = null;
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

            return new UIImageView(image);
        }

        // Handle the selection event for the picker.
        public override void Selected(UIPickerView pickerView, nint row, nint component)
        {
            // Get the selected symbol key values.
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
