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

Namespace RasterLayerRasterFunction

    Partial Public Class RasterLayerRasterFunctionVB

        Public Sub New()

            InitializeComponent()

            ' Create the UI, setup the control references and execute initialization 
            Initialize()

        End Sub

        Private Async Sub Initialize()

            ' Create new map with the streets basemap
            Dim myMap As New Map(Basemap.CreateStreets)

            ' Create a Uri to the image service raster
            Dim myUri = New Uri("http://sampleserver6.arcgisonline.com/arcgis/rest/services/NLCDLandCover2001/ImageServer")

            ' Create new image service raster from the Uri
            Dim myImageServiceRaster As ImageServiceRaster = New ImageServiceRaster(myUri)

            ' Load the image service raster
            Await myImageServiceRaster.LoadAsync()

            ' NOTE: This is the ASCII text for actual raw JSON string:
            ' ========================================================
            '{
            '  "raster_function_arguments":
            '  {
            '    "z_factor":{"double":25.0,"type":"Raster_function_variable"},
            '    "slope_type":{"raster_slope_type":"none","type":"Raster_function_variable"},
            '    "azimuth":{"double":315,"type":"Raster_function_variable"},
            '    "altitude":{"double":45,"type":"Raster_function_variable"},
            '    "type":"Raster_function_arguments",
            '    "raster":{"name":"raster","is_raster":true,"type":"Raster_function_variable"},
            '    "nbits":{"int":8,"type":"Raster_function_variable"}
            '  },
            '  "raster_function":{"type":"Hillshade_function"},
            '  "type":"Raster_function_template"
            '}

            ' Use a string builder to load in the ASCII text of the JSON string
            Dim myStringBuilder As Text.StringBuilder = New Text.StringBuilder
            myStringBuilder.AppendLine("{")
            myStringBuilder.AppendLine("  " + """" + "raster_function_arguments" + """" + ":")
            myStringBuilder.AppendLine("  {")
            myStringBuilder.AppendLine("    " + """" + "z_factor" + """" + ":{" + """" + "double" + """" + ":25.0," + """" + "type" + """" + ":" + """" + "Raster_function_variable" + """" + "},")
            myStringBuilder.AppendLine("    " + """" + "slope_type" + """" + ":{" + """" + "raster_slope_type" + """" + ":" + """" + "none" + """" + "," + """" + "type" + """" + ":" + """" + "Raster_function_variable" + """" + "},")
            myStringBuilder.AppendLine("    " + """" + "azimuth" + """" + ":{" + """" + "double" + """" + ":315," + """" + "type" + """" + ":" + """" + "Raster_function_variable" + """" + "},")
            myStringBuilder.AppendLine("    " + """" + "altitude" + """" + ":{" + """" + "double" + """" + ":45," + """" + "type" + """" + ":" + """" + "Raster_function_variable" + """" + "},")
            myStringBuilder.AppendLine("    " + """" + "type" + """" + ":" + """" + "Raster_function_arguments" + """" + ",")
            myStringBuilder.AppendLine("    " + """" + "raster" + """" + ":{" + """" + "name" + """" + ":" + """" + "raster" + """" + "," + """" + "is_raster" + """" + ":true," + """" + "type" + """" + ":" + """" + "Raster_function_variable" + """" + "},")
            myStringBuilder.AppendLine("    " + """" + "nbits" + """" + ":{" + """" + "int" + """" + ":8," + """" + "type" + """" + ":" + """" + "Raster_function_variable" + """" + "}")
            myStringBuilder.AppendLine("  },")
            myStringBuilder.AppendLine("  " + """" + "raster_function" + """" + ":{" + """" + "type" + """" + ":" + """" + "Hillshade_function" + """" + "},")
            myStringBuilder.AppendLine("  " + """" + "type" + """" + ":" + """" + "Raster_function_template" + """")
            myStringBuilder.AppendLine("}")

            ' Get the JSON string from the string builder
            Dim theJSON_String As String = myStringBuilder.ToString

            ' Create a raster function from the JSON string using the static/Shared method called: RasterFunction.FromJson(json as String)
            Dim myRasterFunction As RasterFunction = RasterFunction.FromJson(theJSON_String)

            ' NOTE: Depending on your platform/device, you could have alternatively created the raster function via a JSON string that is contained in a 
            ' file on disk (ex: hillshade_simplified.json) via the constructor: Esri.ArcGISRuntime.Rasters.RasterFuntion(path as String)

            ' Get the raster function arguments
            Dim myRasterFunctionArguments As RasterFunctionArguments = myRasterFunction.Arguments

            ' Get the list of names from the raster function arguments
            Dim myRasterNames As IReadOnlyList(Of String) = myRasterFunctionArguments.GetRasterNames()

            ' Apply the first raster name and image service raster in the raster function arguments
            myRasterFunctionArguments.SetRaster(myRasterNames.Item(0), myImageServiceRaster)

            ' Create a new raster based on the raster function
            Dim myRaster As Raster = New Raster(myRasterFunction)

            ' Create a new raster layer from the raster
            Dim myRasterLayer As RasterLayer = New RasterLayer(myRaster)

            ' Add the raster layer to the maps layer collection
            myMap.Basemap.BaseLayers.Add(myRasterLayer)

            ' Assign the map to the map view
            MyMapView.Map = myMap

            ' Get the service information (aka. metadata) about the image service raster
            Dim myArcGISImageServiceInfo As ArcGISImageServiceInfo = myImageServiceRaster.ServiceInfo

            ' Zoom the map to the extent of the image service raster (which also the extent of the raster layer)
            Await MyMapView.SetViewpointGeometryAsync(myArcGISImageServiceInfo.FullExtent)

            ' NOTE: The sample zooms to the extent of the ImageServiceRaster. Currently the ArcGIS Runtime does not 
            ' support zooming a RasterLayer out beyond 4 times it's published level of detail. The sample uses 
            ' MapView.SetViewpointCenterAsync() method to ensure the image shows when the app starts. You can see 
            ' the effect of the image service not showing when you zoom out to the full extent of the image and beyond.

        End Sub

    End Class

End Namespace