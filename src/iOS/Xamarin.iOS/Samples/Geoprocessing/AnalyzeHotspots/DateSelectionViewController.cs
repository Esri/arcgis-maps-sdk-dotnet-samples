// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.AnalyzeHotspots
{
    public class DateSelectionViewController : UIViewController
    {
        // Hold references to the UI controls
        public readonly UIDatePicker StartPicker;
        public readonly UIDatePicker EndPicker;

        public DateSelectionViewController()
        {
            Title = "Select a date range";

            // Configured here because these need to be initialized before the view is loaded/presented.
            StartPicker = new UIDatePicker();
            StartPicker.SetDate((NSDate) new DateTime(1998, 1, 1, 0, 0, 0, DateTimeKind.Local), false);
            EndPicker = new UIDatePicker();
            EndPicker.SetDate((NSDate) new DateTime(1998, 1, 31, 0, 0, 0, DateTimeKind.Local), false);
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            UIStackView stackView = new UIStackView();
            stackView.Axis = UILayoutConstraintAxis.Vertical;
            stackView.TranslatesAutoresizingMaskIntoConstraints = false;
            stackView.Spacing = 8;


            UILabel startLabel = new UILabel();
            startLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            startLabel.Text = "Start date:";
            stackView.AddArrangedSubview(startLabel);

            StartPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            StartPicker.Mode = UIDatePickerMode.Date;
            stackView.AddArrangedSubview(StartPicker);

            UILabel endLabel = new UILabel();
            endLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            endLabel.Text = "End date:";
            stackView.AddArrangedSubview(endLabel);

            EndPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            EndPicker.Mode = UIDatePickerMode.Date;
            stackView.AddArrangedSubview(EndPicker);

            // Spacing.
            stackView.AddArrangedSubview(new UIView());

            // Add the views.
            View.AddSubview(stackView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new []
            {
                stackView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 8),
                stackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -8),
                stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8),
                stackView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor, -8)
            });
        }
    }
}