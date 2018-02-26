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
Imports Esri.ArcGISRuntime.UI.Controls
Imports System.Windows.Media
Imports Esri.ArcGISRuntime.UI

Namespace RenderSimpleMarkers

    Public Class RenderSimpleMarkersVB

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateImagery())

            ' Create initial map location and reuse the location for graphic
            Dim centralLocation As New MapPoint(-226773, 6550477, SpatialReferences.WebMercator)
            Dim initialViewpoint As New Viewpoint(centralLocation, 7500)

            ' Set initial viewpoint
            myMap.InitialViewpoint = initialViewpoint

            ' Provide used Map to the MapView
            MyMapView.Map = myMap

            ' Create overlay to where graphics are shown
            Dim overlay As New GraphicsOverlay()

            ' Add created overlay to the MapView
            MyMapView.GraphicsOverlays.Add(overlay)

            ' Create a simple marker symbol
            Dim simpleSymbol As New SimpleMarkerSymbol() With
            {
                .Color = Colors.Red,
                .Size = 10,
                .Style = SimpleMarkerSymbolStyle.Circle
            }

            ' Add a new graphic with a central point that was created earlier
            Dim graphicWithSymbol As New Graphic(centralLocation, simpleSymbol)
            overlay.Graphics.Add(graphicWithSymbol)

        End Sub

    End Class

End Namespace

