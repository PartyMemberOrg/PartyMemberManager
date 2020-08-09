using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq.Expressions;
using System.Text;
using System.Text.Encodings.Web;
using EntityExtension;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace HtmlHelperExtension
{
    /// <summary>
    /// 分页
    /// </summary>
    public static class PagerHelper
    {
        /// <summary>
        /// 无记录时分页显示方式
        /// </summary>
        /// <returns></returns>
        public static IHtmlContent EmptyPager()
        {
            return new HtmlString("");
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="currentPageIndex">当前页</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="recordCount">记录总数</param>
        /// <param name="htmlAttributes">分页头标签属性</param>
        /// <param name="className">分页样式</param>
        /// <param name="mode">分页模式</param>
        /// <returns></returns>
        public static IHtmlContent Pager(this IHtmlHelper helper, int currentPageIndex, int pageSize, int recordCount, object htmlAttributes, string className, PageMode mode)
        {
            if (recordCount == 0) return EmptyPager();
            TagBuilder tbDiv = new TagBuilder("div");
            tbDiv.AddCssClass("pagination pagination-centered");
            tbDiv.AddCssClass(className);
            TagBuilder tbUl = new TagBuilder("ul");
            tbUl.AddCssClass("pagination");
            tbUl.InnerHtml.SetHtmlContent(GetNormalPage(helper, currentPageIndex, pageSize, recordCount, mode).RenderHtmlContent());
            tbDiv.InnerHtml.SetHtmlContent(tbUl.RenderHtmlContent());
            TagBuilder tabDivPgerContainer = new TagBuilder("div");
            tabDivPgerContainer.AddCssClass("col-sm-12 col-md-7");
            tabDivPgerContainer.InnerHtml.SetHtmlContent(tbDiv.RenderHtmlContent());


            int start = (currentPageIndex - 1) * pageSize + 1;
            int end = currentPageIndex * pageSize;
            if (end > recordCount) end = recordCount;

            TagBuilder tagPageTotal = new TagBuilder("div");
            tagPageTotal.AddCssClass("");
            tagPageTotal.InnerHtml.AppendFormat("第 {0} 至 {1} 条，共 {2} 记录", start, end, recordCount);
            TagBuilder tagPageTotalContainer = new TagBuilder("div");
            tagPageTotalContainer.AddCssClass("col-sm-12 col-md-5");
            tagPageTotalContainer.InnerHtml.SetHtmlContent(tagPageTotal.RenderHtmlContent());

            TagBuilder tagPager = new TagBuilder("div");
            tagPager.AddCssClass("row");
            tagPager.InnerHtml.AppendHtml(tagPageTotalContainer.RenderHtmlContent());
            tagPager.InnerHtml.AppendHtml(tabDivPgerContainer.RenderHtmlContent());
            return new HtmlString(tagPager.ToHtmlString());
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="currentPageIndex">当前页</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="recordCount">记录总数</param>
        /// <param name="className">分页样式</param>
        /// <returns></returns>
        public static IHtmlContent Pager(this IHtmlHelper helper, int currentPageIndex, int pageSize, int recordCount, string className)
        {
            return Pager(helper, currentPageIndex, pageSize, recordCount, null, className, PageMode.Numeric);
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="currentPageIndex">当前页</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="recordCount">记录总数</param>
        /// <returns></returns>
        public static IHtmlContent Pager(this IHtmlHelper helper, int currentPageIndex, int pageSize, int recordCount)
        {
            return Pager(helper, currentPageIndex, pageSize, recordCount, null);
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="pagedDataViewModel">分页模型</param>
        /// <returns></returns>
        public static IHtmlContent Pager(this IHtmlHelper helper, PagedDataViewModel pagedDataViewModel)
        {
            return Pager(helper, pagedDataViewModel.CurrentPage, pagedDataViewModel.PageSize, pagedDataViewModel.TotalCount, null);
        }
        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="currentPageIndex">当前页</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="recordCount">记录总数</param>
        /// <param name="mode">分页模式</param>
        /// <returns></returns>
        public static IHtmlContent Pager(this IHtmlHelper helper, int currentPageIndex, int pageSize, int recordCount, PageMode mode)
        {
            return Pager(helper, currentPageIndex, pageSize, recordCount, null, mode);
        }

        /// <summary>
        /// 分页
        /// </summary>
        /// <param name="helper"></param>
        /// <param name="currentPageIndex">当前页</param>
        /// <param name="pageSize">分页尺寸</param>
        /// <param name="recordCount">记录总数</param>
        /// <param name="className">分页样式</param>
        /// <param name="mode">分页模式</param>
        /// <returns></returns>
        public static IHtmlContent Pager(this IHtmlHelper helper, int currentPageIndex, int pageSize, int recordCount, string className, PageMode mode)
        {
            return Pager(helper, currentPageIndex, pageSize, recordCount, null, className, mode);
        }

        /// <summary>
        /// 获取普通分页
        /// </summary>
        /// <param name="currentPageIndex">当前页</param>
        /// <param name="pageSize">每页显示的记录个数</param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        private static IHtmlContent GetNormalPage(IHtmlHelper helper, int currentPageIndex, int pageSize, int recordCount, PageMode mode)
        {
            if (recordCount == 0) return EmptyPager();
            int pageCount = (recordCount % pageSize == 0 ? recordCount / pageSize : recordCount / pageSize + 1);
            StringBuilder url = new StringBuilder();
            HttpContext httpContext = helper.ViewContext.HttpContext;
            //取当前请求地址，并在后面加上? page = 页号
            url.Append(httpContext.Request.Path + "?page={0}");
            var request = httpContext.Request;
            //取当前请求的查询参数
            IQueryCollection collectionQueryString = request.Query;
            var keys = collectionQueryString.Keys;
            //除了page,controller和action之外的参数拼接到后面
            string[] excludeKeys = { "page", "controller", "action" };
            foreach (var key in keys)
            {
                if (Array.IndexOf(excludeKeys, key) < 0)
                    url.AppendFormat("&{0}={1}", key, request.Query[key]);
            }
            //将通过Form提交过来的数据也拼接到后面，主要考虑用户数据查询条件后防止在分页过程中数据丢失，这里有一个安全问题就是将post的数据显示在链接地中
            if (request.Method == "POST")
                foreach (var formKey in request.Form.Keys)
                {
                    string key = formKey.ToString();
                    //如果查询字符串中已经包含，则排除
                    if (keys.Contains(key)) continue;
                    //排除特定的关键字
                    if (Array.IndexOf(excludeKeys, key) < 0)
                        url.AppendFormat("&{0}={1}", key, (string)(httpContext.Request.Form[key]));
                }


            StringBuilder sb = new StringBuilder();
            if (currentPageIndex == 1)
            {
                sb.Append("<li class='paginate_button page-item first disabled' id='datatables_first'>");
                sb.Append("<a class='page-link' aria-controls='mytable' data-dt-idx='0' tabindex='0' id='mytable_first'>首页</a>");
                sb.Append("</li>");
            }
            else
            {
                sb.Append("<li class='paginate_button page-item first' id='datatables_first'>");
                string url1 = string.Format(url.ToString(), 1);
                sb.AppendFormat("<a class='page-link' aria-controls='mytable' data-dt-idx='0' tabindex='0' id='mytable_first' href='{0}'>首页</a>", url1);
                sb.Append("</li>");
            }
            if (currentPageIndex > 1)
            {
                sb.Append("<li class='paginate_button page-item previous' id='datatables_previous'>");
                string url1 = string.Format(url.ToString(), currentPageIndex - 1);
                sb.AppendFormat("<a class='page-link' aria-controls='mytable' data-dt-idx='1' tabindex='0' id='mytable_previous' href='{0}'>上页</a>", url1);
                sb.Append("</li>");
            }
            else
            {
                sb.Append("<li class='paginate_button page-item previous disabled' id='datatables_previous'>");
                sb.Append("<a class='page-link' aria-controls='mytable' data-dt-idx='1' tabindex='0' id='mytable_previous'>上页</a>");
                sb.Append("</li>");
            }
            if (mode == PageMode.Numeric)
            {
                sb.Append(GetNumericPage(currentPageIndex, pageSize, recordCount, pageCount, url.ToString()));
            }
            if (currentPageIndex < pageCount)
            {
                sb.Append("<li class='paginate_button page-item next' id='datatables_next'>");
                string url1 = string.Format(url.ToString(), currentPageIndex + 1);
                sb.AppendFormat("<a class='page-link' aria-controls='mytable' data-dt-idx='11' tabindex='0' id='mytable_previous' href='{0}'>下页</a>", url1);
                sb.Append("</li>");
            }
            else
            {
                sb.Append("<li class='paginate_button page-item next disabled' id='datatables_next'>");
                sb.Append("<a class='page-link' aria-controls='mytable' data-dt-idx='11' tabindex='0' id='mytable_previous'>下页</a>");
                sb.Append("</li>");
            }

            if (currentPageIndex == pageCount || pageCount == 0)
            {
                sb.Append("<li class='paginate_button page-item last disabled' id='datatables_last'>");
                sb.Append("<a class='page-link' aria-controls='mytable' data-dt-idx='12' tabindex='0' id='mytable_previous'>末页</a>");
                sb.Append("</li>");
            }
            else
            {
                sb.Append("<li class='paginate_button page-item last' id='datatables_last'>");
                string url1 = string.Format(url.ToString(), pageCount);
                sb.AppendFormat("<a class='page-link' aria-controls='mytable' data-dt-idx='12' tabindex='0' id='mytable_previous' href='{0}'>末页</a>", url1);
                sb.Append("</li>");
            }
            //sb.AppendFormat("<span class='text-right' style='display: block; float: left;height: 30px;line-height: 30px;padding-left: 20px;'>第{2}页/共{1}页, 总共{0}条记录</span>", recordCount, pageCount, Math.Min(currentPageIndex, pageCount));

            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// 获取数字分页
        /// </summary>
        /// <param name="currentPageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <param name="pageCount"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private static IHtmlContent GetNumericPage(int currentPageIndex, int pageSize, int recordCount, int pageCount, string url)
        {
            var start = Math.Max(1, currentPageIndex - 2 - Math.Max(0, currentPageIndex + 2 - pageCount));
            var end = Math.Min(pageCount, currentPageIndex + 2 - Math.Min(0, currentPageIndex - 2 - 1));
            StringBuilder sb = new StringBuilder();
            for (int i = start; i <= end; i++)
            {
                if (i == currentPageIndex)
                {
                    sb.Append("<li class='paginate_button page-item active'>");
                    sb.AppendFormat("<a class='page-link' aria-controls='mytable' data-dt-idx='{2}' tabindex='0'>{1}</a>", url, i, i + 2);
                    sb.Append("</li>");
                }
                else
                {
                    sb.Append("<li class='paginate_button page-item'>");
                    string url1 = string.Format(url, i);
                    sb.AppendFormat("<a class='page-link' aria-controls='mytable' data-dt-idx='{2}' tabindex='0' href='{0}'>{1}</a>", url1, i, i + 2);
                    sb.Append("</li>");
                }
            }
            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// 获取普通分页
        /// </summary>
        /// <param name="currentPageIndex">当前页</param>
        /// <param name="pageSize">每页显示的记录个数</param>
        /// <param name="recordCount"></param>
        /// <returns></returns>
        private static IHtmlContent AjaxGetNormalPage(int currentPageIndex, int pageSize, int recordCount, PageMode mode)
        {
            if (recordCount == 0) return EmptyPager();
            int pageCount = (recordCount % pageSize == 0 ? recordCount / pageSize : recordCount / pageSize + 1);
            StringBuilder url = new StringBuilder();
            //url.Append(HttpContext.Current.Request.Url.AbsolutePath + "?page={0}");
            //var request = HttpContext.Current.Request;
            //NameValueCollection collectionQueryString = request.QueryString;
            //var keys = collectionQueryString.AllKeys;
            //foreach (var key in keys)
            //{
            //    if (key.ToLower() != "page")
            //        url.AppendFormat("&{0}={1}", key, request[key]);
            //}
            string stringUrl = AjaxPager(url.ToString(), "");
            StringBuilder sb = new StringBuilder();
            if (currentPageIndex == 1)
                sb.Append("<li class='disabled'><a>首页</a></li>");
            else
            {
                string url1 = string.Format(stringUrl.ToString(), 1);
                sb.AppendFormat("<li><a href='{0}'>首页</a></li>", url1);
            }
            if (currentPageIndex > 1)
            {
                string url1 = string.Format(stringUrl.ToString(), currentPageIndex - 1);
                sb.AppendFormat("<li><a href='{0}'>上一页</a></li>", url1);
            }
            else
                sb.Append("<li class='disabled'><a>上一页</a></li>");
            if (mode == PageMode.Numeric)
                sb.Append(AjaxGetNumericPage(currentPageIndex, pageSize, recordCount, pageCount, stringUrl.ToString()));
            if (currentPageIndex < pageCount)
            {
                string url1 = string.Format(stringUrl.ToString(), currentPageIndex + 1);
                sb.AppendFormat("<li><a href='{0}'>下一页</a></li>", url1);
            }
            else
                sb.Append("<li class='disabled'><a>下一页</a></li>");

            if (currentPageIndex == pageCount || pageCount == 0)
                sb.Append("<li class='disabled'><a>末页</a></li>");
            else
            {
                string url1 = string.Format(stringUrl.ToString(), pageCount);
                sb.AppendFormat("<li><a href='{0}'>末页</a></li>", url1);
            }
            sb.AppendFormat("<p class='text-right'style='display: block; float: left;height: 30px;line-height: 30px;padding-left: 20px;'>第{2}页/共{1}页, 总共{0}条记录</p>", recordCount, pageCount, Math.Min(currentPageIndex, pageCount));
            sb.Append(AjaxPagerFunction());
            return new HtmlString(sb.ToString());
        }

        /// <summary>
        /// 获取数字分页
        /// </summary>
        /// <param name="currentPageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="recordCount"></param>
        /// <param name="pageCount"></param>
        /// <param name="url"></param>
        /// <returns></returns>
        private static IHtmlContent AjaxGetNumericPage(int currentPageIndex, int pageSize, int recordCount, int pageCount, string url)
        {
            var start = Math.Max(1, currentPageIndex - 2 - Math.Max(0, currentPageIndex + 2 - pageCount));
            var end = Math.Min(pageCount, currentPageIndex + 2 - Math.Min(0, currentPageIndex - 2 - 1));
            StringBuilder sb = new StringBuilder();
            for (int i = start; i <= end; i++)
            {
                if (i == currentPageIndex)
                    sb.AppendFormat("<li class=\"active\"><a>{0}</a></li>", i);
                else
                {
                    string url1 = string.Format(url, i);
                    sb.AppendFormat("<li><a href='{0}'>{1}</a></li>", url1, i);
                }
            }
            return new HtmlString(sb.ToString());
        }

        private static IHtmlContent AjaxPagerFunction()
        {
            string javaScript = @"
<script type='text / javascript'>
    function loadPage(url, tag)
    {
        $.get(url, { }, function(data) {
            $('#' + tag.html(data));
            })
    }
</script>";
            return new HtmlString(javaScript);
        }
        private static string AjaxPager(string url, string parentContainer)
        {
            return string.Format("javascript:loadPage(\"{0}\",\"{1}\")");
        }
        /// <summary>
        /// 创建提交文件的Form
        /// </summary>
        /// <param name="htmlHelper"></param>
        /// <returns></returns>
        public static MvcForm BeginFilePostForm(this IHtmlHelper htmlHelper)
        {
            string actionName = htmlHelper.ViewContext.RouteData.Values["action"].ToString();
            string controllerName = htmlHelper.ViewContext.RouteData.Values["controller"].ToString();
            MvcForm mvcForm = htmlHelper.BeginForm(actionName, controllerName, FormMethod.Post, new { enctype = "multipart/form-data" });
            return mvcForm;
        }


        #region Common extensions

        /// <summary>
        /// Convert IHtmlContent to string
        /// </summary>
        /// <param name="htmlContent">HTML content</param>
        /// <returns>Result</returns>
        public static string RenderHtmlContent(this IHtmlContent htmlContent)
        {
            using (var writer = new StringWriter())
            {
                htmlContent.WriteTo(writer, HtmlEncoder.Default);
                var htmlOutput = writer.ToString();
                return htmlOutput;
            }
        }

        /// <summary>
        /// Convert IHtmlContent to string
        /// </summary>
        /// <param name="tag">Tag</param>
        /// <returns>String</returns>
        public static string ToHtmlString(this IHtmlContent tag)
        {
            using (var writer = new StringWriter())
            {
                tag.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        #endregion
    }
    /// <summary>
    /// 分页模式
    /// </summary>
    public enum PageMode
    {
        /// <summary>
        /// 普通分页模式
        /// </summary>
        Normal,
        /// <summary>
        /// 普通分页加数字分页
        /// </summary>
        Numeric
    }
}
