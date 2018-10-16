## Integrated Windows Authentication

This sample illustrates the use of Windows credentials to access services hosted on a portal secured with Integrated Windows Authentication (IWA).
When accessing an item secured with IWA from a WPF app, default credentials (the current user's login) are sent to the portal. 
Platforms such as Android, iOS, and Universal Windows Platform (UWP) require credentials to be entered explicitly.

<image src="IntegratedWindowsAuth.jpg"/>     

     
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

### Important
To successfully run the sample on Android, you need to use the `Managed` http handler. 
To make sure the project is using the correct handler, go to `Project > Properties > Android Options`.
Click `Advanced`, then select `Managed` from the `HttpClient implementation` drop down.