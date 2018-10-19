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
        public UIDatePicker StartPicker;
        public UIDatePicker EndPicker;

        public DateSelectionViewController()
        {
            Title = "Select a date range";
            StartPicker = new UIDatePicker();
            StartPicker.SetDate((NSDate)new DateTime(1998, 1, 1, 0, 0, 0, DateTimeKind.Local), false);
            EndPicker = new UIDatePicker();
            EndPicker.SetDate((NSDate)new DateTime(1998, 1, 31, 0, 0, 0, DateTimeKind.Local), false);
        }

        public override void LoadView()
        {
            View = new UIView();
            View.BackgroundColor = UIColor.White;

            UIStackView stackView = new UIStackView();
            stackView.Axis = UILayoutConstraintAxis.Vertical;
            stackView.TranslatesAutoresizingMaskIntoConstraints = false;
            stackView.Spacing = 8;
            View.AddSubview(stackView);

            stackView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 8).Active = true;
            stackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -8).Active = true;
            stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8).Active = true;
            stackView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor, -8).Active = true;

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
        }
    }
}
