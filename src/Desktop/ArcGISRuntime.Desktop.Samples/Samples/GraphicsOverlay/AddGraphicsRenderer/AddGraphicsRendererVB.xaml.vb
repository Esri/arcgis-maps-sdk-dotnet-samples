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
Imports Esri.ArcGISRuntime.Symbology
Imports Esri.ArcGISRuntime.UI
Imports System.Windows.Media

Namespace AddGraphicsRenderer

    Public Class AddGraphicsRendererVB

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create a map with 'Imagery with Labels' basemap and an initial location
            Dim myMap As New Map(BasemapType.ImageryWithLabels, 34.056295, -117.1958, 14)

            ' Create graphics when MapView's viewpoint is initialized
            AddHandler MyMapView.ViewpointChanged, AddressOf OnViewpointChanged

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

        Private Sub OnViewpointChanged(ByVal sender As Object, ByVal e As EventArgs)

            ' Unhook the event
            RemoveHandler MyMapView.ViewpointChanged, AddressOf OnViewpointChanged

            ' Get area that is shown in a MapView
            Dim visibleArea As Polygon = MyMapView.VisibleArea

            ' Get extent of that area
            Dim extent As Envelope = visibleArea.Extent

            ' Get central point of the extent
            Dim centerPoint As MapPoint = extent.GetCenter()

            ' Create values inside the visible extent for creating graphic
            Dim extentWidth = CInt(extent.Width) \ 5
            Dim extentHeight = CInt(extent.Height) \ 10

            ' Create point collection
            Dim points As New Esri.ArcGISRuntime.Geometry.PointCollection(SpatialReferences.WebMercator) From
            {
                New MapPoint(centerPoint.X - extentWidth * 2, centerPoint.Y - extentHeight * 2),
                New MapPoint(centerPoint.X - extentWidth * 2, centerPoint.Y + extentHeight * 2),
                New MapPoint(centerPoint.X + extentWidth * 2, centerPoint.Y + extentHeight * 2),
                New MapPoint(centerPoint.X + extentWidth * 2, centerPoint.Y - extentHeight * 2)
            }

            ' Create overlay to where graphics are shown
            Dim overlay As New GraphicsOverlay()

            ' Add points to the graphics overlay
            For Each point In points

                ' Create new graphic and add it to the overlay
                overlay.Graphics.Add(New Graphic(point))

            Next point

            ' Create symbol for points
            Dim pointSymbol As New SimpleMarkerSymbol() With
            {
                .Color = Colors.Yellow,
                .Size = 30,
                .Style = SimpleMarkerSymbolStyle.Square
            }

            ' Create simple renderer with symbol
            Dim renderer As New SimpleRenderer(pointSymbol)

            ' Set renderer to graphics overlay
            overlay.Renderer = renderer

            ' Add created overlay to the MapView
            MyMapView.GraphicsOverlays.Add(overlay)

        End Sub

    End Class

End Namespace
