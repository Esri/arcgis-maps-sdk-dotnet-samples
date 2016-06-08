
' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping

Namespace ChangeSublayerVisibility
    Partial Public Class ChangeSublayerVisibilityVB
        Private _imageLayer As ArcGISMapImageLayer

        Public Sub New()
            InitializeComponent()

            ' Setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            ' Create new Map
            Dim myMap As New Map()

            ' Create uri to the map image layer
            Dim serviceUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/SampleWorldCities/MapServer")

            ' Create new image layer from the url
            _imageLayer = New ArcGISMapImageLayer(serviceUri) With {
                .Name = "World Cities Population"
            }

            ' Add created layer to the basemaps collection
            myMap.Basemap.BaseLayers.Add(_imageLayer)

            ' Assign the map to the MapView
            MyMapView.Map = myMap
        End Sub

        Private Async Sub OnSublayersButtonClicked(sender As Object, e As RoutedEventArgs)
            ' Make sure that layer and it's sublayers are loaded
            ' If layer is already loaded, this returns directly
            Await _imageLayer.LoadAsync()

            Dim dialog = New ContentDialog() With {
                .Title = "Sublayers",
                .FullSizeDesired = True
            }

            ' Create list for layers
            Dim sublayersListView = New ListView()

            ' Create cells for each of the sublayers
            For Each sublayer As ArcGISSublayer In _imageLayer.Sublayers
                ' Using a toggle that provides on/off functionality
                Dim toggle = New ToggleSwitch() With {
                    .Header = sublayer.Name,
                    .IsOn = sublayer.IsVisible,
                    .Margin = New Thickness(5)
                }

                ' Hook into the On/Off changed event
                AddHandler toggle.Toggled, AddressOf OnSublayerToggled

                ' Add cell into the table view
                sublayersListView.Items.Add(toggle)
            Next

            ' Set listview to the dialog
            dialog.Content = sublayersListView

            ' Show dialog as a full screen overlay. 
            Await dialog.ShowAsync()
        End Sub

        Private Sub OnSublayerToggled(sender As Object, e As RoutedEventArgs)
            Dim toggle = TryCast(sender, ToggleSwitch)

            ' Find the layer from the image layer
            Dim sublayer As ArcGISSublayer = _imageLayer.Sublayers.First(Function(x) x.Name = toggle.Header.ToString())

            ' Change sublayers visibility
            sublayer.IsVisible = toggle.IsOn
        End Sub
    End Class
End Namespace