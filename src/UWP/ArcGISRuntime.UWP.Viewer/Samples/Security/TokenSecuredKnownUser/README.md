Token Secured Services (Known User)

This sample demonstrates accessing secured services using ArcGIS token-based authentication. 
Token-based authentication is supported for ArcGIS Server, ArcGIS Online, and Portal for ArcGIS version 10.2 or earlier. 

In this sample, a hard-coded username and password is used to get an ArcGIS token for accessing a secured resource. 
This process is seamless to the user, since no dialog appears to prompt for the username and password. This sample provides a basic example, in a real-world sceneario, better 
protection of the username and password would be required (calling a secured web service to get the token, for example).
To see an example of using the ArcGIS Runtime SDK for .NET AuthenticationManager to prompt for credentials, see the Token Secured Services (Challenge method) sample. 

--------------------

Additional Resources:
https://developers.arcgis.com/authentication/
https://developers.arcgis.com/net/desktop/guide/use-arcgis-token-authentication.htm