' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.Windows
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.UI

Namespace TakeScreenshot
    Partial Public Class TakeScreenshotVB
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
        End Sub

        Private Async Sub OnScreenshotButtonClicked(sender As Object, e As RoutedEventArgs)
            ' Export the image from mapview and assign it to the imageview
            imageView.Source = Await RuntimeImageExtensions.ToImageSourceAsync(Await MyMapView.ExportImageAsync())
        End Sub
    End Class
End Namespace