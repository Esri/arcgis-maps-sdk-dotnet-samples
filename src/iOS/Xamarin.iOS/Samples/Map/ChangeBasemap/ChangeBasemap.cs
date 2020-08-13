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
        name: "Change basemap",
        category: "Map",
        description: "Change a map's basemap. A basemap is beneath all layers on a `Map` and is used to provide visual reference for the operational layers.",
        instructions: "Use the drop down menu to select the active basemap from the list of available basemaps.",
        tags: new[] { "basemap", "map" })]
    public class ChangeBasemap : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _changeBasemapButton;

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
                popoverPresentationController.BarButtonItem = (UIBarButtonItem) sender;
            }

            // Show the alert.
            PresentViewController(basemapSelectionAlert, true, null);
        }

        private void Initialize()
        {
            // Create and use a new map.
            _myMapView.Map = new Map(_basemapOptions.Values.First());
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _changeBasemapButton = new UIBarButtonItem();
            _changeBasemapButton.Title = "Change basemap";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _changeBasemapButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
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

                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _changeBasemapButton.Clicked += BasemapSelectionButtonClick;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _changeBasemapButton.Clicked -= BasemapSelectionButtonClick;
        }
    }
}