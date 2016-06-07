' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping

Namespace FeatureLayerUrl
    Partial Public Class FeatureLayerUrlVB
        Public Sub New()
            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateTerrainWithLabels())

            ' Create and set initial map location
            Dim initialLocation As New MapPoint(-13176752, 4090404, SpatialReferences.WebMercator)
            myMap.InitialViewpoint = New Viewpoint(initialLocation, 300000)

            ' Create uri to the used feature service
            Dim serviceUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Energy/Geology/FeatureServer/9")

            ' Create new FeatureLayer from service uri and
            Dim geologyLayer As New FeatureLayer(serviceUri)

            ' Add created layer to the map
            myMap.OperationalLayers.Add(geologyLayer)

            ' Assign the map to the MapView
            MyMapView.Map = myMap
        End Sub
    End Class
End Namespace