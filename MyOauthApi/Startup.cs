using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;

using Owin;

[assembly: OwinStartup(typeof(MyOauthApi.Startup))]

namespace MyOauthApi
{
    public class Startup
    {
        private readonly ConcurrentDictionary<string, string> _authenticationCodes = new ConcurrentDictionary<string, string>(StringComparer.Ordinal);

        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
            ConfigAuth(app);

            HttpConfiguration config = new HttpConfiguration();
            //WebApiConfig.Register(config);
            app.UseCors(CorsOptions.AllowAll);
            app.UseWebApi(config);
        }





        public void ConfigAuth(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 https://go.microsoft.com/fwlink/?LinkID=316888

            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            //创建OAuth授权服务器
            app.UseOAuthAuthorizationServer(new OAuthAuthorizationServerOptions()
            {
                AuthenticationType = "Bearer",
                AllowInsecureHttp = true,                           //允许客户端使用http协议请求
                TokenEndpointPath = new PathString("/token"),       //请求地址
                AccessTokenExpireTimeSpan = TimeSpan.FromMinutes(1),   //token过期时间
                //提供认证策略  处理由授权服务器中间件引发事件应用程序提供的对象。 应用程序可能完全实现接口，或它可能会造成的实例OAuthAuthorizationServerProvider并分配此服务器支持的 OAuth 流所需的委托。
                Provider = new OAuthAuthorizationServerProvider()
                {
                    OnGrantResourceOwnerCredentials = GrantResourceOwnerCredentials,    //客户端通过包括从资源所有者处收到的凭据从授权服务器的令牌终结点请求访问令牌。 当发出请求，客户端进行身份验证与授权服务器。身份验证模式，就是账号密码
                    OnGrantClientCredentials = GrantClientCredentials,                  //客户端向授权服务器进行身份验证，并从令牌终结点请求访问令牌。客户端模式
                    OnValidateClientRedirectUri = ValidateClientRedirectUri,            //用来验证其注册的重定向 URL 的客户端。防钓鱼
                    OnValidateClientAuthentication = ValidateClientAuthentication,      //检查的基本方案标头和窗体正文以获取客户端的凭据
                  
                },
                //生成一次性授权代码返回到客户端应用程序。 为 OAuth 服务器要保护的应用程序必须提供的一个实例AuthorizationCodeProvider由生成的令牌
                AuthorizationCodeProvider = new AuthenticationTokenProvider
                {
                    OnCreate = CreateAuthenticationCode,
                    OnReceive = ReceiveAuthenticationCode,
                },
                //生成acces_token 
                AccessTokenProvider = new AuthenticationTokenProvider()
                {
                    OnCreate = CreateAccessToken,
                    OnReceive = ReceiveAccessToken,
                },
                // 生成可用于生成新的访问令牌时所需的刷新令牌。 如果未提供授权服务器不会返回的刷新令牌/Token终结点
                RefreshTokenProvider = new AuthenticationTokenProvider
                {
                    OnCreate = CreateRefreshToken,
                    OnReceive = ReceiveRefreshToken
                },

            });
        }


        private void ReceiveAccessToken(AuthenticationTokenReceiveContext context)
        {
            context.DeserializeTicket(context.Token);
        }

        private void CreateAccessToken(AuthenticationTokenCreateContext context)
        {
            context.SetToken(context.SerializeTicket());
        }


        private void ReceiveRefreshToken(AuthenticationTokenReceiveContext context)
        {
            context.DeserializeTicket(context.Token);
            if (context.Ticket==null)
            {
                context.Response.StatusCode = (int) HttpStatusCode.Unauthorized;
            }
        }

        private void CreateRefreshToken(AuthenticationTokenCreateContext context)
        {
            context.Ticket.Properties.ExpiresUtc = new DateTimeOffset(DateTime.Now.Add(TimeSpan.FromDays(1)));
            context.SetToken(context.SerializeTicket());
        }

        #region 生成Code（拿code换Token的code）
        private void ReceiveAuthenticationCode(AuthenticationTokenReceiveContext context)
        {
            string value;
            if (_authenticationCodes.TryRemove(context.Token, out value))
            {
                context.DeserializeTicket(value);
            }
        }

        private void CreateAuthenticationCode(AuthenticationTokenCreateContext context)
        {
            context.SetToken(Guid.NewGuid().ToString("n") + Guid.NewGuid().ToString("n"));
            _authenticationCodes[context.Token] = context.SerializeTicket();
        }
        #endregion

        private Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            string clientId;
            string clientSecret;
            if (context.TryGetBasicCredentials(out clientId, out clientSecret) ||
                context.TryGetFormCredentials(out clientId, out clientSecret))
            {
                if (clientId == "123456" && clientSecret == "123456")
                {
                    context.Validated();
                }
            }
            context.Validated();
            return Task.FromResult(0);
        }

        private Task ValidateClientRedirectUri(OAuthValidateClientRedirectUriContext context)
        {
            if (context.ClientId == "123456")
            {
                context.Validated();
            }
            return Task.FromResult(0);
        }

        private Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {
            var identity = new ClaimsIdentity(new GenericIdentity(context.ClientId, OAuthDefaults.AuthenticationType), context.Scope.Select(x => new Claim("urn:oauth:scope", x)));

            context.Validated(identity);

            return Task.FromResult(0);
        }

        private Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var identity = new ClaimsIdentity(new GenericIdentity("111", OAuthDefaults.AuthenticationType),
                new[]
                {
                    new Claim("CurrentUserId", "111"),
                    new Claim("CurrentUserName","222"),
                    new Claim("CurrentUserRoleId", 10086.ToString()),
                    new Claim("dbkey", "333")
                });


            context.Validated(identity);

            return Task.FromResult(0);
        }
    }
}
