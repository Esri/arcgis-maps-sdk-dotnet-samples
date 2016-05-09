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
Imports System.Windows
Imports System.Windows.Media

Namespace IdentifyGraphics

    Public Class IdentifyGraphicsVB

        ' Graphics overlay to host graphics
        Private _polygonOverlay As GraphicsOverlay

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create a map with 'Imagery with Labels' basemap and an initial location
            Dim myMap As New Map(Basemap.CreateTopographic())

            ' Create graphics overlay with graphics
            CreateOverlay()

            ' Hook into tapped event
            AddHandler MyMapView.GeoViewTapped, AddressOf OnMapViewTapped

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

        Private Sub CreateOverlay()

            ' Create polygon builder and add polygon corners into it
            Dim builder As New PolygonBuilder(SpatialReferences.WebMercator)
            builder.AddPoint(New MapPoint(-2000000.0, 2000000.0))
            builder.AddPoint(New MapPoint(2000000.0, 2000000.0))
            builder.AddPoint(New MapPoint(2000000.0, -2000000.0))
            builder.AddPoint(New MapPoint(-2000000.0, -2000000.0))

            ' Get geometry from the builder
            Dim polygonGeometry As Polygon = builder.ToGeometry()

            ' Create symbol for the polygon
            Dim polygonSymbol As New SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Yellow, Nothing)

            ' Create new graphic
            Dim polygonGraphic As New Graphic(polygonGeometry, polygonSymbol)

            ' Create overlay to where graphics are shown
            _polygonOverlay = New GraphicsOverlay()
            _polygonOverlay.Graphics.Add(polygonGraphic)

            ' Add created overlay to the MapView
            MyMapView.GraphicsOverlays.Add(_polygonOverlay)

        End Sub

        Private Async Sub OnMapViewTapped(ByVal sender As Object, ByVal e As GeoViewInputEventArgs)

            Dim tolerance = 10.0R ' Use larger tolerance for touch
            Dim maximumResults = 1 ' Only return one graphic

            ' Use the following method to identify graphics in a specific graphics overlay
            Dim identifyResults As IReadOnlyList(Of Graphic) = Await MyMapView.IdentifyGraphicsOverlayAsync(
                _polygonOverlay,
                e.Position,
                tolerance, maximumResults)

            ' Check if we got results
            If identifyResults.Count > 0 Then

                '  Display to the user the identify worked.
                MessageBox.Show("Tapped on graphic", "")

            End If

        End Sub

    End Class

End Namespace
