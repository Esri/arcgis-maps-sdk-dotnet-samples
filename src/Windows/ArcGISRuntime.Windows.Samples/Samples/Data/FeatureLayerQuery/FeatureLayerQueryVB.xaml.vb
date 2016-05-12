' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License") you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http:'www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Geometry
Imports Esri.ArcGISRuntime.Mapping
Imports Esri.ArcGISRuntime.Symbology
Imports Windows.UI
Imports Windows.UI.Popups

Namespace FeatureLayerQuery
    Partial Public Class FeatureLayerQueryVB

        ' Create reference to service of US States  
        Private _statesUrl As String = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/2"

        ' Create globally available feature table for easy referencing 
        Private _featureTable As ServiceFeatureTable

        ' Create globally available feature layer for easy referencing 
        Private _featureLayer As FeatureLayer

        Public Sub New()
            InitializeComponent()

            'Call a function that will create a basemap, the US states layer, and load a new map into the map view
            InitializeMap()
        End Sub


        Private Sub InitializeMap()
            ' Create a New Map with the Topographic basemap
            Dim myMap As Map = New Map(Basemap.CreateTopographic())

            ' Set an initial map location that shows the entire United States
            Dim initialLocation As MapPoint = New MapPoint(-11000000, 5000000, SpatialReferences.WebMercator)
            myMap.InitialViewpoint = New Viewpoint(initialLocation, 100000000)

            ' Create a feature table using the US states service endpoint url
            _featureTable = New ServiceFeatureTable(New Uri(_statesUrl))

            ' Create a feature layer using this feature table
            _featureLayer = New FeatureLayer(_featureTable)

            ' Set the Opacity of the Feature Layer so it's semi-transparent
            _featureLayer.Opacity = 0.6

            ' Create a line symbol to use for state boundaries
            Dim lineSymbol As SimpleLineSymbol = New SimpleLineSymbol
            With lineSymbol
                .Style = SimpleLineSymbolStyle.Solid
                .Color = Colors.Black
                .Width = 1
            End With
            ' Create a fill symbol that uses the line symbol for the outline
            Dim fillSymbol As SimpleFillSymbol = New SimpleFillSymbol
            With fillSymbol
                .Style = SimpleFillSymbolStyle.Solid
                .Color = Colors.Yellow
                .Outline = lineSymbol
            End With

            ' Apply the fill symbol to the States feature layer using a simple renderer
            _featureLayer.Renderer = New SimpleRenderer(fillSymbol)

            ' Define selection symbol properties for the layer (color And line width)
            _featureLayer.SelectionColor = Colors.Red
            _featureLayer.SelectionWidth = 10

            ' Add the feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer)

            ' Assign the map to the MapView
            MyMapView.Map = myMap
        End Sub

        ' Handler for the QueryStatesButton button click event
        Private Sub OnQueryClicked(sender As Object, e As RoutedEventArgs)
            ' Remove any previous feature selections And messages
            _featureLayer.ClearSelection()
            MessagesTextBlock.Text = String.Empty

            ' Call a function that will query states using the text entered 
            QueryStatesTable(StateNameTextBox.Text)
        End Sub

        ' Query features in the US States layer using STATE_NAME attribute values
        Private Async Sub QueryStatesTable(stateName As String)
            Try
                ' Create a QueryParameters object to define the query  
                Dim queryParams As QueryParameters = New QueryParameters()

                ' Assign a where clause that finds features with a matching STATE_NAME value
                queryParams.WhereClause = "upper(STATE_NAME) = '" + (stateName.ToUpper().Trim() + "'")

                ' Restrict the query results to a single feature
                queryParams.MaxFeatures = 1

                ' Query the feature table using the QueryParameters
                Dim queryResult As FeatureQueryResult = Await _featureTable.QueryFeaturesAsync(queryParams)

                ' Check for a valid feature in the results
                Dim stateResult As Feature = queryResult.FirstOrDefault()

                ' If a result was found, select it in the map And zoom to its extent
                If Not stateResult Is Nothing Then
                    ' Select (highlight) the matching state in the layer
                    _featureLayer.SelectFeature(stateResult)

                    ' Zoom to the extent of the feature (with 100 pixels of padding around the shape)
                    Await MyMapView.SetViewpointGeometryAsync(stateResult.Geometry.Extent, 100)
                Else
                    ' Inform the user that the query was unsuccessful
                    MessagesTextBlock.Text = stateName + " was not found."
                End If
            Catch ex As Exception
                ' Inform the user that an exception was encountered
                Dim message As New MessageDialog("An error occurred: " + ex.ToString(), "Sample error")
                message.ShowAsync()
            End Try
        End Sub
    End Class
End Namespace