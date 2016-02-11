'Copyright 2015 Esri.
'
'Licensed under the Apache License, Version 2.0 (the "License");
'you may not use this file except in compliance with the License.
'You may obtain a copy of the License at
'
'http://www.apache.org/licenses/LICENSE-2.0
'
'Unless required by applicable law or agreed to in writing, software
'distributed under the License is distributed on an "AS IS" BASIS,
'WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
'See the License for the specific language governing permissions and
'limitations under the License.
Imports Esri.ArcGISRuntime
Imports Esri.ArcGISRuntime.Mapping
Imports System.Windows

Namespace OpenExistingMap

    Partial Public Class OpenExistingMapVB

        ' Below are three URLs corresponding to Portal Items. In this example the items are Maps. 
        Private itemURL1 As String = "http://www.arcgis.com/home/item.html?id=2d6fa24b357d427f9c737774e7b0f977"
        Private itemURL2 As String = "http://www.arcgis.com/home/item.html?id=01f052c8995e4b9e889d73c3e210ebe3"
        Private itemURL3 As String = "http://www.arcgis.com/home/item.html?id=74a8f6645ab44c4f82d537f1aa0e375d"

        ' Corresponding Title strings for the itemUrls.  
        Private title1 As String = "Housing with Mortgages"
        Private title2 As String = "USA Tapestry Segmentation"
        Private title3 As String = "Geology of United States"

        ' Construct Load Map sample control.
        Public Sub New()

            InitializeComponent()
            AddHandler Loaded, AddressOf OnLoaded

        End Sub

        ' Loads UI elements and an initial Map.
        Private Async Sub OnLoaded(sender As Object, e As RoutedEventArgs)

            ' Adding items' Titles and URLs to a collection that will be used to populate the combobox's drop down. 
            Dim comboBoxContent As ICollection(Of KeyValuePair(Of [String], [String])) =
                New Dictionary(Of [String], [String])() From
                {
                    {title1, itemURL1},
                    {title2, itemURL2},
                    {title3, itemURL3}
                }

            Try
                comboMap.ItemsSource = comboBoxContent
                comboMap.SelectedIndex = 0
                Await LoadMapAsync(comboBoxContent.FirstOrDefault().Value)
            Catch ex As Exception
                Dim errorMessage = "Map cannot be loaded. " + ex.Message
                MessageBox.Show(errorMessage, "Sample error")
            End Try

        End Sub

        ' Loads a webmap on load button click.
        Private Async Sub OnLoadButtonClicked(sender As Object, e As RoutedEventArgs)

            Dim url As String = String.Empty

            If comboMap.SelectedIndex >= 0 Then
                Try
                    url = TryCast(comboMap.SelectedValue, String)
                    Await LoadMapAsync(url)
                Catch ex As Exception
                    Dim errorMessage = "Map cannot be loaded." + ex.Message
                    MessageBox.Show(errorMessage, "Sample error")
                End Try

            End If
        End Sub

        ' Loads the given map.
        Private Async Function LoadMapAsync(mapUrl As String) As Task

            progress.Visibility = Visibility.Visible

            ' Initialize map from a portal item URI and load map into MapView. 
            Dim map = New Map(New Uri(mapUrl))
            ' Await LoadAsync so all properties of map will definitely be loaded when interrogated later. i.e Map.PortalItem. 
            Await map.LoadAsync()
            MyMapView.Map = map

            ' Get map's info to populate "Map Details" UI element.          
            Dim item = MyMapView.Map.PortalItem
            detailsPanel.DataContext = item

            detailsPanel.Visibility = Visibility.Visible
            progress.Visibility = Visibility.Hidden
        End Function

    End Class
End Namespace


