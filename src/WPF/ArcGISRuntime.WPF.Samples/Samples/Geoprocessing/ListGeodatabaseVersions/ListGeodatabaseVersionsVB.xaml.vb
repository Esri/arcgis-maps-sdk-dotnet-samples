' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Tasks
Imports Esri.ArcGISRuntime.Tasks.Geoprocessing
Imports System.Windows

Namespace ListGeodatabaseVersions

    Public Class ListGeodatabaseVersionsVB

        ' Url to used geoprocessing service
        Private Const ListVersionsUrl As String = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/GDBVersions/GPServer/ListVersions"

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Async Sub Initialize()

            ' Set the UI to indicate that the geoprocessing is running
            SetBusy(True)

            ' Get versions from a geodatabase
            Dim versionsFeatureSet As IFeatureSet = Await GetGeodatabaseVersionsAsync()

            ' Continue if we got a valid geoprocessing result
            If versionsFeatureSet IsNot Nothing Then

                ' Create a string builder to hold all of the information from the geoprocessing 
                ' task to display in the UI 
                Dim myStringBuilder = New System.Text.StringBuilder()

                ' Loop through each Feature in the FeatureSet 
                For Each version In versionsFeatureSet

                    ' Get the attributes (a dictionary of <key,value> pairs) from the Feature
                    Dim myDictionary = version.Attributes

                    ' Loop through each attribute (a <key,value> pair)
                    For Each oneAttribute In myDictionary

                        ' Get the key
                        Dim myKey As String = oneAttribute.Key

                        ' Get the value
                        Dim myValue As Object = oneAttribute.Value

                        If myKey IsNot Nothing And myValue IsNot Nothing Then

                            ' Add the key and value strings to the string builder 
                            myStringBuilder.AppendLine(myKey.ToString & ": " & myValue.ToString)

                        End If

                    Next oneAttribute

                    ' Add a blank line after each Feature (the listing of geodatabase versions)
                    myStringBuilder.AppendLine()

                Next version

                ' Display the result in the textbox
                theTextBox.Text = myStringBuilder.ToString()
            End If

            ' Set the UI to indicate that the geoprocessing is not running
            SetBusy(False)

        End Sub

        Private Async Function GetGeodatabaseVersionsAsync() As Task(Of IFeatureSet)

            ' Results will be returned as a feature set
            Dim results As IFeatureSet = Nothing

            ' Create new geoprocessing task 
            Dim listVersionsTask = New GeoprocessingTask(New Uri(ListVersionsUrl))

            ' Create parameters that are passed to the used geoprocessing task
            Dim listVersionsParameters As New GeoprocessingParameters(GeoprocessingExecutionType.SynchronousExecute)

            ' Create job that handles the communication between the application and the geoprocessing task
            Dim listVersionsJob = listVersionsTask.CreateJob(listVersionsParameters)

            Try

                ' Execute analysis and wait for the results
                Dim analysisResult As GeoprocessingResult = Await listVersionsJob.GetResultAsync()

                ' Get results from the outputs
                Dim listVersionsResults As GeoprocessingFeatures = TryCast(analysisResult.Outputs("Versions"), GeoprocessingFeatures)

                ' Set results
                results = listVersionsResults.Features

            Catch ex As Exception

                ' Error handling if something goes wrong
                If listVersionsJob.Status = JobStatus.Failed AndAlso listVersionsJob.Error IsNot Nothing Then
                    MessageBox.Show("Executing geoprocessing failed. " & listVersionsJob.Error.Message, "Geoprocessing error")
                Else
                    MessageBox.Show("An error occurred. " & ex.ToString(), "Sample error")
                End If

            Finally

                ' Set the UI to indicate that the geoprocessing is not running
                SetBusy(False)

            End Try

            Return results

        End Function

        Private Sub SetBusy(Optional ByVal isBusy As Boolean = True)

            If isBusy Then

                ' Change UI to indicate that the geoprocessing is running
                busyOverlay.Visibility = Visibility.Visible
                progress.IsIndeterminate = True

            Else

                ' Change UI to indicate that the geoprocessing is not running
                busyOverlay.Visibility = Visibility.Collapsed
                progress.IsIndeterminate = False

            End If

        End Sub

    End Class

End Namespace
