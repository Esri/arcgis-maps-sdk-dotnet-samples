##PKI Authentication - Desktop##

This sample illustrates the use of a client certificate to access services hosted on a portal secured with Public Key Infrastructure (PKI) authentication.
Credentials are created by prompting the user to select a client certificate installed on the local machine. After loading the certificate, requests
to the portal can be encrypted and decrypted using public key cryptography. The portal can then be searched for web maps to display in the map view.    
     
More information about PKI and client certificates can be found at the links below:
 - [PKI - MSDN](https://msdn.microsoft.com/en-us/library/windows/desktop/bb427432(v=vs.85).aspx)
 - [PKI - Wikipedia](https://en.wikipedia.org/wiki/Public_key_infrastructure)
 - [Certificates - MSDN](https://msdn.microsoft.com/en-us/library/windows/desktop/bb540819(v=vs.85).aspx)

###You will need###
 - Access to a portal secured with PKI authentication.
 - A certificate that grants access to the above portal (installed in the current user's certificate store).

###Project requirements###
 - The "SecuredPortalUrl" variable must be updated with a URL that points to your PKI-secured portal.

