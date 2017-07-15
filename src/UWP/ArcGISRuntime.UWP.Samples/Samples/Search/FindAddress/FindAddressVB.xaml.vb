' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
' language governing permissions and limitations under the License.

Imports System.Reflection
Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Symbology
Imports Esri.ArcGISRuntime.Tasks.Geocoding
Imports Esri.ArcGISRuntime.UI
Imports Esri.ArcGISRuntime.UI.Controls

Namespace FindAddress

    Partial Public Class FindAddressVB

        ' String array to store the different device location options.
        Private _navigationTypes As String() = New String() {"On", "Re-Center", "Navigation", "Compass"}

        ' The LocatorTask provides geocoding services via a Service
        Private _geocoder As LocatorTask

        Private _serviceUri As Uri = New Uri("https://geocode.arcgis.com/arcgis/rest/services/World/GeocodeServer")

        Public Sub New()
            InitializeComponent()

            ' Setup the control references and execute initialization
            Initialize()
        End Sub

        Private Async Sub Initialize()
            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateImageryWithLabels())

            ' Assign the map to the MapView
            MyMapView.Map = myMap

            ' Initialize the LocatorTask with the provided service Uri
            _geocoder = Await LocatorTask.CreateAsync(_serviceUri)

        End Sub

        Private Async Sub updateSearch()
            Dim enteredText = MySearchBox.Text

            ' Clear existing marker
            MyMapView.GraphicsOverlays.Clear()

            ' Return gracefully if the textbox Is empty
            If String.IsNullOrWhiteSpace(enteredText) Then Return

            ' Get the nearest suggestion to entered text
            Dim suggestions As IReadOnlyList(Of SuggestResult) = Await _geocoder.SuggestAsync(enteredText)

            ' Stop gracefully if there are no suggestions
            If suggestions.Count < 1 Then Return

            ' Get the full address for the first suggestion
            Dim addresses As IReadOnlyList(Of GeocodeResult) = Await _geocoder.GeocodeAsync(suggestions.First().Label)

            ' Stop gracefully if the geocoder does not return a result
            If addresses.Count < 1 Then Return

            ' Place a marker on the map
            Dim resultOverlay = New GraphicsOverlay()
            Dim point = Await _graphicForPoint(addresses.First().DisplayLocation)

            ' Record the address with the overlay for easy recall when the graphic is tapped
            point.Attributes.Add("Address", addresses.First().Label)
            resultOverlay.Graphics.Add(point)
            MyMapView.GraphicsOverlays.Add(resultOverlay)
            Await MyMapView.SetViewpointGeometryAsync(addresses.First().Extent)
        End Sub

        Private Async Function _graphicForPoint(point As MapPoint) As Task(Of Graphic)
            ' Get current assembly that contains the image
            Dim currentAssembly = Me.GetType().GetTypeInfo().Assembly

            ' Get image as a stream from the resources
            ' Picture Is defined as EmbeddedResource And DoNotCopy
            Dim resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.UWP.Samples.pin_star_red.png")

            ' Create New symbol using asynchronous factory method from stream
            Dim pinSymbol As PictureMarkerSymbol = Await PictureMarkerSymbol.CreateAsync(resourceStream)
            pinSymbol.Width = 15
            pinSymbol.Height = 30
            pinSymbol.OffsetX = pinSymbol.Width / 2
            pinSymbol.OffsetY = pinSymbol.Height / 2
            Return New Graphic(point, pinSymbol)
        End Function

        Private Sub MySearchBox_TextChanged(sender As Object, e As TextChangedEventArgs)
            updateSearch()
        End Sub

        Private Sub MenuFlyoutItem_Click(sender As Object, e As RoutedEventArgs)
            Dim menuItem As MenuFlyoutItem = sender
            Dim textValue As String = menuItem.Text
            MySearchBox.Text = textValue
            updateSearch()
        End Sub

        '<summary>
        ' Handle tap event on the map; displays callouts showing the address for a tapped search result
        ' </summary>
        Private Async Sub MyMapView_GeoViewTapped(sender As Object, e As GeoViewInputEventArgs) Handles MyMapView.GeoViewTapped
            ' Search for the graphics underneath the user's tap
            Dim results As IReadOnlyList(Of IdentifyGraphicsOverlayResult) = Await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 12, False)

            ' Return gracefully if there was no result
            If results.Count = 0 Then Return

            ' Reverse geocode to get addresses
            Dim addresses As IReadOnlyList(Of GeocodeResult) = Await _geocoder.ReverseGeocodeAsync(e.Location)

            ' Format addresses
            Dim address As GeocodeResult = addresses.First()
            Dim calloutTitle As String = address.Attributes("City").ToString() & ", " & address.Attributes("Region").ToString()
            Dim calloutDetail As String = address.Attributes("MetroArea").ToString()

            ' Display the callout
            If results.First().Graphics.Count > 0 Then
                Dim point As MapPoint = MyMapView.ScreenToLocation(e.Position)
                MyMapView.ShowCalloutAt(point, New CalloutDefinition(calloutTitle, calloutDetail))
            End If
        End Sub

    End Class

End Namespace