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
        name: "Change sublayer renderer",
        category: "Layers",
        description: "Apply a renderer to a sublayer.",
        instructions: "Wait for the map image layer to load. Tap the button to apply a unique value renderer to see different population ranges in the counties sub-layer data.",
        tags: new[] { "class breaks", "dynamic layer", "dynamic rendering", "renderer", "sublayer", "symbology", "visualization" })]
    public class ChangeSublayerRenderer : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIBarButtonItem _changeRendererButton;

        // ArcGIS map image layer that contains four Census sub-layers.
        private ArcGISMapImageLayer _arcGISMapImageLayer;

        public ChangeSublayerRenderer()
        {
            Title = "Change sublayer renderer";
        }

        private void Initialize()
        {
            // Create a new map based on the streets base map.
            Map newMap = new Map(Basemap.CreateStreets());

            // Create an envelope that covers the continental US in the web Mercator spatial reference.
            Envelope continentalUSEnvelope = new Envelope(-14193469.5655232, 2509617.28647268, -7228772.04749191, 6737139.97573925, SpatialReferences.WebMercator);

            // Zoom the map to the extent of the envelope.
            newMap.InitialViewpoint = new Viewpoint(continentalUSEnvelope);

            // Assign the map to the MapView.
            _myMapView.Map = newMap;

            // Create an ArcGIS map image layer based on the Uri to that points to an ArcGIS Server map service that contains four Census sub-layers.
            // NOTE: sub-layer[0] = Census Block Points, sub-layer[1] = Census Block Group, sub-layer[3] = Counties, sub-layer[3] = States. 
            _arcGISMapImageLayer = new ArcGISMapImageLayer(new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer"));

            // Add the ArcGIS map image layer to the map's operation layers collection.
            newMap.OperationalLayers.Add(_arcGISMapImageLayer);
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
            ((UIBarButtonItem) sender).Enabled = false;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _changeRendererButton = new UIBarButtonItem();
            _changeRendererButton.Title = "Change sublayer renderer";

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _changeRendererButton,
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
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _changeRendererButton.Clicked += ChangeSublayerRendererButton_TouchUpInside;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _changeRendererButton.Clicked -= ChangeSublayerRendererButton_TouchUpInside;
        }
    }
}