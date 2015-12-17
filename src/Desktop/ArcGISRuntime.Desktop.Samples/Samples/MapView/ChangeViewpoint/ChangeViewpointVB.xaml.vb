
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
Imports System.Collections.Generic
Imports System.Windows

Namespace ChangeViewpoint
    Partial Public Class ChangeViewpointVB

        Private londonCoords As New MapPoint(-13881.7678417696, 6710726.57374296, SpatialReferences.WebMercator)
        Private londonScale As Double = 8762.7156655229
        Private redlandsEnvelope As New Polygon(New List(Of MapPoint)() From {
            (New MapPoint(-13049785.1566222, 4032064.6003424)),
            (New MapPoint(-13049785.1566222, 4040202.42595729)),
            (New MapPoint(-13037033.5780234, 4032064.6003424)),
            (New MapPoint(-13037033.5780234, 4040202.42595729))
        }, SpatialReferences.WebMercator)
        Private edinburghEnvelope As New Polygon(New List(Of MapPoint)() From {
            (New MapPoint(-354262.156621384, 7548092.94093301)),
            (New MapPoint(-354262.156621384, 7548901.50684376)),
            (New MapPoint(-353039.164455303, 7548092.94093301)),
            (New MapPoint(-353039.164455303, 7548901.50684376))
        }, SpatialReferences.WebMercator)


        Public Sub New()
            InitializeComponent()
        End Sub

        Private Async Sub AnimateButton_Click(sender As Object, e As RoutedEventArgs)
            Try
                'Return to initial viewpoint so Animation curve can be demonstrated clearly.
                Await MyMapView.SetViewpointAsync(MyMapView.Map.InitialViewpoint)
                Dim viewpoint = New Viewpoint(edinburghEnvelope)
                'Animates the changing of the viewpoint giving a smooth transition from the old to the new view.
                MyMapView.SetViewpointAsync(viewpoint, System.TimeSpan.FromSeconds(5))
            Catch ex As Exception
                Dim errorMessage = "Viewpoint could not be set. " + ex.Message
                MessageBox.Show(errorMessage, "Sample error")
            End Try
        End Sub

        Private Sub GeometryButton_Click(sender As Object, e As RoutedEventArgs)
            Try
                'Sets the viewpoint extent to the provide bounding geometry.   
                MyMapView.SetViewpointGeometryAsync(redlandsEnvelope)
            Catch ex As Exception
                Dim errorMessage = "Viewpoint could not be set. " + ex.Message
                MessageBox.Show(errorMessage, "Sample error")
            End Try
        End Sub

        Private Sub CenterScaleButton_Click(sender As Object, e As RoutedEventArgs)
            Try
                'Centers the viewpoint on the provided map point. 
                MyMapView.SetViewpointCenterAsync(londonCoords)
                'Sets the viewpoint's zoom scale to the provided double value.  
                MyMapView.SetViewpointScaleAsync(londonScale)
            Catch ex As Exception
                Dim errorMessage = "Viewpoint could not be set. " + ex.Message
                MessageBox.Show(errorMessage, "Sample error")
            End Try
        End Sub

        Private Async Sub RotateButton_Click(sender As Object, e As RoutedEventArgs)
            Try
                'Gets the current rotation value of the map view.
                Dim currentRotation = MyMapView.MapRotation
                'Rotate the viewpoint by the given number of degrees. In this case the current rotation value
                'plus 90 is passed, this will result in a the map rotating 90 degrees anti-clockwise.  
                Await MyMapView.SetViewpointRotationAsync(currentRotation + 90.0)
            Catch ex As Exception
                Dim errorMessage = "Viewpoint could not be set. " + ex.Message
                MessageBox.Show(errorMessage, "Sample error")
            End Try
        End Sub
    End Class
End Namespace


