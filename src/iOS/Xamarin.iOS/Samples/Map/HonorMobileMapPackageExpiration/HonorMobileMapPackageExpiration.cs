﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.HonorMobileMapPackageExpiration
{
    [Register("HonorMobileMapPackageExpiration")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Honor mobile map package expiration date",
        "Map",
        "Access the expiration information of an expired mobile map package.",
        "",
        "Featured")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("174150279af74a2ba6f8b87a567f480b")]
    public class HonorMobileMapPackageExpiration : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UILabel _expirationLabel;

        public HonorMobileMapPackageExpiration()
        {
            Title = "Honor mobile map package expiration date";
        }

        private async void Initialize()
        {
            try
            {
                // Path to the mobile map package.
                string mobileMapPackagePath = DataManager.GetDataFolder("174150279af74a2ba6f8b87a567f480b", "LothianRiversAnno.mmpk");

                // Create a mobile map package.
                MobileMapPackage mobileMapPackage = new MobileMapPackage(mobileMapPackagePath);

                // Load the mobile map package.
                await mobileMapPackage.LoadAsync();

                // Check if the map package is expired.
                if (mobileMapPackage.Expiration?.IsExpired == true)
                {
                    // Get the expiration of the mobile map package.
                    Expiration expiration = mobileMapPackage.Expiration;

                    // Get the expiration message.
                    string expirationMessage = expiration.Message;

                    // Get the expiration date.
                    string expirationDate = expiration.DateTime.ToString("F");

                    // Set the expiration message.
                    _expirationLabel.Text = $"{expirationMessage}\nExpiration date: {expirationDate}";

                    // Check if the map is accessible after expiration.
                    if (expiration.Type == ExpirationType.AllowExpiredAccess && mobileMapPackage.Maps.Count > 0)
                    {
                        // Set the mapview to the map from the mobile map package.
                        _myMapView.Map = mobileMapPackage.Maps[0];
                    }
                    else if (expiration.Type == ExpirationType.PreventExpiredAccess)
                    {
                        new UIAlertView("Error", "The author of this mobile map package has disallowed access after the expiration date.", (IUIAlertViewDelegate)null, "OK", null).Show();
                    }
                }
                else if (mobileMapPackage.Maps.Any())
                {
                    // Set the mapview to the map from the mobile map package.
                    _myMapView.Map = mobileMapPackage.Maps[0];
                }
                else
                {
                    new UIAlertView("Error", "Failed to load the mobile map package.", (IUIAlertViewDelegate)null, "OK", null).Show();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _expirationLabel = new UILabel
            {
                Text = "Map package not expired.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 0,
                LineBreakMode = UILineBreakMode.WordWrap,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, _expirationLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                _expirationLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _expirationLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _expirationLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _expirationLabel.HeightAnchor.ConstraintEqualTo(80)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}