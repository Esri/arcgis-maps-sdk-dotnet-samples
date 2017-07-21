' Copyright 2017 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Rasters
Imports Esri.ArcGISRuntime.ArcGISServices

Namespace RasterLayerImageServiceRaster

    Partial Public Class RasterLayerImageServiceRasterVB

        Public Sub New()

            InitializeComponent()

            ' Setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Async Sub Initialize()

            ' Create new map with the dark gray canvas basemap
            Dim myMap As New Map(Basemap.CreateDarkGrayCanvasVector)

            ' Create a Uri to the image service raster
            Dim myUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NLCDLandCover2001/ImageServer")

            ' Create new image service raster from the Uri
            Dim myImageServiceRaster As ImageServiceRaster = New ImageServiceRaster(myUri)

            ' Load the image service raster
            Await myImageServiceRaster.LoadAsync()

            ' Get the service information (aka. metadata) about the image service raster
            Dim myArcGISImageServiceInfo As ArcGISImageServiceInfo = myImageServiceRaster.ServiceInfo

            ' Create a new raster layer from the image service raster
            Dim myRasterLayer As RasterLayer = New RasterLayer(myImageServiceRaster)

            ' Add the raster layer to the maps layer collection
            myMap.Basemap.BaseLayers.Add(myRasterLayer)

            ' Assign the map to the map view
            MyMapView.Map = myMap

            ' Zoom the map to the extent of the image service raster (which also the extent of the raster layer)
            Await MyMapView.SetViewpointGeometryAsync(myArcGISImageServiceInfo.FullExtent)

            ' NOTE: The sample zooms to the extent of the ImageServiceRaster. Currently the ArcGIS Runtime does not 
            ' support zooming a RasterLayer out beyond 4 times it's published level of detail. The sample uses 
            ' MapView.SetViewpointCenterAsync() method to ensure the image shows when the app starts. You can see 
            ' the effect of the image service not showing when you zoom out to the full extent of the image and beyond.

        End Sub

    End Class

End Namespace