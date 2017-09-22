' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.Windows
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Location
Imports Esri.ArcGISRuntime.Mapping

Namespace DisplayDeviceLocation
    Partial Public Class DisplayDeviceLocationVB
        ' String array to store the different device location options.
        Private _navigationTypes As String() = New String() {"On", "Re-Center", "Navigation", "Compass"}

        Public Sub New()
            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            ' Create new Map with basemap
            Dim myMap As New Map(Basemap.CreateImagery())

            ' Provide used Map to the MapView
            MyMapView.Map = myMap

            ' Set navigation types as items source and set default value
            modeChooser.ItemsSource = _navigationTypes
            modeChooser.SelectedIndex = 0
        End Sub

        Private Sub OnStartButtonClicked(sender As Object, e As RoutedEventArgs)
            'TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
            If Not MyMapView.LocationDisplay.IsStarted Then
                MyMapView.LocationDisplay.Start()
            End If
        End Sub
        Private Sub OnStopButtonClicked(sender As Object, e As RoutedEventArgs)
            'TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
            If MyMapView.LocationDisplay.IsStarted Then
                MyMapView.LocationDisplay.[Stop]()
            End If
        End Sub

        Private Sub OnModeChooserSelectionChanged(sender As Object, e As System.Windows.Controls.SelectionChangedEventArgs)
            ' Get index that is used to get the selected url
            Dim selectedIndex = _navigationTypes.ToList().IndexOf(modeChooser.SelectedValue.ToString())

            Select Case selectedIndex
                Case 0
                    ' Starts location display with auto pan mode set to Off
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Off

                    'TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    If Not MyMapView.LocationDisplay.IsStarted Then
                        MyMapView.LocationDisplay.Start()
                    End If
                    Exit Select

                Case 1
                    ' Starts location display with auto pan mode set to Re-center
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter

                    'TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    If Not MyMapView.LocationDisplay.IsStarted Then
                        MyMapView.LocationDisplay.Start()
                    End If
                    Exit Select

                Case 2
                    ' Starts location display with auto pan mode set to Navigation
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Navigation

                    'TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    If Not MyMapView.LocationDisplay.IsStarted Then
                        MyMapView.LocationDisplay.Start()
                    End If
                    Exit Select

                Case 3
                    ' Starts location display with auto pan mode set to Compass Navigation
                    MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.CompassNavigation

                    'TODO Remove this IsStarted check https://github.com/Esri/arcgis-runtime-samples-xamarin/issues/182
                    If Not MyMapView.LocationDisplay.IsStarted Then
                        MyMapView.LocationDisplay.Start()
                    End If
                    Exit Select
            End Select
        End Sub
    End Class
End Namespace