OAuth Authentication - Desktop

This sample demonstrates the use of OAuth 2.0 authorization for an ArcGIS Online user login. In this sample the user is prompted to enter their credentials which are then authenticated against the requested resource. A short-term access token is returned for initial access (default of 60 minutes) along with a refresh token (default unlimited). The refresh token is stored in the project settings, but you should save this to a secure location on the user's computer. 

The ArcGIS Runtime SDK for .NET provides an AuthenticationManager class to manage the authentication process when an app is challenged for credentials by the resource; therefore, you do not have to write code to manage (most of) this process. For more information, see https://developers.arcgis.com/authentication/what-is-oauth-2/. 

Once the user has been authenticated successfully, the AuthenticationManager stores this information until the user explicitly signs out. 

You will need the following information to run this sample. Learn more at https://developers.arcgis.com/authentication/. 
- ArcGIS Online credentials-The AuthenticationManager will prompt for username and password. 
- Client ID-An alphanumeric string that identifies the app on the server. 
- Redirect URI-An address to which a successful authentication response is sent. For a server app, this might be the URL of a web service that can accept the authorization response. You can use the a ficticious url to deliver the response to a portal URL (/oauth2/approval). Such URLs are typically used by applications that don't have a web server or a custom URI scheme where the code can be delivered. When received by your app, the response is read from the authentication success page.

Project requirements: References to System.Net.Http and System.Security
--------------------

Additional Resources:
https://developers.arcgis.com/authentication/
https://developers.arcgis.com/authentication/what-is-oauth-2/

