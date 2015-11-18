##PKI Authentication - Desktop##

This sample illustrates the use of a client certificate to access services hosted on a portal secured with Public Key Infrastructure (PKI) authentication.
Credentials are loaded by the app using an exported certificate (.pfx) that you create and include with the app. After loading the certificate, requests
to the portal can be encrypted and decrypted using public key cryptography. The portal can then be searched for hosted items, such as web maps, to include
in your app. Once it's loaded, the app will retain the certificate until the app is uninstalled.     
     
More information about PKI and client certificates can be found at the links below:
 - [PKI - MSDN](https://msdn.microsoft.com/en-us/library/windows/desktop/bb427432(v=vs.85).aspx)
 - [PKI - Wikipedia](https://en.wikipedia.org/wiki/Public_key_infrastructure)
 - [Certificates - MSDN](https://msdn.microsoft.com/en-us/library/windows/desktop/bb540819(v=vs.85).aspx)

###You will need###
 - Access to a portal secured with PKI authentication.
 - A certificate that grants you access to the above portal.
 - A Personal Information Exchange (.pfx) file containing the certificate information (can be exported from the certificate).

###Project requirements###
 - Your .pfx file (certificate) must be added to the project's "Certificates" folder.
 - The "CertificateFileName" variable (top of MainPage.xaml.cs) must be updated with the name of the .pfx file.
 - The "SecuredPortalUrl" variable must be updated with a URL that points to your PKI-secured portal.

###Notes:###
 - When using client certificates, the following capabilities in Package.appxmanifest must be set:
    - Internet (Client and Server)
    - Shared User Certificates
    - Private Networks (Client and Server) - if accessing a portal on the Intranet
 - A client certificate can also be configured for the app using the "Certificates" declaration in Package.appxmanifest.
