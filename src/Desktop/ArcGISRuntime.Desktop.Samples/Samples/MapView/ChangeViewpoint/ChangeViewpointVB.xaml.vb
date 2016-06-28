' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Geometry
Imports System.Windows
Imports System.Windows.Controls


Namespace ChangeViewpoint

    Partial Public Class ChangeViewpointVB

        ' Coordinates for London
        Private _londonCoords As New MapPoint(-13881.7678417696, 6710726.57374296, SpatialReferences.WebMercator)
        Private _londonScale As Double = 8762.7156655229

        ' Coordinates for Redlands
        Private _redlandsEnvelope As New Polygon(New List(Of MapPoint)() From {
            (New MapPoint(-13049785.1566222, 4032064.6003424)),
            (New MapPoint(-13049785.1566222, 4040202.42595729)),
            (New MapPoint(-13037033.5780234, 4032064.6003424)),
            (New MapPoint(-13037033.5780234, 4040202.42595729))
        }, SpatialReferences.WebMercator)

        ' Coordinates for Edinburgh
        Private _edinburghEnvelope As New Polygon(New List(Of MapPoint)() From {
            (New MapPoint(-354262.156621384, 7548092.94093301)),
            (New MapPoint(-354262.156621384, 7548901.50684376)),
            (New MapPoint(-353039.164455303, 7548092.94093301)),
            (New MapPoint(-353039.164455303, 7548901.50684376))
        }, SpatialReferences.WebMercator)

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Sub Initialize()
            ' Create new Map with basemap and initial location
            Dim myMap As New Map(Esri.ArcGISRuntime.Mapping.Basemap.CreateTopographic())

            ' Assign the map to the MapView
            MyMapView.Map = myMap
        End Sub

        Private Async Sub OnButtonClick(sender As Object, e As RoutedEventArgs)

            ' Get .Content from the selected item
            Dim myButton As Button = TryCast(sender, Button)
            Dim selectedMapTitle = myButton.Content.ToString()

            Select Case selectedMapTitle
                Case "Geometry"

                    ' Set Viewpoint using Redlands envelope defined above and a padding of 20
                    Await MyMapView.SetViewpointGeometryAsync(_redlandsEnvelope, 20)

                Case "Center and Scale"

                    ' Set Viewpoint so that it is centered on the London coordinates defined above
                    Await MyMapView.SetViewpointCenterAsync(_londonCoords)

                    ' Set the Viewpoint scale to match the specified scale 
                    Await MyMapView.SetViewpointScaleAsync(_londonScale)

                Case "Animate"

                    ' Navigate to full extent of the first baselayer before animating to specified geometry
                    Await MyMapView.SetViewpointAsync(New Viewpoint(MyMapView.Map.Basemap.BaseLayers.First().FullExtent))

                    ' Create a new Viewpoint using the specified geometry
                    Dim viewpoint = New Viewpoint(_edinburghEnvelope)

                    ' Set Viewpoint of MapView to the Viewpoint created above and animate to it using a timespan of 5 seconds
                    Await MyMapView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(5))

                Case Else

            End Select

        End Sub

    End Class

End Namespace


