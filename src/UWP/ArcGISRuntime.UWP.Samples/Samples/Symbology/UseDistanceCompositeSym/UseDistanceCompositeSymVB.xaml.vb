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
Imports Esri.ArcGISRuntime.UI
Imports Windows.UI

Namespace UseDistanceCompositeSym

    Public Class UseDistanceCompositeSymVB

        ' URL for an image service to use as an elevation source
        Private _elevationSourceUrl As String = "http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"

        Public Sub New()

            InitializeComponent()

            ' Create the Scene, basemap, graphic, and composite symbol 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create a New Scene with an imagery basemap
            Dim myScene As Scene = New Scene(Basemap.CreateImagery())

            ' Create an elevation source for the Scene
            Dim elevationSrc As ArcGISTiledElevationSource = New ArcGISTiledElevationSource(New Uri(_elevationSourceUrl))
            myScene.BaseSurface.ElevationSources.Add(elevationSrc)

            ' Add the Scene to the SceneView
            MySceneView.Scene = myScene

            ' Create a new GraphicsOverlay and add it to the SceneView
            Dim graphicOverlay As GraphicsOverlay = New GraphicsOverlay()
            graphicOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative
            MySceneView.GraphicsOverlays.Add(graphicOverlay)

            ' Call a function to create a new distance composite symbol with three ranges
            Dim compositeSym As DistanceCompositeSceneSymbol = CreateCompositeSymbol()

            ' Create a new point graphic with the composite symbol, add it to the graphics overlay
            Dim locationPoint As MapPoint = New MapPoint(-2.708471, 56.096575, 5000, SpatialReferences.Wgs84)
            Dim pointGraphic As Graphic = New Graphic(locationPoint, compositeSym)
            graphicOverlay.Graphics.Add(pointGraphic)

            ' Set the viewpoint with a New camera focused on the graphic
            Dim newCamara As Camera = New Camera(New MapPoint(-2.708471, 56.096575, 5000, SpatialReferences.Wgs84), 1500, 0, 80, 0)
            MySceneView.SetViewpointCameraAsync(newCamara)

        End Sub

        Private Function CreateCompositeSymbol() As DistanceCompositeSceneSymbol

            ' Create three symbols for displaying a feature according to its distance from the camera
            ' First, a 3D (blue cube) symbol for when the camera is near the feature
            Dim cubeSym As SimpleMarkerSceneSymbol = New SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Cube, Colors.Blue, 125, 125, 125, SceneSymbolAnchorPosition.Center)

            ' 3D (red cone) symbol for when the feature is at an intermediate range
            Dim coneSym As SimpleMarkerSceneSymbol = New SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Cone, Colors.Red, 75, 75, 75, SceneSymbolAnchorPosition.Bottom)

            ' Simple marker symbol (circle) when the feature is far from the camera
            Dim markerSym As SimpleMarkerSymbol = New SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Yellow, 10.0)

            ' Create three New ranges for displaying each symbol
            Dim closeRange As DistanceSymbolRange = New DistanceSymbolRange(cubeSym, 0, 999)
            Dim midRange As DistanceSymbolRange = New DistanceSymbolRange(coneSym, 1000, 1999)
            Dim farRange As DistanceSymbolRange = New DistanceSymbolRange(markerSym, 2000, 0)

            ' Create a New DistanceCompositeSceneSymbol And add the ranges
            Dim compositeSym As DistanceCompositeSceneSymbol = New DistanceCompositeSceneSymbol()
            compositeSym.Ranges.Add(closeRange)
            compositeSym.Ranges.Add(midRange)
            compositeSym.Ranges.Add(farRange)

            ' Return the composite symbol
            Return compositeSym

        End Function

    End Class

End Namespace
