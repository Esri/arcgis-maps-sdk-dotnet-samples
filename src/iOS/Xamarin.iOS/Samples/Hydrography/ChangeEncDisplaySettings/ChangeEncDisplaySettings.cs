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
using ArcGISRuntime;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Hydrography;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ChangeEncDisplaySettings
{
    [Register("ChangeEncDisplaySettings")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Change ENC display settings",
        category: "Hydrography",
        description: "Configure the display of ENC content.",
        instructions: "The sample displays an electronic navigational chart when it opens. Use the options to choose variations on colors and symbology.",
        tags: new[] { "ENC", "IHO", "S-52", "S-57", "display", "hydrographic", "hydrography", "layers", "maritime", "nautical chart", "settings", "symbology" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9d2987a825c646468b3ce7512fb76e2d")]
    public class ChangeEncDisplaySettings : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _colorsButton;
        private UIBarButtonItem _areasButton;
        private UIBarButtonItem _pointsButton;

        // Hold a reference to the (static) app-wide ENC Mariner settings
        private readonly EncMarinerSettings _encMarinerSettings =
            EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings;

        public ChangeEncDisplaySettings()
        {
            Title = "ENC Display Settings";
        }

        private async void Initialize()
        {
            // Initialize the map with an oceans basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISOceans);

            // Get the path to the ENC Exchange Set.
            string encPath = DataManager.GetDataFolder("9d2987a825c646468b3ce7512fb76e2d", "ExchangeSetwithoutUpdates",
                "ENC_ROOT", "CATALOG.031");

            // Create the Exchange Set.
            // Note: this constructor takes an array of paths because so that update sets can be loaded alongside base data.
            EncExchangeSet encExchangeSet = new EncExchangeSet(encPath);

            try
            {
                // Wait for the layer to load.
                await encExchangeSet.LoadAsync();

                // Store a list of data set extent's - will be used to zoom the mapview to the full extent of the Exchange Set.
                List<Envelope> dataSetExtents = new List<Envelope>();

                // Add each data set as a layer.
                foreach (EncDataset encDataSet in encExchangeSet.Datasets)
                {
                    EncLayer encLayer = new EncLayer(new EncCell(encDataSet));

                    // Add the layer to the map.
                    _myMapView.Map.OperationalLayers.Add(encLayer);

                    // Wait for the layer to load.
                    await encLayer.LoadAsync();

                    // Add the extent to the list of extents.
                    dataSetExtents.Add(encLayer.FullExtent);
                }

                // Use the geometry engine to compute the full extent of the ENC Exchange Set.
                Envelope fullExtent = GeometryEngine.CombineExtents(dataSetExtents);

                // Set the viewpoint.
                _myMapView.SetViewpoint(new Viewpoint(fullExtent));
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        public override void ViewWillDisappear(bool animated)
        {
            base.ViewWillDisappear(animated);

            // ENC environment settings apply to the entire application.
            // They need to be reset after leaving the sample to avoid affecting other samples.
            EncEnvironmentSettings.Default.DisplaySettings.MarinerSettings.ResetToDefaults();
            EncEnvironmentSettings.Default.DisplaySettings.ViewingGroupSettings.ResetToDefaults();
            EncEnvironmentSettings.Default.DisplaySettings.TextGroupVisibilitySettings.ResetToDefaults();
        }

        private void ColorSettingsClicked(object sender, EventArgs e)
        {
            // Create the alert controller with a title.
            UIAlertController alertController =
                UIAlertController.Create("Choose a color scheme", "", UIAlertControllerStyle.Alert);

            // Actions can be default, cancel, or destructive
            alertController.AddAction(UIAlertAction.Create("Day", UIAlertActionStyle.Default,
                action => _encMarinerSettings.ColorScheme = EncColorScheme.Day));
            alertController.AddAction(UIAlertAction.Create("Dusk", UIAlertActionStyle.Default,
                action => _encMarinerSettings.ColorScheme = EncColorScheme.Dusk));
            alertController.AddAction(UIAlertAction.Create("Night", UIAlertActionStyle.Default,
                action => _encMarinerSettings.ColorScheme = EncColorScheme.Night));

            // Show the alert.
            PresentViewController(alertController, true, null);
        }

        private void AreaSettingsClicked(object sender, EventArgs e)
        {
            // Create the alert controller with a title.
            UIAlertController alertController =
                UIAlertController.Create("Choose how areas will be shown", "", UIAlertControllerStyle.Alert);

            // Actions can be default, cancel, or destructive
            alertController.AddAction(UIAlertAction.Create("Plain", UIAlertActionStyle.Default, action =>
                _encMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Plain));
            alertController.AddAction(UIAlertAction.Create("Symbolized", UIAlertActionStyle.Default, action =>
                _encMarinerSettings.AreaSymbolizationType = EncAreaSymbolizationType.Symbolized));

            // Show the alert.
            PresentViewController(alertController, true, null);
        }

        private void PointSettingsClicked(object sender, EventArgs e)
        {
            // Create the alert controller with a title.
            UIAlertController alertController =
                UIAlertController.Create("Choose how points will be shown", "", UIAlertControllerStyle.Alert);

            // Actions can be default, cancel, or destructive
            alertController.AddAction(UIAlertAction.Create("Paper chart", UIAlertActionStyle.Default, action =>
                _encMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.PaperChart));
            alertController.AddAction(UIAlertAction.Create("Simplified", UIAlertActionStyle.Default, action =>
                _encMarinerSettings.PointSymbolizationType = EncPointSymbolizationType.Simplified));

            // Show the alert.
            PresentViewController(alertController, true, null);
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

            _colorsButton = new UIBarButtonItem();
            _colorsButton.Title = "Colors";

            _areasButton = new UIBarButtonItem();
            _areasButton.Title = "Areas";

            _pointsButton = new UIBarButtonItem();
            _pointsButton.Title = "Points";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _colorsButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _areasButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _pointsButton
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
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _colorsButton.Clicked += ColorSettingsClicked;
            _areasButton.Clicked += AreaSettingsClicked;
            _pointsButton.Clicked += PointSettingsClicked;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _colorsButton.Clicked -= ColorSettingsClicked;
            _areasButton.Clicked -= AreaSettingsClicked;
            _pointsButton.Clicked -= PointSettingsClicked;
        }
    }
}