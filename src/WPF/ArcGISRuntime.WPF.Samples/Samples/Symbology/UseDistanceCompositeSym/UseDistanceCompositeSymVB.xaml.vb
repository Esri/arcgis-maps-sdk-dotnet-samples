' Copyright 2017 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.Windows.Media
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Symbology
Imports Esri.ArcGISRuntime.UI

Namespace UseDistanceCompositeSym

    Public Class UseDistanceCompositeSymVB

        ' Path to the model used to render the ModelSceneSymbol (helicopter)
        Private _modelFilePath As String = "/Resources/SkyCrane/SkyCrane.lwo"

        ' URL for an image service to use as an elevation source
        Private _elevationSourceUrl As String = "http://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"

        Public Sub New()

            InitializeComponent()

            ' Create the Scene, basemap, graphic, and composite symbol 
            Initialize()

        End Sub

        Private Async Sub Initialize()

            ' Create a New Scene with an imagery basemap
            Dim myScene As Scene = New Scene(Basemap.CreateImagery())

            ' Create an elevation source for the Scene
            Dim elevationSrc As ArcGISTiledElevationSource = New ArcGISTiledElevationSource(New Uri(_elevationSourceUrl))
            myScene.BaseSurface.ElevationSources.Add(elevationSrc)

            ' Add the Scene to the SceneView
            MySceneView.Scene = myScene

            ' Create a New GraphicsOverlay And add it to the SceneView
            Dim grafixOverlay As GraphicsOverlay = New GraphicsOverlay()
            grafixOverlay.SceneProperties.SurfacePlacement = SurfacePlacement.Relative
            MySceneView.GraphicsOverlays.Add(grafixOverlay)

            ' Call a function to create a New distance composite symbol with three ranges
            Dim compositeSym As DistanceCompositeSceneSymbol = Await CreateCompositeSymbolAsync()

            ' Create a New point graphic with the composite symbol, add it to the graphics overlay
            Dim heliPoint As MapPoint = New MapPoint(-2.708471, 56.096575, 5000, SpatialReferences.Wgs84)
            Dim heliGraphic As Graphic = New Graphic(heliPoint, compositeSym)
            grafixOverlay.Graphics.Add(heliGraphic)

            ' Set the viewpoint with a New camera focused on the graphic
            Dim newCamara As Camera = New Camera(New MapPoint(-2.708471, 56.096575, 5000, SpatialReferences.Wgs84), 1500, 0, 80, 0)
            MySceneView.SetViewpointCameraAsync(newCamara)

        End Sub

        Private Async Function CreateCompositeSymbolAsync() As Task(Of DistanceCompositeSceneSymbol)

            ' Build a URI that points to the model file
            Dim modelUri As Uri = New Uri(AppDomain.CurrentDomain.BaseDirectory & _modelFilePath)

            ' Create three symbols for displaying a feature according to its distance from the camera
            ' First, a model symbol for when the camera Is near the feature
            Dim modelSym As ModelSceneSymbol = Await ModelSceneSymbol.CreateAsync(modelUri, 0.01)

            ' 3D (cone) symbol for when the feature Is at an intermediate range
            Dim coneSym As SimpleMarkerSceneSymbol = New SimpleMarkerSceneSymbol(SimpleMarkerSceneSymbolStyle.Cone, Colors.Red, 75, 75, 75, SceneSymbolAnchorPosition.Bottom)

            ' Simple marker symbol (circle) when the feature Is far from the camera
            Dim markerSym As SimpleMarkerSymbol = New SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Red, 10.0)

            ' Create three New ranges for displaying each symbol
            Dim closeRange As DistanceSymbolRange = New DistanceSymbolRange(modelSym, 0, 999)
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
