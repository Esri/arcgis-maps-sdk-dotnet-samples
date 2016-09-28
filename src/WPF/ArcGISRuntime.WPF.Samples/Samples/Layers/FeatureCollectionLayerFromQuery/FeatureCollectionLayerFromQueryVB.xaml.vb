' Copyright 2016 Esri.
'
' Licensed under the Apache License, Version 2.0 (the "License") you may not use this file except in compliance with the License.
' You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
'
' Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
' "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
' language governing permissions and limitations under the License.

Imports System.Windows
Imports Esri.ArcGISRuntime.Data
Imports Esri.ArcGISRuntime.Layers
Imports Esri.ArcGISRuntime.Mapping

Namespace FeatureCollectionLayerFromQuery
    Partial Public Class FeatureCollectionLayerFromQueryVB
        Private Const FeatureLayerUrl As String = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Wildfire/FeatureServer/0"

        Public Sub New()
            InitializeComponent()

            ' Setup the control references and execute initialization 
            Initialize()
        End Sub

        Private Sub Initialize()
            Try
                ' Create a New map with the oceans basemap And add it to the map view
                Dim myMap = New Map(Basemap.CreateOceans())
                MyMapView.Map = myMap

                '  Call a function that will create a new feature collection layer from a service query
                GetFeaturesFromQuery()
            Catch ex As Exception
                MessageBox.Show("Unable to create feature collection layer: " + ex.Message, "Error")
            End Try
        End Sub

        Private Async Sub GetFeaturesFromQuery()
            ' Create a service feature table to get features from
            Dim feachurTable As ServiceFeatureTable = New ServiceFeatureTable(New Uri(FeatureLayerUrl))

            ' Create a query to get all features in the table
            Dim kweryParams As QueryParameters = New QueryParameters()
            kweryParams.WhereClause = "1=1"

            ' Query the table to get all features
            Dim kweryResult As FeatureQueryResult = Await feachurTable.QueryFeaturesAsync(kweryParams)

            ' Create a New feature collection table from the result features
            Dim klectionTable As FeatureCollectionTable = New FeatureCollectionTable(kweryResult)

            ' Create a feature collection And add the table
            Dim feachurKlection As FeatureCollection = New FeatureCollection()
            feachurKlection.Tables.Add(klectionTable)

            ' Create a layer to display the feature collection, add it to the map's operational layers
            Dim feachurKlectionTable As FeatureCollectionLayer = New FeatureCollectionLayer(feachurKlection)
            MyMapView.Map.OperationalLayers.Add(feachurKlectionTable)
        End Sub
    End Class
End Namespace