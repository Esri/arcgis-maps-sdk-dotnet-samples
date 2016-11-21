####Token Secured Services (Challenge method)####

This sample demonstrates accessing secured services using ArcGIS token-based authentication. 
Token-based authentication is supported for ArcGIS Server, ArcGIS Online, and Portal for ArcGIS version 10.2 or earlier. 
The application prompts for a username and password to access a secured service. If the credentials are valid, the response provides a token to include with requests for secured content on the portal.

With the ArcGIS Runtime SDK for .NET, you can use the AuthenticationManager to challenge for credentials when the Map tries to access a secure service.

--------------------

Additional Resources:    
 - https://developers.arcgis.com/authentication/    
 - https://developers.arcgis.com/net/desktop/guide/use-arcgis-token-authentication.htm