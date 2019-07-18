# Search for webmap

Find webmap portal items by using a search term.

![screenshot](SearchPortalMaps.jpg)

## Use case

Portals can contain many portal items and at times you may wish to query the portal to find what you're looking for. In this example, we search for webmap portal items using a text search.

## How to use the sample

Enter search terms into the search bar. Once the search is complete, a list is populated with the resultant webmaps. Tap on a webmap to set it to the map view.

## How it works

1. Create a new `Portal` and load it.
2. Create new `PortalItemQueryParameters`. Set the type to `PortalItem.Type.WebMap` and add the text you want to search for.
3. Use `portal.FindItemsAsync(params)` to find matching items.

## Relevant API

* Portal
* PortalItem
* PortalQueryParameters
* PortalQueryResultSet

## Tags

keyword, query, search, webmap
