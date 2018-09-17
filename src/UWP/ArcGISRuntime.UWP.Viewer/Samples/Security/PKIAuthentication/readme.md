##PKI Authentication - Universal Windows##

This sample illustrates the use of a client certificate to access services hosted on a portal secured with Public Key Infrastructure (PKI) authentication.
Credentials are created by prompting the user to select an exported client certificate file (*.pfx). After loading the certificate, requests
to the portal can be encrypted and decrypted using public key cryptography. The portal can then be searched for web maps to display in the map view.    
     
More information about PKI and client certificates can be found at the links below:
 - [PKI - MSDN](https://msdn.microsoft.com/en-us/library/windows/desktop/bb427432(v=vs.85).aspx)
 - [PKI - Wikipedia](https://en.wikipedia.org/wiki/Public_key_infrastructure)
 - [Certificates - MSDN](https://msdn.microsoft.com/en-us/library/windows/desktop/bb540819(v=vs.85).aspx)

###You will need###
 - Access to a portal secured with PKI authentication.
 - An exported certificate file (*.pfx) that grants access to the above portal (you can create this using the Windows certificate manager).

###Project requirements###
 - The "SecuredPortalUrl" variable must be updated with a URL that points to your PKI-secured portal.

###Notes:###
 - When using client certificates, the following capabilities in Package.appxmanifest must be set:
    - Internet (Client and Server)
    - Private Networks (Client and Server) - if accessing a portal on the Intranet
