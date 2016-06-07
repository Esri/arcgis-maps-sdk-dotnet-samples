' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime
Imports Esri.ArcGISRuntime.Mapping
Imports System.Threading

Namespace AccessLoadStatus
    Partial Public Class AccessLoadStatusVB
        Public Sub New()
            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateImagery())

            ' Register to handle loading status changes
            AddHandler myMap.LoadStatusChanged, AddressOf OnMapsLoadStatusChanged

            ' Provide used Map to the MapView
            myMapView.Map = myMap
        End Sub

        Private Sub OnMapsLoadStatusChanged(sender As Object, e As LoadStatusEventArgs)
            ' Update the load status information
            Dispatcher.BeginInvoke(
                New ThreadStart(
                    Function() InlineAssignHelper(
                        loadStatusLabel.Content, String.Format("Maps' load status : {0}", e.Status.ToString()))))
        End Sub
        Private Shared Function InlineAssignHelper(Of T)(ByRef target As T, value As T) As T
            target = value
            Return value
        End Function
    End Class
End Namespace
