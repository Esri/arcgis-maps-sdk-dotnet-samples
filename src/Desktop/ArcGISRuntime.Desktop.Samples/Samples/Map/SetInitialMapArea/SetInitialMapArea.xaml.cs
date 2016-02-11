//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime;

namespace ArcGISRuntime.Desktop.Samples.SetInitialMapArea
{
    public partial class SetInitialMapArea
    {
        private Envelope myEnvelope = new Envelope(
            -12211308.778729, 4645116.003309, 
            -12208257.879667, 4650542.535773, 
            SpatialReferences.WebMercator);

        public SetInitialMapArea()
        {
            InitializeComponent();

            //Create a viewpoint from envelope
            var myViewPoint = new Viewpoint(myEnvelope);
            //Set MapView's Map initial extent
            MyMapView.Map.InitialViewpoint = myViewPoint;
        }
    }
}