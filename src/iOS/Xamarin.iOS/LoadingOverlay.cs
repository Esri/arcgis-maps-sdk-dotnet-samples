// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using System;
using UIKit;

namespace ArcGISRuntime
{
    /// <summary>
    /// View to show over the sample list while a sample with offline data is loading.
    /// </summary>
    public sealed class LoadingOverlay : UIView
    {
        public LoadingOverlay(CGRect frame) : base(frame)
        {
            BackgroundColor = UIColor.Black;
            Alpha = 0.8f;
            AutoresizingMask = UIViewAutoresizing.All;

            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            var activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            activitySpinner.Frame = new CGRect(
                centerX - (activitySpinner.Frame.Width / 2),
                centerY - activitySpinner.Frame.Height - 20,
                activitySpinner.Frame.Width,
                activitySpinner.Frame.Height);
            activitySpinner.AutoresizingMask = UIViewAutoresizing.All;
            AddSubview(activitySpinner);
            activitySpinner.StartAnimating();

            var loadingLabel = new UILabel(new CGRect(
                centerX - ((Frame.Width - 20) / 2),
                centerY + 20,
                Frame.Width - 20,
                22
            ))
            {
                BackgroundColor = UIColor.Clear,
                TextColor = UIColor.White,
                Text = "Downloading Data",
                TextAlignment = UITextAlignment.Center,
                AutoresizingMask = UIViewAutoresizing.All
            };
            AddSubview(loadingLabel);
        }

        public void Hide()
        {
            Animate(
                0.5,
                () => { Alpha = 0; },
                RemoveFromSuperview
            );
        }
    }
}