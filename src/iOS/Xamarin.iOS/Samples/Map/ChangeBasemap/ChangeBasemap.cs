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
using System.Linq;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeBasemap
{
    [Register("ChangeBasemap")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change basemap",
        "Map",
        "This sample demonstrates how to dynamically change the basemap displayed in a Map.",
        "")]
    public class ChangeBasemap : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _myMapView;

        // Dictionary that associates names with basemaps.
        private readonly Dictionary<string, Basemap> _basemapOptions = new Dictionary<string, Basemap>
        {
            {"Streets (Raster)", Basemap.CreateStreets()},
            {"Streets (Vector)", Basemap.CreateStreetsVector()},
            {"Streets - Night (Vector)", Basemap.CreateStreetsNightVector()},
            {"Imagery (Raster)", Basemap.CreateImagery()},
            {"Imagery with Labels (Raster)", Basemap.CreateImageryWithLabels()},
            {"Imagery with Labels (Vector)", Basemap.CreateImageryWithLabelsVector()},
            {"Dark Gray Canvas (Vector)", Basemap.CreateDarkGrayCanvasVector()},
            {"Light Gray Canvas (Raster)", Basemap.CreateLightGrayCanvas()},
            {"Light Gray Canvas (Vector)", Basemap.CreateLightGrayCanvasVector()},
            {"Navigation (Vector)", Basemap.CreateNavigationVector()},
            {"OpenStreetMap (Raster)", Basemap.CreateOpenStreetMap()}
        };

        public ChangeBasemap()
        {
            Title = "Change basemap";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void BasemapSelectionButtonClick(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of basemaps.
            UIAlertController basemapSelectionAlert = UIAlertController.Create("Select a basemap", "", UIAlertControllerStyle.ActionSheet);

            // Add an option for each basemap.
            foreach (string item in _basemapOptions.Keys)
            {
                // Selecting a basemap will call the lambda method, which will apply the chosen basemap.
                basemapSelectionAlert.AddAction(UIAlertAction.Create(item, UIAlertActionStyle.Default, action => _myMapView.Map.Basemap = _basemapOptions[item]));
            }

            // Fix to prevent crash on iPad.
            var popoverPresentationController = basemapSelectionAlert.PopoverPresentationController;
            if (popoverPresentationController != null)
            {
                popoverPresentationController.SourceView = (UIButton) sender;
            }

            // Show the alert.
            PresentViewController(basemapSelectionAlert, true, null);
        }

        private void Initialize()
        {
            // Create and use a new map.
            _myMapView.Map = new Map(_basemapOptions.Values.First());
        }

        public override void LoadView()
        {
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            View.AddSubviews(_myMapView, toolbar);

            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem("Change basemap", UIBarButtonItemStyle.Plain, BasemapSelectionButtonClick),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor).Active = true;

            toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor).Active = true;
            toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }
    }
}