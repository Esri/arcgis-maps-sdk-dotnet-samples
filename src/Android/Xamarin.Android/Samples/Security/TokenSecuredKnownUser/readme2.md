# ArcGIS token with a known user

This sample demonstrates how to authenticate with ArcGIS Server using ArcGIS Tokens to access a secure service. Accessing secured services requires a login that's been defined on the server.

<img src="TokenSecuredKnownUser.jpg" width="350"/>

## Instructions

1. When you run the sample, the app will load a map that contains a layer from a secured service.
2. You will NOT be challenged for a user name and password to view that layer because that info has been hard-coded into the app.
3. If the credentials in the code are correct, the secured layer will display, otherwise the map will contain only the public layers.
