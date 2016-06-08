
' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Symbology
Imports Windows.UI

Namespace ChangeFeatureLayerRenderer
    Partial Public Class ChangeFeatureLayerRendererVB
        ' Create and hold reference to the feature layer
        Private _featureLayer As FeatureLayer

        Public Sub New()
            InitializeComponent()

            ' Setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Async Sub Initialize()
            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateTopographic())

            ' Create and set initial map area
            Dim initialLocation As New Envelope(-13075816.4047166, 4014771.46954516, -13073005.6797177, 4016869.78617381, SpatialReferences.WebMercator)

            ' Set the initial viewpoint for map
            myMap.InitialViewpoint = New Viewpoint(initialLocation)

            ' Provide used Map to the MapView
            MyMapView.Map = myMap

            ' Create uri to the used feature service
            Dim serviceUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/PoolPermits/FeatureServer/0")

            ' Initialize feature table using a url to feature server url
            Dim featureTable As New ServiceFeatureTable(serviceUri)

            ' Initialize a new feature layer based on the feature table
            _featureLayer = New FeatureLayer(featureTable)

            ' Make sure that the feature layer gets loaded
            Await _featureLayer.LoadAsync()

            ' Check for the load status. If the layer is loaded then add it to map
            If _featureLayer.LoadStatus = Esri.ArcGISRuntime.LoadStatus.Loaded Then
                ' Add the feature layer to the map
                myMap.OperationalLayers.Add(_featureLayer)
            End If
        End Sub

        Private Sub OnOverrideButtonClicked(sender As Object, e As RoutedEventArgs)
            ' Create a symbol to be used in the renderer
            Dim symbol As New SimpleLineSymbol() With {
                .Color = Colors.Blue,
                .Width = 2,
                .Style = SimpleLineSymbolStyle.Solid
            }

            ' Create a new renderer using the symbol just created
            Dim renderer As New SimpleRenderer(symbol)

            ' Assign the new renderer to the feature layer
            _featureLayer.Renderer = renderer
        End Sub

        Private Sub OnResetButtonClicked(sender As Object, e As RoutedEventArgs)
            ' Reset the renderer to default
            _featureLayer.ResetRenderer()
        End Sub
    End Class
End Namespace