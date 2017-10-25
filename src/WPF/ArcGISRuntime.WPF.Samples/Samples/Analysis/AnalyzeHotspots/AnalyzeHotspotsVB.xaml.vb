' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Tasks
Imports Esri.ArcGISRuntime.Tasks.Geoprocessing
Imports System.Windows

Namespace AnalyzeHotspots

    Public Class AnalyzeHotspotsVB

        ' Url for the geoprocessing service
        Private Const HotspotsUrl As String =
            "http://sampleserver6.arcgisonline.com/arcgis/rest/services/911CallsHotspot/GPServer/911%20Calls%20Hotspot"

        ' The geoprocessing task for hot spot analysis 
        Private _hotspotTask As GeoprocessingTask

        ' The job that handles the communication between the application and the geoprocessing task
        Private _hotspotJob As GeoprocessingJob

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Async Sub Initialize()

            ' Create a map with a topographic basemap
            Dim myMap As New Map(Basemap.CreateTopographic())

            ' Create a new geoprocessing task
            _hotspotTask = Await GeoprocessingTask.CreateAsync(New Uri(HotspotsUrl))

            ' Assign the map to the MapView
            MyMapView.Map = myMap

        End Sub

        Private Async Sub OnAnalyzeHotspotsClicked(ByVal sender As Object, ByVal e As RoutedEventArgs)

            ' Show the busyOverlay indication
            ShowBusyOverlay()

            ' Get the 'from' and 'to' dates from the date pickers for the geoprocessing analysis
            Dim myFromDate = FromDate.SelectedDate.Value
            Dim myToDate = ToDate.SelectedDate.Value

            ' The end date must be at least one day after the start date
            If myToDate <= myFromDate.AddDays(1) Then

                ' Show error message
                MessageBox.Show(
                    "Please select valid time range. There has to be at least one day in between To and From dates.",
                    "Invalid date range")

                ' Remove the busyOverlay
                ShowBusyOverlay(False)

                Return

            End If

            ' Create the parameters that are passed to the used geoprocessing task
            Dim myHotspotParameters As New GeoprocessingParameters(GeoprocessingExecutionType.AsynchronousSubmit)

            ' Construct the date query
            Dim queryString = String.Format("(""DATE"" > date '{0} 00:00:00' AND ""DATE"" < date '{1} 00:00:00')",
                myFromDate.ToString("yyyy-MM-dd"),
                myToDate.ToString("yyyy-MM-dd"))

            ' Add the query that contains the date range used in the analysis
            myHotspotParameters.Inputs.Add("Query", New GeoprocessingString(queryString))

            ' Create job that handles the communication between the application and the geoprocessing task
            _hotspotJob = _hotspotTask.CreateJob(myHotspotParameters)

            Try

                ' Execute the geoprocessing analysis and wait for the results
                Dim myAnalysisResult As GeoprocessingResult = Await _hotspotJob.GetResultAsync()

                ' Add results to a map using map server from a geoprocessing task
                ' Load to get access to full extent
                Await myAnalysisResult.MapImageLayer.LoadAsync()

                ' Add the analysis layer to the map view
                MyMapView.Map.OperationalLayers.Add(myAnalysisResult.MapImageLayer)

                ' Zoom to the results
                Await MyMapView.SetViewpointAsync(New Viewpoint(myAnalysisResult.MapImageLayer.FullExtent))

            Catch e1 As TaskCanceledException

                ' This is thrown if the task is canceled. Ignore.

            Catch ex As Exception

                ' Display error messages if the geoprocessing task fails
                If _hotspotJob.Status = JobStatus.Failed AndAlso _hotspotJob.Error IsNot Nothing Then
                    MessageBox.Show("Executing geoprocessing failed. " & _hotspotJob.Error.Message, "Geoprocessing error")
                Else
                    MessageBox.Show("An error occurred. " & ex.ToString(), "Sample error")
                End If

            Finally

                ' Remove the busyOverlay
                ShowBusyOverlay(False)

            End Try

        End Sub


        Private Sub OnCancelTaskClicked(ByVal sender As Object, ByVal e As RoutedEventArgs)

            ' Cancel current geoprocessing job
            If _hotspotJob.Status = JobStatus.Started Then
                _hotspotJob.Cancel()
            End If

            ' Hide the busyOverlay indication
            ShowBusyOverlay(False)

        End Sub

        Private Sub ShowBusyOverlay(Optional ByVal visibility As Boolean = True)

            ' Function to toggle the visibility of interaction with the GUI for the user to 
            ' specify dates for the hot spot analysis. When the analysis Is running, the GUI
            ' for changing the dates Is 'grayed-out' and the progress bar with a cancel 
            ' button (aka. busyOverlay object) becomes active.

            If visibility Then

                'The geoprocessing task is processing. The busyOverly is present.
                busyOverlay.Visibility = System.Windows.Visibility.Visible
                progress.IsIndeterminate = True

            Else

                ' The user can interact with the date pickers. The busyOverlay is invisible.
                busyOverlay.Visibility = System.Windows.Visibility.Collapsed
                progress.IsIndeterminate = False

            End If

        End Sub

    End Class

End Namespace
