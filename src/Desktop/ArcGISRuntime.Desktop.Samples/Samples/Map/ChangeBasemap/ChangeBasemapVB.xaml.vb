' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping
Imports System.Windows.Controls

Namespace ChangeBasemap
    Partial Public Class ChangeBasemapVB
        ' String array to store titles for the viewpoints specified above.
        Private titles As String() = New String() {"Topo", "Streets", "Imagery", "Ocean"}

        Public Sub New()
            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateTopographic())

            ' Set titles as a items source
            basemapChooser.ItemsSource = titles

            ' Assign the map to the MapView
            MyMapView.Map = myMap
        End Sub

        Private Sub OnBasemapChooserSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim selectedBasemap = e.AddedItems(0).ToString()

            Select Case selectedBasemap
                Case "Topo"
                    ' Set the basemap to Topographic
                    MyMapView.Map.Basemap = Basemap.CreateTopographic()
                    Exit Select
                Case "Streets"
                    ' Set the basemap to Streets
                    MyMapView.Map.Basemap = Basemap.CreateStreets()
                    Exit Select
                Case "Imagery"
                    ' Set the basemap to Imagery
                    MyMapView.Map.Basemap = Basemap.CreateImagery()
                    Exit Select
                Case "Ocean"
                    ' Set the basemap to Oceans
                    MyMapView.Map.Basemap = Basemap.CreateOceans()
                    Exit Select
                Case Else
                    Exit Select
            End Select
        End Sub
    End Class
End Namespace
