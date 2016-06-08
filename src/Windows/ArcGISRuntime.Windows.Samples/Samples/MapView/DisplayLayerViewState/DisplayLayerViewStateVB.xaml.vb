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
    Partial Public NotInheritable Class DisplayLayerViewStateVB
        ' Reference to list of view status for each layer
        Private _layerStatusModels As New List(Of LayerStatusModel)()

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

            ' Initialize the model list with unknown status for each layer
            For Each layer As Layer In myMap.OperationalLayers
                _layerStatusModels.Add(New LayerStatusModel(layer.Name, "Unknown"))
            Next

            ' Set models list as a itemssource
            layerStatusListView.ItemsSource = _layerStatusModels

            ' Event for layer view state changed
            AddHandler MyMapView.LayerViewStateChanged, AddressOf OnLayerViewStateChanged

            ' Provide used Map to the MapView
            MyMapView.Map = myMap
        End Sub

        Private Sub OnLayerViewStateChanged(sender As Object, e As LayerViewStateChangedEventArgs)
            ' State changed event is sent by a layer. In the list, find the layer which sends this event. 
            ' If it exists then update the status
            Dim model = _layerStatusModels.FirstOrDefault(Function(l) l.LayerName = e.Layer.Name)
            If model IsNot Nothing Then
                model.LayerViewStatus = e.LayerViewState.Status.ToString()
            End If
        End Sub

        ''' <summary>
        ''' This is a custom class that holds information for layer name and status
        ''' </summary>
        Public Class LayerStatusModel
            Implements INotifyPropertyChanged
            Private m_layerViewStatus As String

            Public Property LayerName() As String
                Get
                    Return m_LayerName
                End Get
                Private Set
                    m_LayerName = Value
                End Set
            End Property
            Private m_LayerName As String


            Public Property LayerViewStatus() As String
                Get
                    Return m_layerViewStatus
                End Get
                Set
                    m_layerViewStatus = Value
                    NotifyPropertyChanged()
                End Set
            End Property

            Public Sub New(layerName__1 As String, layerStatus As String)
                LayerName = layerName__1
                LayerViewStatus = layerStatus
            End Sub

            Public Event PropertyChanged As PropertyChangedEventHandler Implements INotifyPropertyChanged.PropertyChanged

            Private Sub NotifyPropertyChanged(<CallerMemberName> Optional propertyName As String = "")
                RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
            End Sub
        End Class
    End Class
End Namespace
