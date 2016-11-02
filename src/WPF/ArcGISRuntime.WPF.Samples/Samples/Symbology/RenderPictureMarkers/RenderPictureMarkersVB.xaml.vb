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
Imports Esri.ArcGISRuntime.UI.Controls

Namespace RenderPictureMarkers

    Public Class RenderPictureMarkersVB

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Async Sub Initialize()

            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateTopographic())

            ' Create and set initial map area
            Dim initialLocation As New Envelope(-229835, 6550763, -222560, 6552021, SpatialReferences.WebMercator)
            myMap.InitialViewpoint = New Viewpoint(initialLocation)

            ' Assign the map to the MapView
            MyMapView.Map = myMap

            ' Create overlay to where graphics are shown
            Dim overlay As New GraphicsOverlay()

            ' Add created overlay to the MapView
            MyMapView.GraphicsOverlays.Add(overlay)

            ' Add graphics using different source types
            Await CreatePictureMarkerSymbolFromUrl(overlay)
            Await CreatePictureMarkerSymbolFromResources(overlay)

        End Sub

        Private Async Function CreatePictureMarkerSymbolFromUrl(ByVal overlay As GraphicsOverlay) As Task

            ' Create uri to the used image
            Dim symbolUri = New Uri(
                "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Recreation/FeatureServer/0/images/e82f744ebb069bb35b234b3fea46deae")

            ' Create new symbol using asynchronous factory method from uri
            Dim campsiteSymbol As PictureMarkerSymbol = New PictureMarkerSymbol(symbolUri)

            ' Optionally set the size (if not set, the size in pixels of the image will be used)
            campsiteSymbol.Height = 18
            campsiteSymbol.Width = 18

            ' Create location for the campsite
            Dim campsitePoint As New MapPoint(-223560, 6552021, SpatialReferences.WebMercator)

            ' Create graphic with the location and symbol
            Dim campsiteGraphic As New Graphic(campsitePoint, campsiteSymbol)

            ' Add graphic to the graphics overlay
            overlay.Graphics.Add(campsiteGraphic)

        End Function

        Private Async Function CreatePictureMarkerSymbolFromResources(ByVal overlay As GraphicsOverlay) As Task

            ' Get current assembly that contains the image
            Dim currentAssembly = System.Reflection.Assembly.GetExecutingAssembly()

            ' Get image as a stream from the resources
            ' Picture is defined as EmbeddedResource and DoNotCopy
            Dim resourceStream = currentAssembly.GetManifestResourceStream(
                "ArcGISRuntime.WPF.Resources.PictureMarkerSymbols.pin_star_blue.png")

            ' Create new symbol using asynchronous factory method from stream
            Dim pinSymbol As PictureMarkerSymbol = Await PictureMarkerSymbol.CreateAsync(resourceStream)

            ' Create location for the pint
            Dim pinPoint As New MapPoint(-226773, 6550477, SpatialReferences.WebMercator)

            ' Create graphic with the location and symbol
            Dim pinGraphic As New Graphic(pinPoint, pinSymbol)

            ' Add graphic to the graphics overlay
            overlay.Graphics.Add(pinGraphic)

        End Function

    End Class

End Namespace

