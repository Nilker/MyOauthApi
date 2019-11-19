using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;

/*----------------------------------------------------------------
* 项目名称 ：MyOauthApi.OAuth2
* 项目描述 ：
* 类 名 称 ：ApplicationOAuthProvider
* 类 描 述 ： 
* 作    者 ：lihongli
* 创建时间 ：2019/11/19 10:06:28
* 版 本 号 ：v1.0.0.0
----------------------------------------------------------------*/
namespace MyOauthApi.OAuth2
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        /// <summary>
        ///     验证客户[client_id与client_secret验证]
        /// </summary>
        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            //http://localhost:48339/token
            string client_id;
            string client_secret;
            context.TryGetFormCredentials(out client_id, out client_secret);
            if (client_id == "zara" && client_secret == "123456")
                context.Validated(client_id);
            else
                //context.Response.StatusCode = Convert.ToInt32(HttpStatusCode.OK);
                context.SetError("invalid_client", "client is not valid");
            return base.ValidateClientAuthentication(context);
        }

        /// <summary>
        ///     客户端授权[生成access token]
        /// </summary>
        public override Task GrantClientCredentials(OAuthGrantClientCredentialsContext context)
        {
            var oAuthIdentity = new ClaimsIdentity(context.Options.AuthenticationType);
            oAuthIdentity.AddClaim(new Claim(ClaimTypes.Name, "iphone"));
            var ticket = new AuthenticationTicket(oAuthIdentity, new AuthenticationProperties {AllowRefresh = true});
            context.Validated(ticket);
            return base.GrantClientCredentials(context);
        }

        /// <summary>
        ///     刷新Token[刷新refresh_token]
        /// </summary>
        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            //enforce client binding of refresh token
            if (context.Ticket == null || context.Ticket.Identity == null || !context.Ticket.Identity.IsAuthenticated)
                context.SetError("invalid_grant", "Refresh token is not valid");
            return base.GrantRefreshToken(context);
        }
    }
}