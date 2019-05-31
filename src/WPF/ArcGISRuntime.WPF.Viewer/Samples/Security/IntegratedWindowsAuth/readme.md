## Integrated Windows Authentication

This sample illustrates the use of Windows credentials to access services hosted on a portal secured with Integrated Windows Authentication (IWA).
When accessing resources secured with IWA from a WPF app, default credentials (the current user's username and password) are sent to the portal. 
Assuming the user has access, such an app requires no additional authentication code and secure portal resources can be accessed without
the user being prompted to sign in. For the sake of illustration, this sample shows how to prompt for a username, password, and domain in order to 
explicitly create an `ArcGISNetworkCredential`.
Platforms such as Android, iOS, and Universal Windows Platform (UWP) require credentials to be entered explicitly.

See [Use the Authentication Manager](https://developers.arcgis.com/net/latest/wpf/guide/use-the-authentication-manager.htm) in the developers guide for more information.

<img src="IntegratedWindowsAuth.jpg"/>    

     
More information about IWA can be found at the links below:
 - [IWA - Wikipedia](https://en.wikipedia.org/wiki/Integrated_Windows_Authentication)
 - [Windows Authentication for IIS](http://www.iis.net/configreference/system.webserver/security/authentication/windowsauthentication)

### You will need
 - Access to a portal secured with IWA.
 - A login that grants you access to items stored on the above portal.

### Instructions
1. Enter the URL to your IWA-secured portal.
2. Click the button to search for web maps on the secure portal.
3. You will be prompted for a user name, password, and domain to authenticate with the portal.
4. If you authenticate successfully, search results will display.
5. Select a web map in the list to display it in the map view.
