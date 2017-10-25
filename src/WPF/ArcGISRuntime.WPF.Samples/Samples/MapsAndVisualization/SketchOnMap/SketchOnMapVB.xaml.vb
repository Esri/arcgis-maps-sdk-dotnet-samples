' Copyright 2017 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.Windows
Imports System.Windows.Media
Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Symbology
Imports Esri.ArcGISRuntime.UI

Namespace SketchOnMap

    Public Class SketchOnMapVB

        ' Graphics overlay to display sketch graphics
        Private _sketchOverlay As GraphicsOverlay

        Public Sub New()

            InitializeComponent()

            ' Call a function to set up the map and sketch editor 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create a map with 'Light Gray Canvas' basemap
            Dim myMap As New Map(Basemap.CreateLightGrayCanvas())

            ' Assign the map to the MapView
            MyMapView.Map = myMap

            ' Create graphics overlay to display sketch geometry
            _sketchOverlay = New GraphicsOverlay()
            MyMapView.GraphicsOverlays.Add(_sketchOverlay)

            ' Fill the combo box with choices for the sketch modes (shapes)
            SketchModeComboBox.ItemsSource = System.Enum.GetValues(GetType(SketchCreationMode))
            SketchModeComboBox.SelectedIndex = 0

            ' Set the sketch editor configuration to allow vertex editing, resizing, And moving
            Dim config As SketchEditConfiguration = MyMapView.SketchEditor.EditConfiguration
            config.AllowVertexEditing = True
            config.ResizeMode = SketchResizeMode.Uniform
            config.AllowMove = True

            ' Set the sketch editor as the page's data context
            Me.DataContext = MyMapView.SketchEditor

        End Sub

#Region "Graphic And symbol helpers"
        Private Function CreateGraphic(geom As Esri.ArcGISRuntime.Geometry.Geometry) As Graphic

            ' Create a graphic to display the specified geometry
            Dim sym As Symbol = Nothing

            Select Case (geom.GeometryType)

                ' Symbolize with a fill symbol
                Case GeometryType.Envelope
                Case GeometryType.Polygon
                    sym = New SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Red, Nothing)

                ' Symbolize with a line symbol
                Case GeometryType.Polyline
                    sym = New SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.Red, 5D)

                ' Symbolize with a marker symbol
                Case GeometryType.Point
                Case GeometryType.Multipoint
                    sym = New SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Colors.Red, 15D)

            End Select

            ' pass back a New graphic with the appropriate symbol
            Return New Graphic(geom, sym)

        End Function

        Private Async Function GetGraphicAsync() As Task(Of Graphic)
            ' Wait for the user to click a location on the map
            Dim mapPt As MapPoint = CType(Await MyMapView.SketchEditor.StartAsync(SketchCreationMode.Point, False), MapPoint)

            ' Convert the map point to a screen point
            Dim screenCoordinate As Point = MyMapView.LocationToScreen(mapPt)

            ' Identify graphics in the graphics overlay using the point
            Dim results As IReadOnlyList(Of IdentifyGraphicsOverlayResult) = Await MyMapView.IdentifyGraphicsOverlaysAsync(screenCoordinate, 2, False)

            ' If results were found, get the first graphic
            Dim sketchGraphic As Graphic = Nothing
            Dim idResult As IdentifyGraphicsOverlayResult = results.FirstOrDefault()
            If Not idResult Is Nothing AndAlso idResult.Graphics.Count > 0 Then
                sketchGraphic = idResult.Graphics.FirstOrDefault()
            End If

            ' Return the graphic (Or null if none were found)
            Return sketchGraphic
        End Function
#End Region

        Private Async Sub DrawButtonClick(sender As Object, e As RoutedEventArgs)

            Try

                ' Let the user draw on the map view using the chosen sketch mode
                Dim creationMode As SketchCreationMode = CType(SketchModeComboBox.SelectedItem, SketchCreationMode)
                Dim geom As Esri.ArcGISRuntime.Geometry.Geometry = Await MyMapView.SketchEditor.StartAsync(creationMode, True)

                ' Create and add a graphic from the geometry the user drew
                Dim sketchGraphic = CreateGraphic(geom)
                _sketchOverlay.Graphics.Add(sketchGraphic)

                ' Enable/disable the clear And edit buttons according to whether Or Not graphics exist in the overlay
                ClearButton.IsEnabled = _sketchOverlay.Graphics.Count > 0
                EditButton.IsEnabled = _sketchOverlay.Graphics.Count > 0

            Catch ex As TaskCanceledException

                ' Ignore ... let the user cancel drawing

            Catch ex As Exception

                ' Report exceptions
                MessageBox.Show("Error drawing graphic shape: " + ex.Message)

            End Try

        End Sub

        Private Sub ClearButtonClick(sender As Object, e As RoutedEventArgs)

            ' Remove all graphics from the graphics overlay
            _sketchOverlay.Graphics.Clear()

            ' Disable buttons that require graphics
            ClearButton.IsEnabled = False
            EditButton.IsEnabled = False

        End Sub

        Private Async Sub EditButtonClick(sender As Object, e As RoutedEventArgs)

            Try

                ' Allow the user to select a graphic
                Dim editGraphic As Graphic = Await GetGraphicAsync()
                If editGraphic Is Nothing Then Return

                ' Let the user make changes to the graphic's geometry, await the result (updated geometry)
                Dim newGeometry As Esri.ArcGISRuntime.Geometry.Geometry = Await MyMapView.SketchEditor.StartAsync(editGraphic.Geometry)

                ' Display the updated geometry in the graphic
                editGraphic.Geometry = newGeometry

            Catch ex As TaskCanceledException

                ' Ignore ... let the user cancel editing

            Catch ex As Exception

                ' Report exceptions
                MessageBox.Show("Error editing shape: " + ex.Message)
            End Try
        End Sub
    End Class

End Namespace
