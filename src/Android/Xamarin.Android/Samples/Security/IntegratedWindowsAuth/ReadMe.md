## IWA Authentication

This sample illustrates the use of Windows credentials to access services hosted on a portal secured with Integrated Windows Authentication (IWA).
When accessing an item secured with IWA, default credentials (the login that started the app, i.e.) are only sent to the portal from a WPF app. 
Platforms such as Android, iOS, and Universal Windows Platform (UWP) require credentials to be entered explicitly, either by prompting the user 
for them or by hard-coding them into the app.
The sample allows you to define a Windows credential in the code (username, password, and domain) or to prompt the user when accessing a secure resource.

     
More information about IWA can be found at the links below:
 - [IWA - Wikipedia](https://en.wikipedia.org/wiki/Integrated_Windows_Authentication)
 - [Windows Authentication for IIS](http://www.iis.net/configreference/system.webserver/security/authentication/windowsauthentication)

### You will need
 - Access to a portal secured with IWA.
 - A login that grants you access to items stored on the above portal.

### Project requirements
 - The "SecuredPortalUrl" variable must be updated with a URL that points to your IWA-secured portal.
 - The "WebMapId" variable must be updated with the ID of a web map (portal item) stored on the portal.
