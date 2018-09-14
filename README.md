# asp.net.4.play

[Download and Install 4.7.2 Developer Pack](https://www.microsoft.com/net/download/visual-studio-sdks)


[OIDCPlay](./src/OIDCPlay) is a project that only has a single login, an external login.
The assumption is that the external provider's concept of the User, primarily their subject identifier is one that this app will adopt.

So no linking external accounts to yours.  


[using-a-custom-hostname-with-iis-express-with-visual-studio-2015-vs2015](http://10printhello.com/using-a-custom-hostname-with-iis-express-with-visual-studio-2015-vs2015/)  
```
<site name="OIDCPlay(1)" id="2">
        <application path="/" applicationPool="Clr4IntegratedAppPool">
          <virtualDirectory path="/" physicalPath="H:\github\ghstahl2\asp.net.4.play\src\OIDCPlay" />
        </application>
        <bindings>
          <binding protocol="https" bindingInformation="*:44344:p7core.127.0.0.1.xip.io" />
          <binding protocol="https" bindingInformation="*:44344:localhost" />
          <binding protocol="http" bindingInformation="*:56440:localhost" />
        </bindings>
      </site>
      <site name="OIDCPlay.Core" id="3">
        <application path="/" applicationPool="Clr4IntegratedAppPool">
          <virtualDirectory path="/" physicalPath="H:\github\ghstahl2\asp.net.4.play\src\OIDCPlay.Core" />
        </application>
        <bindings>
          <binding protocol="https" bindingInformation="*:44311:p7core.127.0.0.1.xip.io" />
          <binding protocol="https" bindingInformation="*:44311:localhost" />
          <binding protocol="http" bindingInformation="*:32247:localhost" />
        </bindings>
      </site>
```

I have whitelisted the following urls in;  

use the following urls to test both Google and Norton.   

Asp.Net 4 Version:     
https://p7core.127.0.0.1.xip.io:44344

Asp.Net Core 2.1 Version:  
https://p7core.127.0.0.1.xip.io:44311


