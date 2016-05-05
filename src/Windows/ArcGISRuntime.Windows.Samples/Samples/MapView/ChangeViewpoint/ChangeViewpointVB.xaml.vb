
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
Imports Esri.ArcGISRuntime.Mapping
Imports System.Collections.Generic
Imports System.Windows
Imports Windows.UI.Popups

Namespace ChangeViewpoint
    Partial Public Class ChangeViewpointVB
        Private _londonCoords As New MapPoint(-13881.7678417696, 6710726.57374296, SpatialReferences.WebMercator)
        Private _londonScale As Double = 8762.7156655229
        Private _redlandsEnvelope As New Polygon(New List(Of MapPoint)() From {
            (New MapPoint(-13049785.1566222, 4032064.6003424)),
            (New MapPoint(-13049785.1566222, 4040202.42595729)),
            (New MapPoint(-13037033.5780234, 4032064.6003424)),
            (New MapPoint(-13037033.5780234, 4040202.42595729))
        }, SpatialReferences.WebMercator)
        Private _edinburghEnvelope As New Polygon(New List(Of MapPoint)() From {
            (New MapPoint(-354262.156621384, 7548092.94093301)),
            (New MapPoint(-354262.156621384, 7548901.50684376)),
            (New MapPoint(-353039.164455303, 7548092.94093301)),
            (New MapPoint(-353039.164455303, 7548901.50684376))
        }, SpatialReferences.WebMercator)

        Private errorDialog As MessageDialog = Nothing


        Public Sub New()
            InitializeComponent()
        End Sub

        Private Async Sub OnAnimateButtonClick(sender As Object, e As RoutedEventArgs)
            Try
                Dim viewpoint = New Viewpoint(_edinburghEnvelope)
                'Animates the changing of the viewpoint giving a smooth transition from the old to the new view
                MyMapView.SetViewpointAsync(viewpoint, System.TimeSpan.FromSeconds(5))
            Catch ex As Exception
                Dim errorMessage = ex.Message
                errorDialog = New MessageDialog(errorMessage, "Sample error")
            End Try
            If errorDialog IsNot Nothing Then
                Await errorDialog.ShowAsync()
            End If
        End Sub

        Private Async Sub OnGeometryButtonClick(sender As Object, e As RoutedEventArgs)
            Try
                'Sets the viewpoint extent to the provide bounding geometry   
                MyMapView.SetViewpointGeometryAsync(_redlandsEnvelope)
            Catch ex As Exception
                Dim errorMessage = ex.Message
                errorDialog = New MessageDialog(errorMessage, "Sample error")
            End Try
            If errorDialog IsNot Nothing Then
                Await errorDialog.ShowAsync()
            End If
        End Sub

        Private Async Sub OnCentreScaleButtonClick(sender As Object, e As RoutedEventArgs)
            Try
                'Centers the viewpoint on the provided map point 
                MyMapView.SetViewpointCenterAsync(_londonCoords)
                'Sets the viewpoint's zoom scale to the provided double value  
                MyMapView.SetViewpointScaleAsync(_londonScale)
            Catch ex As Exception
                Dim errorMessage = ex.Message
                errorDialog = New MessageDialog(errorMessage, "Sample error")
            End Try
            If errorDialog IsNot Nothing Then
                Await errorDialog.ShowAsync()
            End If
        End Sub

        Private Async Sub OnRotateButtonClick(sender As Object, e As RoutedEventArgs)
            Try
                'Gets the current rotation value of the map view
                Dim currentRotation = MyMapView.MapRotation
                'Rotate the viewpoint by the given number of degrees. In this case the current rotation value 
                'plus 90 is passed, this will result in a the map rotating 90 degrees anti-clockwise  
                Await MyMapView.SetViewpointRotationAsync(currentRotation + 90.0)
            Catch ex As Exception
                Dim errorMessage = ex.Message
                errorDialog = New MessageDialog(errorMessage, "Sample error")
            End Try
            If errorDialog IsNot Nothing Then
                Await errorDialog.ShowAsync()
            End If
        End Sub
    End Class
End Namespace