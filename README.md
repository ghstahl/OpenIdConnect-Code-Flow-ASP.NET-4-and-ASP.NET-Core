# OpenIdConnect-Code-Flow-ASP.NET-4-and-ASP.NET-Core

[Download and Install 4.7.2 Developer Pack](https://www.microsoft.com/net/download/visual-studio-sdks)


[OIDCPlay](./src/OIDCPlay) is a project to demonstrate the code flows that were added to the [Microsoft.Owin.Security.OpenIdConnect](https://github.com/aspnet/AspNetKatana/tree/dev/src/Microsoft.Owin.Security.OpenIdConnect) project.  The modified library exists here [P7.Microsoft.Owin.Security.OpenIdConnect](https://github.com/ghstahl/OpenIdConnect-Code-Flow-ASP.NET-4-and-ASP.NET-Core/tree/master/src/P7.Microsoft.Owin.Security.OpenIdConnect) 

Changes:

The original library only let the code flow through if the provider posted back the AuthorizationCode and id_token.  That was removed.  
The original library only allowed a POST, which has been corrected.   As in the asp.net core version, GET is now supported.  
If we get an AuthorizationCode, I use the [IdentityModel](https://github.com/IdentityModel) library to redeem the AuthorizationCode.  


Changed AuthorizationCodeReceivedNotification to AuthorizationCodeRedeemedNotification, because I am making the back channel call from inside the library, thus burning that one time AuthorizationCode.  I event out mainly so that you can harvest the tokens that I got from redeeming the AuthorizationCode.  
```
app.UseOpenIdConnectAuthentication(new OpenIdConnectAuthenticationOptions
{
    Caption = "Google",
    AuthenticationType = "Google",
    ClientId = "1096301616546-edbl612881t7rkpljp3qa3juminskulo.apps.googleusercontent.com",
    ClientSecret = "gOKwmN181CgsnQQDWqTSZjFs",
    Authority = "https://accounts.google.com/",
//                ResponseType = OpenIdConnectResponseType.IdToken,// Works as well, just no access_tokens  
    ResponseType = OpenIdConnectResponseType.Code,
    Scope = "openid email",
    UseTokenLifetime = false,
    RedirectUri = "https://p7core.127.0.0.1.xip.io:44344/signin-google",
    Notifications = new OpenIdConnectAuthenticationNotifications()
    {
        AuthorizationCodeRedeemed= async n =>
        {
            var ticket = n.AuthenticationTicket;
            // store tokens for later use
            var idToken = ticket.Properties.GetTokenValue("id_token");
            var accessToken = ticket.Properties.GetTokenValue("access_token");
            var refreshToken = ticket.Properties.GetTokenValue("refresh_token");
        }
    }
});
```  


Thats about it.



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


