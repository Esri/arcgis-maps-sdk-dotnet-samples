' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Symbology
Imports Esri.ArcGISRuntime.Tasks
Imports Esri.ArcGISRuntime.Tasks.Geoprocessing
Imports Esri.ArcGISRuntime.UI
Imports Esri.ArcGISRuntime.UI.Controls
Imports System.Windows
Imports System.Windows.Media

Namespace AnalyzeViewshed

    Public Class AnalyzeViewshedVB

        ' Url for the geoprocessing service
        Private Const _viewshedUrl As String = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Elevation/ESRI_Elevation_World/GPServer/Viewshed"

        ' Used to store state of the geoprocessing task
        Private _isExecutingGeoprocessing As Boolean

        ' The graphics overlay to show where the user clicked in the map
        Private _inputOverlay As GraphicsOverlay

        ' The graphics overlay to display the result of the viewshed analysis
        Private _resultOverlay As GraphicsOverlay

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Sub Initialize()

            ' Create a map with topographic basemap and an initial location
            Dim myMap As New Map(BasemapType.Topographic, 45.3790902612337, 6.84905317262762, 13)

            ' Hook into the tapped event
            AddHandler MyMapView.GeoViewTapped, AddressOf OnMapViewTapped

            ' Create empty overlays for the user clicked location and the results of the viewshed analysis
            CreateOverlays()

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

        Private Async Sub OnMapViewTapped(ByVal sender As Object, ByVal e As GeoViewInputEventArgs)

            ' The geoprocessing task is still executing, don't do anything else (i.e. respond to 
            ' more user taps) until the processing is complete.
            If _isExecutingGeoprocessing Then
                Return
            End If

            ' Indicate that the geoprocessing is running
            SetBusy()

            ' Clear previous user click location and the viewshed geoprocessing task results
            _inputOverlay.Graphics.Clear()
            _resultOverlay.Graphics.Clear()

            ' Create a marker graphic where the user clicked on the map and add it to the existing graphics overlay 
            Dim myInputGraphic As New Graphic(e.Location)
            _inputOverlay.Graphics.Add(myInputGraphic)

            ' Execute the geoprocessing task using the user click location 
            Await CalculateViewshed(e.Location)

        End Sub

        Private Async Function CalculateViewshed(ByVal location As MapPoint) As Task

            ' This function will define a new geoprocessing task that performs a custom viewshed analysis based upon a 
            ' user click on the map and then display the results back as a polygon fill graphics overlay. If there
            ' is a problem with the execution of the geoprocessing task an error message will be displayed 

            ' Create new geoprocessing task using the url defined in the member variables section
            Dim myViewshedTask = New GeoprocessingTask(New Uri(_viewshedUrl))

            ' Create a new feature collection table based upon point geometries using the current map view spatial reference
            Dim myInputFeatures = New FeatureCollectionTable(New List(Of Field)(), GeometryType.Point, MyMapView.SpatialReference)

            ' Create a new feature from the feature collection table. It will not have a coordinate location (x,y) yet
            Dim myInputFeature As Feature = myInputFeatures.CreateFeature()

            ' Assign a physical location to the new point feature based upon where the user clicked in the map view
            myInputFeature.Geometry = location

            ' Add the new feature with (x,y) location to the feature collection table
            Await myInputFeatures.AddFeatureAsync(myInputFeature)

            ' Create the parameters that are passed to the used geoprocessing task
            Dim myViewshedParameters As New GeoprocessingParameters(GeoprocessingExecutionType.SynchronousExecute)

            ' Request the output features to use the same SpatialReference as the map view
            myViewshedParameters.OutputSpatialReference = MyMapView.SpatialReference

            ' Add an input location to the geoprocessing parameters
            myViewshedParameters.Inputs.Add("Input_Observation_Point", New GeoprocessingFeatures(myInputFeatures))

            ' Create the job that handles the communication between the application and the geoprocessing task
            Dim myViewshedJob = myViewshedTask.CreateJob(myViewshedParameters)

            Try

                ' Execute analysis and wait for the results
                Dim myAnalysisResult As GeoprocessingResult = Await myViewshedJob.GetResultAsync()

                ' Get the results from the outputs
                Dim myViewshedResultFeatures As GeoprocessingFeatures = TryCast(myAnalysisResult.Outputs("Viewshed_Result"), GeoprocessingFeatures)

                ' Add all the results as a graphics to the map
                Dim myViewshedAreas As IFeatureSet = myViewshedResultFeatures.Features
                For Each myFeature In myViewshedAreas
                    _resultOverlay.Graphics.Add(New Graphic(myFeature.Geometry))
                Next myFeature

            Catch ex As Exception

                ' Display an error message if there is a problem
                If myViewshedJob.Status = JobStatus.Failed AndAlso myViewshedJob.Error IsNot Nothing Then
                    MessageBox.Show("Executing geoprocessing failed. " & myViewshedJob.Error.Message, "Geoprocessing error")
                Else
                    MessageBox.Show("An error occurred. " & ex.ToString(), "Sample error")
                End If

            Finally

                ' Indicate that the geoprocessing is not running
                SetBusy(False)

            End Try

        End Function

        Private Sub CreateOverlays()

            ' This function will create the overlays that show the user clicked location and the results of the 
            ' viewshed analysis. Note: the overlays will not be populated with any graphics at this point

            ' Create renderer for input graphic. Set the size and color properties for the simple renderer
            Dim myInputRenderer As New SimpleRenderer() With {.Symbol = New SimpleMarkerSymbol() With {.Size = 15, .Color = Colors.Red}}

            ' Create overlay to where input graphic is shown
            _inputOverlay = New GraphicsOverlay() With {.Renderer = myInputRenderer}

            ' Create fill renderer for output of the viewshed analysis. Set the color property of the simple renderer 
            Dim myResultRenderer As New SimpleRenderer() With {.Symbol = New SimpleFillSymbol() With {.Color = Color.FromArgb(100, 226, 119, 40)}}

            ' Create overlay to where viewshed analysis graphic is shown
            _resultOverlay = New GraphicsOverlay() With {.Renderer = myResultRenderer}

            ' Add the created overlays to the MapView
            MyMapView.GraphicsOverlays.Add(_inputOverlay)
            MyMapView.GraphicsOverlays.Add(_resultOverlay)

        End Sub

        Private Sub SetBusy(Optional ByVal isBusy As Boolean = True)

            ' This function toggles the visibility of the 'busyOverlay' Grid control defined in xaml,
            ' sets the 'progress' control feedback status and updates the _isExecutingGeoprocessing
            ' boolean to denote if the viewshed analysis is executing as a result of the user click 
            ' on the map

            If isBusy Then

                ' Change UI to indicate that the geoprocessing is running
                _isExecutingGeoprocessing = True
                busyOverlay.Visibility = Visibility.Visible
                progress.IsIndeterminate = True

            Else

                ' Change UI to indicate that the geoprocessing is not running
                _isExecutingGeoprocessing = False
                busyOverlay.Visibility = Visibility.Collapsed
                progress.IsIndeterminate = False

            End If

        End Sub

    End Class

End Namespace
