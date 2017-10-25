
' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime
Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.UI
Imports Esri.ArcGISRuntime.UI.Controls
Imports Windows.UI.Core

Namespace DisplayDrawingStatus
    Partial Public NotInheritable Class DisplayDrawingStatusVB
        Public Sub New()
            Me.InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            ' Hook up the DrawStatusChanged event
            AddHandler MyMapView.DrawStatusChanged, AddressOf OnDrawStatusChanged

            ' Create new Map with basemap
            Dim myMap As New Map(BasemapType.Topographic, 34.056, -117.196, 4)

            ' Create uri to the used feature service
            Dim serviceUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/DamageAssessment/FeatureServer/0")

            ' Initialize a new feature layer
            Dim myFeatureTable As New ServiceFeatureTable(serviceUri)
            Dim myFeatureLayer As New FeatureLayer(myFeatureTable)

            ' Add the feature layer to the Map
            myMap.OperationalLayers.Add(myFeatureLayer)

            ' Provide used Map to the MapView
            MyMapView.Map = myMap
        End Sub

        Private Async Sub OnDrawStatusChanged(sender As Object, e As DrawStatusChangedEventArgs)
            ' Update the load status information
            Await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, Function()
                                                                         ' Show the activity indicator if the map is drawing
                                                                         If e.Status = DrawStatus.InProgress Then
                                                                             activityIndicator.IsActive = True
                                                                         Else
                                                                             activityIndicator.IsActive = False
                                                                         End If

                                                                     End Function)
        End Sub
    End Class
End Namespace