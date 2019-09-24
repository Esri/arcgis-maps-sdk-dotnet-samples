// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using AVFoundation;
using Esri.ArcGISRuntime.ARToolkit;
using Esri.ArcGISRuntime.Location;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Navigation;
using Esri.ArcGISRuntime.Tasks.NetworkAnalysis;
using Esri.ArcGISRuntime.UI;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.NavigateAR
{
    public class RouteViewerAR : UIViewController
    {
        private ARSceneView _arView;
        private UILabel _helpLabel;
        private UIBarButtonItem _calibrateButton;
        private UIBarButtonItem _navigateButton;

        public RouteResult _routeResult;
        public RouteTask _routeTask;
        public RouteParameters _routeParameters;
        private Route _currentRoute;
        private RouteTracker _routeTracker;
        private SystemLocationDataSource _trackingLocationDataSource;
        private AVSpeechSynthesizer _synthesizer;

        private GraphicsOverlay _routeOverlay;
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        private bool _isCalibrating = false;
        private bool IsCalibrating {
            get => _isCalibrating;
            set
            {
                _isCalibrating = value;
                if (_isCalibrating)
                {
                    _arView.Scene.BaseSurface.Opacity = 0.5;
                }
                else
                {
                    _arView.Scene.BaseSurface.Opacity = 0;
                }
                
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View = new UIView { BackgroundColor = UIColor.White };

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            _arView = new ARSceneView();
            _arView.TranslatesAutoresizingMaskIntoConstraints = false;

            _helpLabel = new UILabel();
            _helpLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _helpLabel.TextAlignment = UITextAlignment.Center;
            _helpLabel.TextColor = UIColor.White;
            _helpLabel.BackgroundColor = UIColor.FromWhiteAlpha(0, 0.6f);
            _helpLabel.Text = "Adjust calibration before starting";

            _calibrateButton = new UIBarButtonItem("Calibrate", UIBarButtonItemStyle.Plain, null);
            _navigateButton = new UIBarButtonItem("Navigate", UIBarButtonItemStyle.Plain, StartTurnByTurn);

            toolbar.Items = new[]
            {
                _calibrateButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _navigateButton
            };

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _arView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _arView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _arView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _arView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                _helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _helpLabel.HeightAnchor.ConstraintEqualTo(40)
            }); ;

            Initialize();
        }

        private void Initialize()
        {

        }

        private void StartTurnByTurn(object sender, EventArgs e)
        {
            _routeTracker = new RouteTracker(_routeResult, 0);

            if (_routeTracker.IsReroutingEnabled)
            {
                _routeTracker.EnableReroutingAsync(_routeTask, _routeParameters, ReroutingStrategy.ToNextStop, true);
            }

            _routeTracker.NewVoiceGuidance += (o,ee) => {

            };

            _routeTracker.RerouteCompleted += (o, ee) =>
            {

            };

            _routeTracker.RerouteStarted += (o, ee) =>
            {

            };

            _routeTracker.TrackingStatusChanged += (o, ee) =>
            {

            };
        }

        public override void ViewDidAppear(bool animated)
        {
            base.ViewDidAppear(animated);
            _arView.StartTrackingAsync();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            _arView.StopTracking();
        }
    }
}
