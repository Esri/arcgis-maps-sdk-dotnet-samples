# Statistical query

Query a table to get aggregated statistics back for a specific field.

![screenshot](StatisticalQuery.jpg)

## Use case

For example, a county boundaries table with population information can be queried to return aggregated results for total, average, maximum, and minimum population, rather than downloading the values for every county and calculating statistics manually.

## How to use the sample

Pan and zoom to define the extent for the query. Click 'Get statistics' and choose one of the options.

## How it works

1. Create a `ServiceFeatureTable` with a URL to the feature service.
2. Create `StatisticsQueryParameters`, and `StatisticDefinition` objects, and add to the parameters.
3. Execute `QueryStatistics` on the `ServiceFeatureTable`. Depending on the state of the two checkboxes, additional parameters are set.
4. Display each `StatisticRecord` in the first returned `QueryStatisticsResult`.

## Relevant API

* QueryParameters
* ServiceFeatureTable
* StatisticDefinition
* StatisticRecord
* StatisticType
* StatisticsQueryParameters
* StatisticsQueryResult

## Tags

analysis, average, bounding geometry, filter, intersect, maximum, mean, minimum, query, spatial query, standard deviation, statistics, sum, variance
