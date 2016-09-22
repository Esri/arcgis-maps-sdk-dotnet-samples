' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping

Namespace ChangeSublayerVisibility
    Partial Public Class ChangeSublayerVisibilityVB
        Public Sub New()
            InitializeComponent()

            ' Setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Async Sub Initialize()
            ' Create new Map
            Dim myMap As New Map()

            ' Create uri to the map image layer
            Dim serviceUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer")

            ' Create new image layer from the url
            Dim imageLayer As New ArcGISMapImageLayer(serviceUri) With {
                .Name = "World Cities Population"
            }

            ' Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(imageLayer)

            ' Assign the map to the MapView
            MyMapView.Map = myMap

            ' Wait that the image layer is loaded and sublayer information is fetched
            Await imageLayer.LoadAsync()

            ' Assign sublayers to the listview
            sublayerListView.ItemsSource = imageLayer.Sublayers
        End Sub
    End Class
End Namespace