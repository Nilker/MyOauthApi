using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

/*----------------------------------------------------------------
* 项目名称 ：MyOauthApi.Filter
* 项目描述 ：
* 类 名 称 ：WebApiExceptionFilterAttribute
* 类 描 述 ： 
* 作    者 ：lihongli
* 创建时间 ：2019/11/19 16:19:54
* 版 本 号 ：v1.0.0.0
----------------------------------------------------------------*/
namespace MyOauthApi.Filter
{
    /// <summary>
    /// 全局异常过滤器
    /// </summary>
    public class WebApiExceptionFilterAttribute : ExceptionFilterAttribute
    {
        //private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        //重写基类的异常处理方法
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            //1.异常日志记录（正式项目里面一般是用log4net记录异常日志）
            //Console.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "——" +
            //                  actionExecutedContext.Exception.GetType().ToString() + "：" + actionExecutedContext.Exception.Message + "——堆栈信息：" +
            //                  actionExecutedContext.Exception.StackTrace);
            //log.Error("全局异常", actionExecutedContext.Exception);

            //2.返回调用方具体的异常信息
            if (actionExecutedContext.Exception is NotImplementedException)
            {
                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.NotImplemented);
                //actionExecutedContext.Response.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new JsonFlag<object>(ResultCodeEnum.HTTP_ERROR_IMPLEMENTED, "未实现")));
            }
            else if (actionExecutedContext.Exception is TimeoutException)
            {
                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.RequestTimeout);
                //actionExecutedContext.Response.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new JsonFlag<object>(ResultCodeEnum.HTTP_ERROR_UNAUTHORIZED, "未授权")));
            }
            //.....这里可以根据项目需要返回到客户端特定的状态码。如果找不到相应的异常，统一返回服务端错误500
            else
            {
                actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                //actionExecutedContext.Response.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject(new JsonFlag<object>(ResultCodeEnum.HTTP_ERROR_SERVER_ERROR, "服务器异常错误")));
            }

            base.OnException(actionExecutedContext);
        }
    }
}