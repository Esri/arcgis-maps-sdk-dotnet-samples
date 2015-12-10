
'Copyright 2015 Esri.
'
'Licensed under the Apache License, Version 2.0 (the "License");
'you may not use this file except in compliance with the License.
'You may obtain a copy of the License at
'
'http://www.apache.org/licenses/LICENSE-2.0
'
'Unless required by applicable law or agreed to in writing, software
'distributed under the License is distributed on an "AS IS" BASIS,
'WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'See the License for the specific language governing permissions and
'limitations under the License.

Imports Esri.ArcGISRuntime
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Layers
Imports Windows.UI.Popups

Namespace SetMapSpatialReference
    Partial Public Class SetMapSpatialReferenceVB

        Private imageLayerUrl As String = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer"

        Public Sub New()
            InitializeComponent()
            LoadMap()
        End Sub

        Private Sub LoadMap()
            Dim errorDialog As MessageDialog
            Try
                'Create a map with World_Bonne projection
                Dim myMap = New Map(SpatialReference.Create(54024))
                'Create a map image layer which can re-project itself to the map's spatial reference
                Dim layer = New ArcGISMapImageLayer(New Uri(imageLayerUrl))
                'Set the map image layer as basemap
                myMap.Basemap.BaseLayers.Add(layer)
                'Set the map to be displayed in this view
                MyMapView.Map = myMap
            Catch ex As Exception
                Dim errorMessage = "Map cannot be loaded. " + ex.Message
                Dim errorDialog As New MessageDialog(errorMessage, "Sample error").ShowAsync()
            End Try
            If errorDialog IsNot Nothing Then
                errorDialog.ShowAsync()
            End If
        End Sub

    End Class
End Namespace

