
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
Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Symbology
Imports System.Windows.Media

Namespace UniqueValueRenderer

    Partial Public Class UniqueValueRendererVB
        ' Url for the USA States (polygon) layer
        Private Const StatesServiceUrl As String = "https://sampleserver6.arcgisonline.com/arcgis/rest/services/Census/MapServer/3"

        Public Sub New()
            InitializeComponent()

            ' Handle the spatial reference changed event to add a new layer with a unique value renderer
            AddHandler MyMapView.SpatialReferenceChanged, AddressOf ShowRegionsWithUniqueValues
        End Sub

        ' Add a new layer with some US States symbolized by region
        Private Sub ShowRegionsWithUniqueValues(sender As Object, e As System.EventArgs)
            ' Create a new service feature table using the USA States url
            Dim statesFeatureTable = New ServiceFeatureTable(New System.Uri(StatesServiceUrl))

            ' Create a new feature layer using the service feature table
            Dim statesLayer = New FeatureLayer(statesFeatureTable)

            ' Create a new unique value renderer
            Dim regionRenderer = New Esri.ArcGISRuntime.Symbology.UniqueValueRenderer()
            ' Add the "SUB_REGION" field to the renderer
            regionRenderer.FieldNames.Add("SUB_REGION")

            ' Define a line symbol to use for the region fill symbols
            Dim stateOutlineSymbol = New SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.White, 0.7)
            ' Define distinct fill symbols for a few regions (use the same outline symbol)
            Dim pacificFillSymbol = New SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.Blue, stateOutlineSymbol)
            Dim mountainFillSymbol = New SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.LawnGreen, stateOutlineSymbol)
            Dim westSouthCentralFillSymbol = New SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Colors.SandyBrown, stateOutlineSymbol)

            ' Add values to the renderer: define the label, description, symbol, and attribute value for each
            regionRenderer.UniqueValues.Add(New UniqueValue("Pacific", "Pacific Region", pacificFillSymbol, "Pacific"))
            regionRenderer.UniqueValues.Add(New UniqueValue("Mountain", "Rocky Mountain Region", mountainFillSymbol, "Mountain"))
            regionRenderer.UniqueValues.Add(New UniqueValue("West South Central", "West South Central Region", westSouthCentralFillSymbol, "West South Central"))

            ' Set the default region fill symbol (transparent with no outline) for regions not explicitly defined in the renderer
            Dim defaultFillSymbol = New SimpleFillSymbol(SimpleFillSymbolStyle.Null, Colors.Transparent, Nothing)
            regionRenderer.DefaultSymbol = defaultFillSymbol
            regionRenderer.DefaultLabel = "Other"

            ' Apply the unique value renderer to the states layer
            statesLayer.Renderer = regionRenderer
            ' Add the new layer to the map
            MyMapView.Map.OperationalLayers.Add(statesLayer)
        End Sub
    End Class
End Namespace

