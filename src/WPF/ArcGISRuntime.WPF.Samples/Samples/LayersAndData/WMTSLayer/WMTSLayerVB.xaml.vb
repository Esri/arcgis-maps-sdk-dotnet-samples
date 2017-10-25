' Copyright 2017 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.Windows
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Ogc

Namespace WMTSLayer

    Partial Public Class WMTSLayerVB

        Public Sub New()

            InitializeComponent()

        End Sub

        Private Sub Button1_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)

            Try

                ' Define the Uri to the WMTS service
                Dim myUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer/WMTS")

                ' Create a new instance of a WMTS layer using a Uri and provide an Id value
                Dim myWmtsLayer As Esri.ArcGISRuntime.Mapping.WmtsLayer = New Esri.ArcGISRuntime.Mapping.WmtsLayer(myUri, "WorldTimeZones")

                ' Create a new map
                Dim myMap As New Map()

                ' Get the basemap from the map
                Dim myBasemap As Basemap = myMap.Basemap

                ' Get the layer collection for the base layers
                Dim myLayerCollection As LayerCollection = myBasemap.BaseLayers

                ' Add the WMTS layer to the layer collection of the map
                myLayerCollection.Add(myWmtsLayer)

                ' Assign the map to the MapView
                MyMapView.Map = myMap

            Catch ex As Exception

                MessageBox.Show(ex.ToString())

            End Try

        End Sub

        Private Async Sub Button2_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)

            Try

                ' Define the Uri to the WMTS service
                Dim myUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/WorldTimeZones/MapServer/WMTS")

                ' Define a new instance of the WMTS service
                Dim myWmtsService As WmtsService = New WmtsService(myUri)

                ' Load the WMTS service 
                Await myWmtsService.LoadAsync()

                ' Get the service information (i.e. metadata) about the WMTS service
                Dim myWMTSServiceInfo As WmtsServiceInfo = myWmtsService.ServiceInfo

                ' Obtain the read only list of WMTS layer info objects
                Dim myWmtsLayerInfos As IReadOnlyList(Of WmtsLayerInfo) = myWMTSServiceInfo.LayerInfos

                ' Create a new instance of a WMTS layer using the first item in the read only list of WMTS layer info objects
                Dim myWmtsLayer As Esri.ArcGISRuntime.Mapping.WmtsLayer = New Esri.ArcGISRuntime.Mapping.WmtsLayer(myWmtsLayerInfos(0))

                ' Create a new map
                Dim myMap As New Map()

                ' Get the basemap from the map
                Dim myBasemap As Basemap = myMap.Basemap

                ' Get the layer collection for the base layers
                Dim myLayerCollection As LayerCollection = myBasemap.BaseLayers

                ' Add the WMTS layer to the layer collection of the map
                myLayerCollection.Add(myWmtsLayer)

                ' Assign the map to the MapView
                MyMapView.Map = myMap

            Catch ex As Exception

                MessageBox.Show(ex.ToString())

            End Try

        End Sub

    End Class

End Namespace