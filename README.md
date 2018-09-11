# asp.net.4.play

[Download and Install 4.7.2 Developer Pack](https://www.microsoft.com/net/download/visual-studio-sdks)


[OIDCPlay](./src/OIDCPlay) is a project that only has a single login, an external login.
The assumption is that the external provider's concept of the User, primarily their subject identifier is one that this app will adopt.

So no linking external accounts to yours.  


[using-a-custom-hostname-with-iis-express-with-visual-studio-2015-vs2015](http://10printhello.com/using-a-custom-hostname-with-iis-express-with-visual-studio-2015-vs2015/)  
```
<site name="OIDCPlay(2)" id="5">
    <application path="/" applicationPool="Clr4IntegratedAppPool">
        <virtualDirectory path="/" physicalPath="H:\Github\ghstahl\asp.net.4.play\src\OIDCPlay" />
    </application>
    <bindings>
        <binding protocol="https" bindingInformation="*:44344:p7core.127.0.0.1.xip.io" />
        <binding protocol="https" bindingInformation="*:44344:localhost" />
        <binding protocol="http" bindingInformation="*:56440:localhost" />
    </bindings>
</site>
```
