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

Namespace ArcGISRuntime.Desktop.Samples.FeatureLayerDefinitionExpression
    Partial Public Class FeatureLayerDefinitionExpressionVB
        'Create and hold reference to the feature layer
        Private _featureLayer As FeatureLayer

        Public Sub New()
            InitializeComponent()

            'setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateTopographic())

            ' Create a MapPoint the map should zoom to
            Dim mapPoint As New MapPoint(-13630484, 4545415, SpatialReferences.WebMercator)

            ' Set the initial viewpoint for map
            myMap.InitialViewpoint = New Viewpoint(mapPoint, 90000)

            ' Provide used Map to the MapView
            MyMapView.Map = myMap

            ' Create the uri for the feature service
            Dim featureServiceUri As New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer/0")

            ' Initialize feature table using a url to feature server url
            Dim featureTable As New ServiceFeatureTable(featureServiceUri)

            ' Initialize a new feature layer based on the feature table
            _featureLayer = New FeatureLayer(featureTable)

            'Add the feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer)

        End Sub

        Private Sub OnApplyExpressionClicked(sender As Object, e As EventArgs)
            ' Adding definition expression to show specific features only
            _featureLayer.DefinitionExpression = "req_Type = 'Tree Maintenance or Damage'"
        End Sub

        Private Sub OnResetButtonClicked(sender As Object, e As EventArgs)
            ' Reset the definition expression to see all features again
            _featureLayer.DefinitionExpression = ""
        End Sub
    End Class
End Namespace