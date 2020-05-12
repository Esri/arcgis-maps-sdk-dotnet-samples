// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Threading;
using ArcGISRuntime.Samples.Managers;

namespace ArcGISRuntime.WPF.Samples.AnimateImageOverlay
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Animate images with image overlay",
        "MapView",
        "Animate a series of images with an image overlay.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("9465e8c02b294c69bdb42de056a23ab1")]
    public partial class AnimateImageOverlay


    {
        private ImageOverlay _imageOverlay;

        private Timer _timer;
        private string[] images;
        private Envelope pacificEnvelope;

        public AnimateImageOverlay()
        {
            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            // Create the scene.
            MySceneView.Scene = new Scene(new Basemap(new Uri("https://www.arcgis.com/home/item.html?id=1970c1995b8f44749f4b9b6e81b5ba45")));

            // Set the elevation source for the scene.
            //MySceneView.Scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer")));

            //// create an envelope of the pacific southwest sector for displaying the image frame
            //const Point pointForImageFrame(-120.0724273439448, 35.131016955536694, SpatialReference(4326));
            //m_pacificSouthwestEnvelope = Envelope(pointForImageFrame, 15.09589635986124, -14.3770441522488);

            var pointForFrame = new MapPoint(-120.0724273439448, 35.131016955536694, SpatialReferences.Wgs84);
            pacificEnvelope = new Envelope(pointForFrame, 15.09589635986124, -14.3770441522488);

            //// create a camera, looking at the pacific southwest sector
            //const Point observationPoint(-116.621, 24.7773, 856977);
            //const Camera camera(observationPoint, 353.994, 48.5495, 0);
            //const Viewpoint pacificSouthwestViewpoint(observationPoint, camera);
            //m_scene->setInitialViewpoint(pacificSouthwestViewpoint);

            var observationPoint = new MapPoint(-116.621, 24.7773, 856977, SpatialReferences.Wgs84);
            var camera = new Camera(observationPoint, 353.994, 48.5495, 0);
            var pacificViewpoint = new Viewpoint(observationPoint, camera);
            MySceneView.Scene.InitialViewpoint = pacificViewpoint;

            //// create an image overlay
            //m_imageOverlay = new ImageOverlay(this);
            _imageOverlay = new ImageOverlay();

            //// append the image overlay to the scene view
            //m_sceneView->imageOverlays()->append(m_imageOverlay);

            MySceneView.ImageOverlays.Add(_imageOverlay);

            //// Create new Timer and set the timeout interval to 68
            //// 68 ms interval timer equates to approximately 15 frames a second
            //m_timer = new QTimer(this);
            //m_timer->setInterval(68);
            

            var imageFolder = Path.Combine(DataManager.GetDataFolder("9465e8c02b294c69bdb42de056a23ab1"), "PacificSouthWest");

            images = Directory.GetFiles(imageFolder);

            _timer = new Timer(timerCallback);
            _timer.Change(0, 68);

        }

        private int m_index = 0;

        private void timerCallback(object state)
        {

            _imageOverlay.ImageFrame = new ImageFrame(new Uri(images[m_index]), pacificEnvelope);

            m_index++;
        }
    }
}
