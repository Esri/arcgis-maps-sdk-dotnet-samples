# ArcGIS token challenge

This sample demonstrates how to prompt the user for a username and password to authenticate with ArcGIS Server to access an ArcGIS token-secured service. Accessing secured services requires a login that's been defined on the server.

![screenshot](TokenSecuredChallenge.jpg)

## Use case

Your app may need to access services that are restricted to authorized users. For example, your organization may host ArcGIS services that are only accessible by verified users.

## How it works

1. A custom `ChallengeHandler` is set for `AuthenticationManager` that displays a login dialog for entering a username and password.
2. In response to the attempt to access secured content, the `AuthenticationManager` calls the challenge handler.
3. A `TokenCredential` is created from the entered username and password, and an attempt is made to load the layer.

## Relevant API

* AuthenticationManager
* TokenCredential

## Tags

Authentication, Security, Token
