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

Namespace DisplayLayerViewState
    Partial Public Class DisplayLayerViewStateVB

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create new Map
            Dim myMap As New Map()

            ' Create the uri for the tiled layer
            Dim tiledLayerUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer")

            ' Create a tiled layer using url
            Dim tiledLayer As New ArcGISTiledLayer(tiledLayerUri)
            tiledLayer.Name = "Tiled Layer"

            ' Add the tiled layer to map
            myMap.OperationalLayers.Add(tiledLayer)

            ' Create the uri for the ArcGISMapImage layer
            Dim imageLayerUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer")

            ' Create ArcGISMapImage layer using a url
            Dim imageLayer As New ArcGISMapImageLayer(imageLayerUri)
            imageLayer.Name = "Image Layer"

            ' Set the visible scale range for the image layer
            imageLayer.MinScale = 40000000
            imageLayer.MaxScale = 2000000

            ' Add the image layer to map
            myMap.OperationalLayers.Add(imageLayer)

            ' Create Uri for feature layer
            Dim featureLayerUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0")

            ' Create a feature layer using url
            Dim myFeatureLayer As New FeatureLayer(featureLayerUri)
            myFeatureLayer.Name = "Feature Layer"

            ' Add the feature layer to map
            myMap.OperationalLayers.Add(myFeatureLayer)

            ' Create a mappoint the map should zoom to
            Dim mapPoint As New MapPoint(-11000000, 4500000, SpatialReferences.WebMercator)

            ' Set the initial viewpoint for map
            myMap.InitialViewpoint = New Viewpoint(mapPoint, 50000000)

            ' Event for layer view state changed
            AddHandler MyMapView.LayerViewStateChanged, AddressOf OnLayerViewStateChanged

            ' Provide used Map to the MapView
            MyMapView.Map = myMap

        End Sub

        Private Sub OnLayerViewStateChanged(sender As Object, e As LayerViewStateChangedEventArgs)

            ' For each execution of the MapView.LayerViewStateChanges Event, get the name of
            ' the layer and it's LayerViewState.Status
            Dim lName As String = e.Layer.Name
            Dim lViewStatus As String = e.LayerViewState.Status.ToString()

            ' Display the layer name and view status in the appropriate Label control
            Select Case lName
                Case "Tiled Layer"
                    StatusLabel_TiledLayer.Content = lName & " - " & lViewStatus
                Case "Image Layer"
                    StatusLabel_ImageLayer.Content = lName & " - " & lViewStatus
                Case "Feature Layer"
                    StatusLabel_FeatureLayer.Content = lName & " - " & lViewStatus
                Case Else
            End Select

        End Sub

    End Class

End Namespace
