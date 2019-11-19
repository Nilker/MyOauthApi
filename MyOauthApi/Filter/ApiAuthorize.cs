using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Http;
using System.Web.Http.Controllers;

/*----------------------------------------------------------------
* 项目名称 ：MyOauthApi.Filter
* 项目描述 ：
* 类 名 称 ：ApiAuthorize
* 类 描 述 ： 
* 作    者 ：lihongli
* 创建时间 ：2019/11/19 12:57:14
* 版 本 号 ：v1.0.0.0
----------------------------------------------------------------*/
namespace MyOauthApi.Filter
{
    /// <summary>
    /// 重写实现处理授权失败时返回json,避免跳转登录页
    /// </summary>
    public class ApiAuthorize : AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext filterContext)
        {
            base.HandleUnauthorizedRequest(filterContext);

            var response = filterContext.Response = filterContext.Response ?? new HttpResponseMessage();
            response.StatusCode = HttpStatusCode.Forbidden;
            var content = new 
            {
                success = false,
                errs = new[] { "服务端拒绝访问：你没有权限，或者掉线了" }
            };
            response.Content = new StringContent(Json.Encode(content), Encoding.UTF8, "application/json");
        }
    }
}