' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping

Namespace ArcGISVectorTiledLayerUrl

    Partial Public Class ArcGISVectorTiledLayerUrlVB

        Private _navigationUrl As String = "http://www.arcgis.com/sharing/rest/content/items/00cd8e843bae49b3a040423e5d65416b/resources/styles/root.json"
        Private _streetUrl As String = "http://www.arcgis.com/sharing/rest/content/items/3b8814f6ddbd485cae67e8018992246e/resources/styles/root.json"
        Private _nightUrl As String = "http://www.arcgis.com/sharing/rest/content/items/f96366254a564adda1dc468b447ed956/resources/styles/root.json"
        Private _topographicUrl As String = "http://www.arcgis.com/sharing/rest/content/items/be44936bcdd24db588a1ae5076e36f34/resources/styles/root.json"

        Private _vectorTiledLayerUrl As String
        Private _vectorTiledLayer As ArcGISVectorTiledLayer

        ' String array to store some vector layer choices
        Private _vectorLayerNames As String() = New String() {"Topo", "Streets", "Night", "Navigation"}

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create a new ArcGISVectorTiledLayer with the navigation service Url
            _vectorTiledLayer = New ArcGISVectorTiledLayer(New Uri(_navigationUrl))

            ' Create new Map with basemap
            Dim myMap As New Map(New Basemap(_vectorTiledLayer))

            ' Set titles as a items source
            vectorLayersChooser.ItemsSource = _vectorLayerNames

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

        Private Sub OnVectorLayersChooserSelectionChanged(sender As Object, e As SelectionChangedEventArgs)

            Dim selectedVectorLayer = e.AddedItems(0).ToString()

            Select Case selectedVectorLayer

                Case "Topo"

                    _vectorTiledLayerUrl = _topographicUrl
                    Exit Select

                Case "Streets"

                    _vectorTiledLayerUrl = _streetUrl
                    Exit Select

                Case "Night"

                    _vectorTiledLayerUrl = _nightUrl
                    Exit Select

                Case "Navigation"

                    _vectorTiledLayerUrl = _navigationUrl
                    Exit Select

                Case Else

                    Exit Select

            End Select

            ' Create a new ArcGISVectorTiledLayer with the Url Selected by the user
            _vectorTiledLayer = New ArcGISVectorTiledLayer(New Uri(_vectorTiledLayerUrl))

            ' Create new Map with basemap and assigning to the Mapviews Map
            MyMapView.Map = New Map(New Basemap(_vectorTiledLayer))

        End Sub

    End Class

End Namespace