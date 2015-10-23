OAuth Authentication - Phone

This sample demonstrates the use of OAuth 2.0 authorization for an ArcGIS Online user login. The [Web Authentication Broker](https://msdn.microsoft.com/en-us/library/windows/apps/xaml/hh750287.aspx) enables single sign-on and is shown in this sample. If you cannot use the Web Authentication Broker, you can use the OAuthAuthorizeWebView helper class to display the login information in a web view. 

In this sample the user is prompted to enter their credentials which are then authenticated against the requested resource. A short-term access token is returned for initial access (default of 60 minutes) along with a refresh token (default unlimited). The refresh token is stored in the project settings, but you would want to save this to a secure location on the user's computer. 

You can choose to use the WebAuthenticationBroker or a WebBrowser for prompting the user for credentials. 

The ArcGIS Runtime SDK for .NET provides an IdentityManager class to manage the authentication process when an app is challenged for credentials by the resource; therefore, you do not have to write code to manage this process. For more information, see https://developers.arcgis.com/net/desktop/guide/use-oauth-2-0-authentication.htm. 

Once the user has been authenticated successfully, the IdentityManager stores this information until the user explicitly signs out. 

You will need the following information to run this sample. Learn more at https://developers.arcgis.com/authentication/. 
- ArcGIS Online credentials-The Identity Manager will prompt for username and password. 
- Client ID-An alphanumeric string that identifies the app on the server. 
- Redirect URI-An address to which a successful authentication response is sent. For a server app, this might be the URL of a web service that can accept the authorization response. You can use the special value of urn:ietf:wg:oauth:2.0:oob to deliver the response to a portal URL (/oauth2/approval). This value is typically used by applications that don't have a web server or a custom URI scheme where the code can be delivered. When received by your app, the response is read from the authentication success page.

Project requirements: References to System.Net.Http and System.Security
--------------------

Additional Resources:
https://developers.arcgis.com/authentication/
https://developers.arcgis.com/authentication/what-is-oauth-2/

