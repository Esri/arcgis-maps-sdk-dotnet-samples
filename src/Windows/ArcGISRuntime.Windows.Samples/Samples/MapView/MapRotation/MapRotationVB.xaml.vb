
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
Imports Esri.ArcGISRuntime.Layers
Imports Windows.UI.Popups

Namespace MapRotation

    Partial Public Class MapRotationVB

        Public Sub New()
            InitializeComponent()
            ' TODO move this to xaml 
            Dim myMap = New Map()

            Dim baseLayer = New ArcGISTiledLayer(
                New Uri("http://services.arcgisonline.com/arcgis/rest/services/World_Street_Map/MapServer"))
            myMap.Basemap.BaseLayers.Add(baseLayer)
            MyMapView.Map = myMap
        End Sub

        Private Async Sub OnDegreeSliderChange(sender As Object, e As RangeBaseValueChangedEventArgs)
            Try
                'Set Viewpoint's rotation to that of the slider value
                Await MyMapView.SetViewpointRotationAsync(degreeSlider.Value)
            Catch ex As Exception
                Dim errorMessage = "MapView Viewpoint could not be rotated. " + ex.Message
                Dim dialog = New MessageDialog(errorMessage, "Sample error").ShowAsync()
            End Try
        End Sub

    End Class
End Namespace
