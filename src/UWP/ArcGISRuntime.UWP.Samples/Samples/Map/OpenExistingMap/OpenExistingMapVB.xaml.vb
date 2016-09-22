
' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping

Namespace OpenExistingMap
    Partial Public Class OpenExistingMapVB
        ' String array to hold urls to publicly available web maps
        Private _itemURLs As String() = New String() {
            "http://www.arcgis.com/home/item.html?id=2d6fa24b357d427f9c737774e7b0f977",
            "http://www.arcgis.com/home/item.html?id=01f052c8995e4b9e889d73c3e210ebe3",
            "http://www.arcgis.com/home/item.html?id=74a8f6645ab44c4f82d537f1aa0e375d"}

        ' String array to store titles for the webmaps specified above. These titles are in the same order as the urls above
        Private _titles As String() = New String() {
            "Housing with Mortgages",
            "USA Tapestry Segmentation",
            "Geology of United States"}

        Public Sub New()
            InitializeComponent()

            ' Setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            ' Create a new Map instance with url of the webmap that is displayed by default
            Dim myMap As New Map(New Uri(_itemURLs(0)))

            ' Provide used Map to the MapView
            MyMapView.Map = myMap

            ' Set titles as a items source
            mapsChooser.ItemsSource = _titles
        End Sub

        Private Sub OnMapsChooseSelectionChanged(sender As Object, e As SelectionChangedEventArgs)
            Dim selectedMap = e.AddedItems(0).ToString()

            ' Get index that is used to get the selected url
            Dim selectedIndex = _titles.ToList().IndexOf(selectedMap)

            ' Create a new Map instance with url of the webmap that selected
            MyMapView.Map = New Map(New Uri(_itemURLs(selectedIndex)))
        End Sub
    End Class
End Namespace
