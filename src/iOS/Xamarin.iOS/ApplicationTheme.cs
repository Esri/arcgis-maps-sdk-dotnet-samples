// Copyright 2020 Esri.

// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at

// http://www.apache.org/licenses/LICENSE-2.0

// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using UIKit;

namespace ArcGISRuntime
{
    /// <summary>
    /// Defines common styling parameters; modify this class to change colors, materials, spacing, and corner rounding
    /// </summary>
    public static class ApplicationTheme
    {

        public static UIColor BackgroundColor;
        public static UIColor ForegroundColor;
        public static UIColor SeparatorColor;
        public static UIBlurEffect PanelBackgroundMaterial;

        public static UIColor PrimaryLabelColor;
        public static UIColor SecondaryLabelColor;

        // Accessory button is a light/dark responsive color defined in the asset catalog
        public static UIColor AccessoryButtonColor;
        public static nint ActionButtonHeight;
        public static UIFont HeaderFont;

        static ApplicationTheme()
        {
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                BackgroundColor = UIColor.SystemBackgroundColor;
                ForegroundColor = UIColor.LabelColor;
                SeparatorColor = UIColor.SystemGray2Color;
                PanelBackgroundMaterial = UIBlurEffect.FromStyle(UIBlurEffectStyle.SystemMaterial);
                PrimaryLabelColor = UIColor.LabelColor;
                SecondaryLabelColor = UIColor.SecondaryLabelColor;
            }
            else
            {
                BackgroundColor = UIColor.White;
                ForegroundColor = UIColor.Black;
                SeparatorColor = UIColor.LightGray;
                PanelBackgroundMaterial = UIBlurEffect.FromStyle(UIBlurEffectStyle.Prominent);
                PrimaryLabelColor = UIColor.Black;
                SecondaryLabelColor = UIColor.DarkGray;
            }
        }
    }
}