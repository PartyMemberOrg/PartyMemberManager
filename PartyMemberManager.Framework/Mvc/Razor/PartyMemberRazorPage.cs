using PartyMemberManager.Core.Enums;
using PartyMemberManager.Dal.Entities;
using PartyMemberManager.Framework.Controllers;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PartyMemberManager.Framework.Mvc.Razor
{
    /// <summary>
    /// Web view page
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    public abstract class PartyMemberRazorPage<TModel> : Microsoft.AspNetCore.Mvc.Razor.RazorPage<TModel>
    {
        private Operator currentUser = null;
        public Operator CurrentUser
        {
            get
            {
                if (currentUser != null) return currentUser;
                if (User == null)
                    return null;
                var identity = User.Identity;
                var claims = User.Claims;
                Operator @operator = new Operator
                {
                    Id = Guid.Parse(claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value),
                    LoginName = claims.First(c => c.Type == ClaimTypes.Name).Value,                  
                    Roles = (Role)Enum.Parse(typeof(Role), claims.First(c => c.Type == ClaimTypes.Role).Value)
                };
                currentUser = @operator;
                return @operator;
            }
        }
        /// <summary>
        /// form元素的默认样式
        /// </summary>
        public string FormDefaultClass { get; set; } = "";// "layui-form";
        public string FormGroupDefaultClass { get; set; } = "";//"layui-form-item";
        /// <summary>
        /// 标签默认样式
        /// </summary>
        public string LabelDivDefaultClass { get; set; } = "left-label-div"; /*"col-md-4 col-xd-6 col-lg-4 text-right";*/
        /// <summary>
        /// 输入控件默认样式
        /// </summary>
        public string InputDivDefaultClass { get; set; } = "right-label-div";/*"col-md-8 col-xd-6 col-lg-8 text-left";*/
        public string LabelDefaultClass { get; set; } = "control-label ";
        public string InputDefaultClass { get; set; } = "layui-input";
        /// <summary>
        /// 验证默认样式
        /// </summary>
        public string ValidateSpanDefaultClass { get; set; } = "";//"offset-md-4 offset-xd-6 offset-lg-4 text-left";
        /// <summary>
        /// 编辑按钮文字或图标
        /// </summary>
        public string EditButtonTextOrIcon { get; set; } = "<i class=\"layui-icon layui-icon-edit\" style=\"font-size: 20px; color: #808080;\"></i>";
        /// <summary>
        /// 删除按钮文字或图标
        /// </summary>
        public string DeleteButtonTextOrIcon { get; set; } = "<i class=\"layui-icon layui-icon-delete\" style=\"font-size: 20px; color: #ff0000;\"></i>";
        /// <summary>
        /// 上移按钮文字或图比奥
        /// </summary>
        public string UpButtonTextOrIcon { get; set; } = "<i class=\"layui-icon layui-icon-up\" style=\"font-size: 20px; color: #808080;\"></i>";
        /// <summary>
        /// 下移按钮文字或图比奥
        /// </summary>
        public string DownButtonTextOrIcon { get; set; } = "<i class=\"layui-icon layui-icon-down\" style=\"font-size: 20px; color: #808080;\"></i>";
        /// <summary>
        /// 事项分组编辑按钮文字或图标
        /// </summary>
        public string ItemGroupEditButtonTextOrIcon { get; set; } = "编辑事项分组";//"<i class=\"layui-icon layui-icon-edit\" style=\"font-size: 12px; color: #808080;\"></i>";
        /// <summary>
        /// 事项分组删除按钮文字或图标
        /// </summary>
        public string ItemGroupDeleteButtonTextOrIcon { get; set; } = "<i class=\"layui-icon layui-icon-delete\" style=\"font-size: 12px; color: #ff0000;\"></i>";

        /// <summary>
        /// 返回当前页面的URL，其中&符号用$$代替
        /// </summary>
        public string CurrentUrl
        {
            get
            {
                Microsoft.AspNetCore.Http.HttpRequest request = Context.Request;
                string url = new System.Text.StringBuilder()
                .Append(request.Scheme)
                .Append("://")
                .Append(request.Host)
                .Append(request.PathBase)
                .Append(request.Path)
                .Append(request.QueryString)
                .ToString();
                return url;
            }
        }

        //public IEnumerable<SelectListItem> PatientStausSelectList
        //{
        //    get
        //    {
        //        List<SelectListItem> items = new List<SelectListItem>();
        //        items.Add(new SelectListItem {
        //            Text= PatientStatus.入院.ToString(),
        //            Value=((int)PatientStatus.入院).ToString()
        //        });
        //        items.Add(new SelectListItem
        //        {
        //            Text = PatientStatus.营养支持.ToString(),
        //            Value = ((int)PatientStatus.营养支持).ToString()
        //        });
        //        items.Add(new SelectListItem
        //        {
        //            Text = PatientStatus.术前.ToString(),
        //            Value = ((int)PatientStatus.术前).ToString()
        //        });
        //        items.Add(new SelectListItem
        //        {
        //            Text = PatientStatus.术中.ToString(),
        //            Value = ((int)PatientStatus.术中).ToString()
        //        });
        //        items.Add(new SelectListItem
        //        {
        //            Text = PatientStatus.术后.ToString(),
        //            Value = ((int)PatientStatus.术后).ToString()
        //        });
        //        items.Add(new SelectListItem
        //        {
        //            Text = PatientStatus.出院.ToString(),
        //            Value = ((int)PatientStatus.出院).ToString()
        //        });
        //        return items;
        //    }
        //}
        /// <summary>
        /// 过滤标点符号
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string ReplacePunctuation(string text)
        {
            if (!string.IsNullOrEmpty(text))
                return Regex.Replace(text, "[\\[\\]\\^\\/-_*×――(^)$%~!@#$…&%￥—+=<>《》!！??？:：•`·、。，；,.;\"‘’“”-]", "");
            else
                return text;
        }
    }
    /// <summary>
    /// Web view page
    /// </summary>
    public abstract class ErasRazorPage : PartyMemberRazorPage<dynamic>
    {
    }
}
