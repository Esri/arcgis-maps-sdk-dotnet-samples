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
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ChangeSublayerRenderer
{
    [Register("ChangeSublayerRenderer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Change sublayer renderer",
        "Layers",
        "This sample demonstrates how to change the sub-layer renderer of an ArcGIS map image layer. A unique value renderer is applied to see different population ranges in the counties sub-layer data.",
        "Click the 'Change Sublayer Renderer' button to apply a unique value renderer to the counties sub-layer.",
        "")]
    public class ChangeSublayerRenderer : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _labelToolbar = new UIToolbar();
        private readonly UIToolbar _buttonToolbar = new UIToolbar();
        private UIButton _changeSublayerRendererButton;
        private UILabel _helpLabel;

        // ArcGIS map image layer that contains four Census sub-layers.
        private ArcGISMapImageLayer _arcGISMapImageLayer;

        public ChangeSublayerRenderer()
        {
            Title = "Change sublayer renderer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat barHeight = controlHeight + 2 * margin;

                // Setup the frames for the views.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin + 70, 0, barHeight, 0);
                _labelToolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, 70);
                _helpLabel.Frame = new CGRect(margin, topMargin + margin, View.Bounds.Width - 2 * margin, 60);
                _buttonToolbar.Frame = new CGRect(0, View.Bounds.Height - 40, View.Bounds.Width, 40);
                _changeSublayerRendererButton.Frame = new CGRect(margin, View.Bounds.Height - 40 + margin, View.Bounds.Width - 2 * margin, 30);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }

        private async void Initialize()
        {
            // Create a new map based on the streets base map.
            Map newMap = new Map(Basemap.CreateStreets());

            // Assign the map to the MapView.
            _myMapView.Map = newMap;

            // Create an ArcGIS map image layer based on the Uri to that points to an ArcGIS Server map service that contains four Census sub-layers.
            // NOTE: sub-layer[0] = Census Block Points, sub-layer[1] = Census Block Group, sub-layer[3] = Counties, sub-layer[3] = States. 
            _arcGISMapImageLayer = new ArcGISMapImageLayer(new System.Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer"));

            // Add the ArcGIS map image layer to the map's operation layers collection.
            newMap.OperationalLayers.Add(_arcGISMapImageLayer);

            // Load the ArcGIS map image layer.
            await _arcGISMapImageLayer.LoadAsync();

            // Create an envelope that covers the continental US in the web Mercator spatial reference.
            Envelope continentalUSEnvelope = new Envelope(-14193469.5655232, 2509617.28647268, -7228772.04749191, 6737139.97573925, SpatialReferences.WebMercator);

            // Zoom the map to the extent of the envelope.
            await _myMapView.SetViewpointGeometryAsync(continentalUSEnvelope);
        }

        private ClassBreaksRenderer CreateClassBreaksRenderer()
        {
            // Define the colors that will be used by the unique value renderer.
            System.Drawing.Color gray = System.Drawing.Color.FromArgb(255, 153, 153, 153);
            System.Drawing.Color blue1 = System.Drawing.Color.FromArgb(255, 227, 235, 207);
            System.Drawing.Color blue2 = System.Drawing.Color.FromArgb(255, 150, 194, 191);
            System.Drawing.Color blue3 = System.Drawing.Color.FromArgb(255, 97, 166, 181);
            System.Drawing.Color blue4 = System.Drawing.Color.FromArgb(255, 69, 125, 150);
            System.Drawing.Color blue5 = System.Drawing.Color.FromArgb(255, 41, 84, 120);

            // Create a gray outline and five fill symbols with different shades of blue.
            SimpleLineSymbol outlineSimpleLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, gray, 1);
            SimpleFillSymbol simpleFileSymbol1 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue1, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol2 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue2, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol3 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue3, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol4 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue4, outlineSimpleLineSymbol);
            SimpleFillSymbol simpleFileSymbol5 = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, blue5, outlineSimpleLineSymbol);

            // Create a list of five class breaks for different population ranges.
            List<ClassBreak> listClassBreaks = new List<ClassBreak>
            {
                new ClassBreak("-99 to 8560", "-99 to 8560", -99, 8560, simpleFileSymbol1),
                new ClassBreak("> 8,560 to 18,109", "> 8,560 to 18,109", 8560, 18109, simpleFileSymbol2),
                new ClassBreak("> 18,109 to 35,501", "> 18,109 to 35,501", 18109, 35501, simpleFileSymbol3),
                new ClassBreak("> 35,501 to 86,100", "> 35,501 to 86,100", 35501, 86100, simpleFileSymbol4),
                new ClassBreak("> 86,100 to 10,110,975", "> 86,100 to 10,110,975", 86100, 10110975, simpleFileSymbol5)
            };

            // Create and return the a class break renderer for use with the POP2007 field in the counties sub-layer.
            return new ClassBreaksRenderer("POP2007", listClassBreaks);
        }

        private void ChangeSublayerRendererButton_TouchUpInside(object sender, EventArgs e)
        {
            // Get the counties sub-layer (the 3rd layer) from the ArcGIS map image layer.
            ArcGISMapImageSublayer countiesArcGISMapImageSubLayer = (ArcGISMapImageSublayer) _arcGISMapImageLayer.Sublayers[2];

            // Set the renderer of the ArcGIS map image sub-layer to a class break renderer based on population.
            countiesArcGISMapImageSubLayer.Renderer = CreateClassBreaksRenderer();

            // Disable the button after has been used.
            _changeSublayerRendererButton.Enabled = false;
            _changeSublayerRendererButton.SetTitleColor(UIColor.Gray, UIControlState.Disabled);
        }

        private void CreateLayout()
        {
            // Create a UITextView for the overall sample instructions.
            _helpLabel = new UILabel
            {
                Text = "Tap 'Change sublayer renderer' to apply a unique value renderer to the counties sublayer.",
                Lines = 2,
                AdjustsFontSizeToFitWidth = true
            };

            // Create a UIButton to change the sublayer renderer.
            _changeSublayerRendererButton = new UIButton();
            _changeSublayerRendererButton.SetTitle("Change sublayer renderer", UIControlState.Normal);
            _changeSublayerRendererButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // - Hook to touch event to change the sublayer renderer.
            _changeSublayerRendererButton.TouchUpInside += ChangeSublayerRendererButton_TouchUpInside;

            // Add the MapView and other controls to the page.
            View.AddSubviews(_myMapView, _labelToolbar, _buttonToolbar, _helpLabel, _changeSublayerRendererButton);
        }
    }
}