﻿using ETexsys.Common.Log;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Security;

namespace ETexsys.Cloud.API.Common
{
    public class ApiTimingActionFilter : ActionFilterAttribute
    {
        private const string Key = "__action_duration__";
        private Guid guid;
        public override Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var actionName = actionContext.ActionDescriptor.ActionName;
            var controllerName = actionContext.ActionDescriptor.ControllerDescriptor.ControllerName;

            if (actionName.Contains("Upload"))
            {
                return base.OnActionExecutingAsync(actionContext, cancellationToken);
            }

            var modelState = actionContext.ModelState;
            if (!modelState.IsValid)
            {
                List<string> error = new List<string>();
                foreach (var key in modelState.Keys)
                {
                    var state = modelState[key];
                    if (state.Errors.Any())
                    {
                        error.Add(state.Errors.First().ErrorMessage);
                        break;
                    }
                }
                if (error.Count > 0)
                {
                    var response = new HttpResponseMessage();
                    response.Content = new StringContent(string.Join("\n", error.ToArray()));
                    response.StatusCode = HttpStatusCode.BadRequest;
                    throw new System.Web.Http.HttpResponseException(response);
                }
            }

            if (SkipLogging(actionContext))
            {
                return base.OnActionExecutingAsync(actionContext, cancellationToken);
            }
            var stopWatch = new Stopwatch();
            actionContext.Request.Properties[Key] = stopWatch;
            stopWatch.Start();

            IEnumerable<string> requestToken = new List<string>();
            actionContext.Request.Headers.TryGetValues("access_token", out requestToken);

            string token = string.Empty;
            if (requestToken != null)
            {
                token = requestToken.FirstOrDefault();
            }

            FormsAuthenticationTicket authTicket = null;
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    authTicket = FormsAuthentication.Decrypt(token);
                }
                catch { }
            }

            guid = Guid.NewGuid();

            string requestDataStr = "";
            if (!actionContext.Request.Content.IsMimeMultipartContent())
            {
                //获取请求数据  
                Stream stream = actionContext.Request.Content.ReadAsStreamAsync().Result;

                if (stream != null && stream.Length > 0)
                {
                    stream.Position = 0; //当你读取完之后必须把stream的读取位置设为开始
                    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                    {
                        requestDataStr = reader.ReadToEnd().ToString();
                    }
                }
            }

            new Log4NetFile().Log(string.Format("[ID:{0} ControllerName:{1} ActionName:{2} Parameter:{3} USERID:{4}]", guid.ToString(), controllerName, actionName, requestDataStr, authTicket == null ? "" : authTicket.Name));

            return base.OnActionExecutingAsync(actionContext, cancellationToken);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            if (!actionExecutedContext.Request.Properties.ContainsKey(Key))
            {
                return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
            }

            var actionName = actionExecutedContext.ActionContext.ActionDescriptor.ActionName;
            var controllerName = actionExecutedContext.ActionContext.ActionDescriptor.ControllerDescriptor.ControllerName;

            if (actionExecutedContext.Exception != null)
            {
                new Log4NetFile().Log(string.Format("[ID:{0} ControllerName:{1} ActionName:{2} Error:{3} ; Source:{4};StackTrace:{5}]", guid.ToString(), controllerName, actionName,
                    actionExecutedContext.Exception.Message, actionExecutedContext.Exception.Source, actionExecutedContext.Exception.StackTrace));
                actionExecutedContext.Exception = null;
            }

            var stopWatch = actionExecutedContext.Request.Properties[Key] as Stopwatch;
            if (stopWatch != null)
            {
                stopWatch.Stop();
                new Log4NetFile().Log(string.Format("[ID:{0} ControllerName:{1} ActionName:{2} 耗时 {3}.]", guid.ToString(), controllerName, actionName, stopWatch.ElapsedMilliseconds));
            }

            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }

        private static bool SkipLogging(HttpActionContext actionContext)
        {
            return actionContext.ActionDescriptor.GetCustomAttributes<NoLogAttribute>().Any() ||
                    actionContext.ControllerContext.ControllerDescriptor.GetCustomAttributes<NoLogAttribute>().Any();
        }
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true)]
    public class NoLogAttribute : Attribute
    {

    }
}