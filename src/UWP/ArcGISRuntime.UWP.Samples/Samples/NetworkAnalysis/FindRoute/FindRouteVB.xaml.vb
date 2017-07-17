' Copyright 2017 Esri.
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
Imports Esri.ArcGISRuntime.Tasks
Imports Esri.ArcGISRuntime.Tasks.NetworkAnalysis
Imports Esri.ArcGISRuntime.UI
Imports Windows.UI

Namespace FindRoute
    Partial Public Class FindRouteVB

        ' List of stops on the route ('from' and 'to')
        Private _routeStops As List(Of NetworkAnalysis.Stop)

        ' Graphics overlay to display stops And the route result
        Private _routeGraphicsOverlay As GraphicsOverlay

        ' URI for the San Diego route service
        Private _sanDiegoRouteServiceUri As New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NetworkAnalysis/SanDiego/NAServer/Route")

        ' URIs for picture marker images
        Private _checkedFlagIconUri As New Uri("http://static.arcgis.com/images/Symbols/Transportation/CheckeredFlag.png")
        Private _carIconUri As New Uri("http://static.arcgis.com/images/Symbols/Transportation/CarRedFront.png")

        Public Sub New()
            InitializeComponent()
            ' Create the UI, setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            ' Define the route stop locations (points)
            Dim fromPoint As New MapPoint(-117.15494348793044, 32.706506537686927, SpatialReferences.Wgs84)
            Dim toPoint As New MapPoint(-117.14905088669816, 32.735308180609138, SpatialReferences.Wgs84)

            ' Create Stop objects with the points And add them to a list of stops
            Dim stop1 As New NetworkAnalysis.Stop(fromPoint)
            Dim stop2 As New NetworkAnalysis.Stop(toPoint)
            _routeStops = New List(Of NetworkAnalysis.Stop)
            _routeStops.Add(stop1)
            _routeStops.Add(stop2)

            ' Picture marker symbols from = car, To = checkered flag
            Dim carSymbol As New PictureMarkerSymbol(_carIconUri)
            Dim flagSymbol As New PictureMarkerSymbol(_checkedFlagIconUri)

            ' Add a slight offset (pixels) to the picture symbols
            carSymbol.OffsetX = -30
            flagSymbol.OffsetY = -15

            ' Create graphics for the stops
            Dim fromGraphic As New Graphic(fromPoint, carSymbol)
            Dim toGraphic As New Graphic(toPoint, flagSymbol)

            ' Create the graphics overlay And add the stop graphics
            _routeGraphicsOverlay = New GraphicsOverlay()
            _routeGraphicsOverlay.Graphics.Add(fromGraphic)
            _routeGraphicsOverlay.Graphics.Add(toGraphic)

            ' Get an Envelope that covers the area of the stops (And a little more)
            Dim routeStopsExtent As New Envelope(fromPoint, toPoint)
            Dim envBuilder As New EnvelopeBuilder(routeStopsExtent)
            envBuilder.Expand(1.5)

            ' Create a New viewpoint apply it to the map view when the spatial reference changes
            Dim sanDiegoViewpoint As New Viewpoint(envBuilder.ToGeometry())
            AddHandler MyMapView.SpatialReferenceChanged, Sub(s, e) MyMapView.SetViewpoint(sanDiegoViewpoint)

            ' Add a New Map And the graphics overlay to the map view
            MyMapView.Map = New Map(Basemap.CreateStreets())
            MyMapView.GraphicsOverlays.Add(_routeGraphicsOverlay)
        End Sub

        Private Async Sub SolveRouteClick(sender As Object, e As RoutedEventArgs)
            ' Create a New route task using the San Diego route service URI
            Dim solveRouteTask As RouteTask = Await RouteTask.CreateAsync(_sanDiegoRouteServiceUri)

            ' Get the default parameters from the route task (defined with the service)
            Dim routeParams As RouteParameters = Await solveRouteTask.CreateDefaultParametersAsync()

            ' Make some changes to the default parameters
            routeParams.ReturnStops = True
            routeParams.ReturnDirections = True

            ' Set the list of route stops that were defined at startup
            routeParams.SetStops(_routeStops)

            ' Solve for the best route between the stops And store the result
            Dim solveRouteResult As RouteResult = Await solveRouteTask.SolveRouteAsync(routeParams)

            ' Get the first (should be only) route from the result
            Dim firstRoute As Route = solveRouteResult.Routes.FirstOrDefault()

            ' Get the route geometry (polyline)
            Dim routePolyline As Polyline = firstRoute.RouteGeometry

            ' Create a thick purple line symbol for the route
            Dim routeSymbol As SimpleLineSymbol = New SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Purple, 8.0)

            ' Create a New graphic for the route geometry And add it to the graphics overlay
            Dim routeGraphic As New Graphic(routePolyline, routeSymbol)
            _routeGraphicsOverlay.Graphics.Add(routeGraphic)

            ' Get a list of directions for the route And display it in the list box
            Dim directionsList As IReadOnlyList(Of DirectionManeuver) = firstRoute.DirectionManeuvers
            DirectionsListBox.ItemsSource = directionsList
        End Sub

        Private Sub ResetClick(sender As Object, e As RoutedEventArgs)
            ' Clear the list of directions
            DirectionsListBox.ItemsSource = Nothing

            ' Remove the route graphic from the graphics overlay (only line graphic in the collection)
            Dim graphicsCount As Int32 = _routeGraphicsOverlay.Graphics.Count
            For i = graphicsCount - 1 To 0 Step -1

                ' Get this graphic And see if it has line geometry
                Dim g As Graphic = _routeGraphicsOverlay.Graphics(i)
                If g.Geometry.GeometryType = GeometryType.Polyline Then

                    ' Remove the graphic from the overlay
                    _routeGraphicsOverlay.Graphics.Remove(g)

                End If

            Next i
        End Sub
    End Class
End Namespace
