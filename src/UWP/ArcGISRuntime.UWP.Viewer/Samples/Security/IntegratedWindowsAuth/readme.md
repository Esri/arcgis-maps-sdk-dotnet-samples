##IWA Authentication - Desktop##

This sample illustrates the use of Windows credentials to access services hosted on a portal secured with Integrated Windows Authentication (IWA).
When accessing an item secured with IWA, default credentials (the login that started the app, i.e.) are sent to the portal without the user being prompted. 
If the user's credentials are able to successfully authenticate with the portal, the process is seamless to the user. If the user's credentials do not 
permit access to the portal, your app can prompt them for another login or simply fail to load the secure item.

     
More information about IWA can be found at the links below:
 - [IWA - Wikipedia](https://en.wikipedia.org/wiki/Integrated_Windows_Authentication)
 - [Windows Authentication for IIS](http://www.iis.net/configreference/system.webserver/security/authentication/windowsauthentication)

###You will need###
 - Access to a portal secured with IWA.
 - A login that grants you access to the above portal.

###Project requirements###
 - The "SecuredPortalUrl" variable must be updated with a URL that points to your IWA-secured portal.
