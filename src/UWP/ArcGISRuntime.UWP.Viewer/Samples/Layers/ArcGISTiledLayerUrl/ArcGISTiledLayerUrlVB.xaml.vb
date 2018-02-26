' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping

Namespace ArcGISTiledLayerUrl

    Public Class ArcGISTiledLayerUrlVB

        Public Sub New()

            InitializeComponent()

            ' Add any initialization after the InitializeComponent() call.

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create new Map
            Dim myMap As New Map()

            ' Create uri to the tiled service
            Dim serviceUri = New Uri("http://services.arcgisonline.com/arcgis/rest/services/World_Topo_Map/MapServer")

            ' Create new tiled layer from the url
            Dim imageLayer As New ArcGISTiledLayer(serviceUri)

            ' Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(imageLayer)

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

    End Class

End Namespace
