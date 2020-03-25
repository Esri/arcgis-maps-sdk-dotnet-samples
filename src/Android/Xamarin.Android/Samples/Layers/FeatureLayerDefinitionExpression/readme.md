# Feature layer definition expression

Limit the features displayed on a map with a definition expression.

![Image of feature layer definition expression](FeatureLayerDefinitionExpression.jpg)

## Use case

Set a definition expression to filter out the features to be displayed. You might filter a dataset of tree quality selecting for only those trees which require maintenance or are damaged.

## How to use the sample

Press the 'Apply Expression' button to limit the features requested from the feature layer to those specified by the SQL query definition expression. Tap the 'Reset Expression' button to remove the definition expression on the feature layer, which returns all the records.

## How it works

1. Create a service feature table from a URL.
2. Create a feature layer from the service feature table.
3. Set the limit of the features on your feature layer using the `DefinitionExpression`.

## Relevant API

* DefinitionExpression
* FeatureLayer
* ServiceFeatureTable

## About the data

This map displays point features related to crime incidents that have been reported by city residents.

## Tags

definition expression, filter, limit data, query, restrict data, SQL, where clause