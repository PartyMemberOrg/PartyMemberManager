using PartyMemberManager.Core.Enums;
using PartyMemberManager.Dal;
using PartyMemberManager.Dal.Entities;
using PartyMemberManager.Framework.Models.JsonModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PartyMemberManager.Filters
{
    public class AuthorizeFilter : Attribute, IAsyncAuthorizationFilter
    {
        protected readonly PMContext _context;
        public AuthorizeFilter(PMContext context)
        {
            _context = context;
        }
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            //1. 获取区域、控制器、Action的名称
            //必须在区域里的控制器上加个特性[Area("")]才能获取
            string areaName = null;
            if (context.ActionDescriptor.RouteValues.Keys.Contains("area"))
                areaName = context.ActionDescriptor.RouteValues["area"] == null ? "" : context.ActionDescriptor.RouteValues["area"].ToString();
            string controllerName = context.ActionDescriptor.RouteValues["controller"] == null ? "" : context.ActionDescriptor.RouteValues["controller"].ToString();
            string actionName = context.ActionDescriptor.RouteValues["action"] == null ? "" : context.ActionDescriptor.RouteValues["action"].ToString();

            //下面的方式也能获取控制器和action的名称
            //var controllerName = context.RouteData.Values["controller"].ToString();
            //var actionName = context.RouteData.Values["action"].ToString();
            var identity = context.HttpContext.User.Identity;
            var claims = context.HttpContext.User.Claims;
            Operator @opeartor = new Operator
            {
                Id = Guid.Parse(claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value),
                LoginName = claims.First(c => c.Type == ClaimTypes.Name).Value,
                Name = claims.First(c => c.Type == "FullName").Value,
                Roles = (Role)Enum.Parse(typeof(Role), claims.First(c => c.Type == ClaimTypes.Role).Value)
            };
            //分解用户的多个角色
            List<Role> userRoles = new List<Role>();
            foreach (Role role in Enum.GetValues(typeof(Role)))
            {
                if ((@opeartor.Roles & role) == role)
                    userRoles.Add(role);
            }
            //排列顺序先考虑area,再考虑有action的
            List<Module> modules = await _context.Modules.Where(m => m.Controller == controllerName && (m.Action == null || m.Action == "" || m.Action == actionName) && (m.Area == null || m.Area == "" || m.Area == areaName)).OrderByDescending(m => m.Area).OrderByDescending(m => m.Action).ToListAsync();
            //授权
            bool granted = false;
            //禁止权限
            bool denied = false;
            foreach (Module module in modules)
            {
                foreach (Role role in userRoles)
                    if ((module.Roles & role) == role)
                    {
                        granted = true;
                        continue;
                    }
            }
            //如果未授权则再检查权限和禁止权限（如果没有授权没有必要检查禁止权限，已无权执行了）
            if (!granted)
            {
                List<OperatorModule> operatorModules = await _context.OperatorModules.Where(om => modules.Select(m => m.Id).Contains(om.ModuleId) && om.UserId == @opeartor.Id).ToListAsync();
                if (operatorModules.Any(om => om.RightType == RightType.Grant))
                    granted = true;
                if (operatorModules.Any(om => om.RightType == RightType.Deny))
                    denied = true;
            }
            if (denied || !granted)
            {
                //2.判断是什么请求，进行响应的页面跳转
                if (IsAjaxRequest(context.HttpContext.Request))
                {
                    //2.1 是ajax请求
                    context.Result = new JsonResult(new
                    JsonResultNoData
                    {
                        Code = -1,
                        Message = "您没有权限"
                    });
                }
                else
                {
                    //2.2 不是ajax请求
                    var result = new ViewResult { ViewName = "~/Views/Shared/NoRightError.cshtml" };
                    context.Result = result;
                }
            }
        }

        /// <summary>
        /// 判断该请求是否是ajax请求
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private bool IsAjaxRequest(HttpRequest request)
        {
            string header = request.Headers["X-Requested-With"];
            return "XMLHttpRequest".Equals(header);
        }
    }
}
