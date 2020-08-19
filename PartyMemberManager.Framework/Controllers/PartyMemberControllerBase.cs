using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EntityExtension;
using PartyMemberManager.Core.Enums;
using PartyMemberManager.Core.Exceptions;
using PartyMemberManager.Dal;
using PartyMemberManager.Dal.Entities;
using PartyMemberManager.Framework.Models.JsonModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace PartyMemberManager.Framework.Controllers
{
    [Authorize]
    public class PartyMemberControllerBase : Controller
    {
        private IHttpContextAccessor _accessor;
        private Operator currentUser = null;
        public Operator CurrentUser
        {

            get
            {
                HttpContext httpContext = HttpContext;
                if (httpContext == null)
                    httpContext = _accessor.HttpContext;
                if (currentUser != null) return currentUser;
                if (httpContext == null) return null;
                if (httpContext.User == null)
                    return null;
                var identity = httpContext.User.Identity;
                var claims = httpContext.User.Claims;
                Operator @operator = new Operator
                {
                    Id = Guid.Parse(claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value),
                    LoginName = claims.First(c => c.Type == ClaimTypes.Name).Value,
                    Name = claims.First(c => c.Type == "FullName").Value,
                    Roles = (Role)Enum.Parse(typeof(Role), claims.First(c => c.Type == ClaimTypes.Role).Value),

                };
                currentUser = @operator;
                @operator.DepartmentId = _context.Operators.Find(@operator.Id).DepartmentId;
                return @operator;
            }
        }
        protected readonly PMContext _context;
        protected readonly ILogger<PartyMemberControllerBase> _logger;
        public PartyMemberControllerBase(ILogger<PartyMemberControllerBase> logger, PMContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _logger = logger;
            _accessor = accessor;
        }

        protected void ShowAndLogSystemError(Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            _logger.LogError(ex, ex.Message);
        }

        /// <summary>
        /// 未找到数据
        /// </summary>
        /// <returns></returns>
        public virtual IActionResult NotFoundData()
        {
            return View("NotFoundData");
        }

        /// <summary>
        /// 根据培训班类型、年份和部门查询培训班
        /// </summary>
        /// <param name="trainClassTypeId"></param>
        /// <param name="year"></param>
        /// <param name="term"></param>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetTrainClassDatas(Guid? trainClassTypeId, Guid? yearTermId, Guid? departmentId)
        {
            JsonResultModel<TrainClass> jsonResult = new JsonResultModel<TrainClass>
            {
                Code = 0,
                Message = "",
                Datas = new List<TrainClass>()
            };

            try
            {
                var filter = PredicateBuilder.True<TrainClass>();
                if (trainClassTypeId != null)
                {
                    filter = filter.And(d => d.TrainClassTypeId == trainClassTypeId);
                }
                if (yearTermId != null)
                {
                    filter = filter.And(d => d.YearTermId == yearTermId);
                }
                if (departmentId != null)
                {
                    filter = filter.And(d => d.DepartmentId == departmentId);
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                    var data = await _context.Set<TrainClass>()
                        .Where(filter)
                        .OrderByDescending(d => d.Ordinal).ToListAsync();
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Datas = data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    departmentId = CurrentUser.DepartmentId.Value;
                    var data = await _context.Set<TrainClass>()
                        .Where(filter)
                        .Where(t=>t.DepartmentId==departmentId)
                        .OrderByDescending(d => d.Ordinal).ToListAsync();
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Datas = data;
                }
            }

            catch (PartyMemberException ex)
            {
                jsonResult.Code = -1;
                jsonResult.Message = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                jsonResult.Code = -1;
                jsonResult.Message = "发生系统错误";
            }
            return Json(jsonResult);
        }

        /// <summary>
        /// 转换日期
        /// </summary>
        /// <param name="dateString"></param>
        /// <param name="dateValue"></param>
        /// <returns></returns>
        public bool TryParseDate(string dateString, out DateTime dateValue)
        {
            if (!DateTime.TryParseExact(dateString, "yyyyMM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dateValue))
            {
                if (!DateTime.TryParseExact(dateString, "yyyy-MM", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dateValue))
                {
                    if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dateValue))
                    {
                        if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dateValue))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 转换年月
        /// </summary>
        /// <param name="yearMonthString"></param>
        /// <param name="yearMonthValue"></param>
        /// <returns></returns>
        public bool TryParseYearMonth(string yearMonthString, out DateTime yearMonthValue)
        {
            if (!DateTime.TryParseExact(yearMonthString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out yearMonthValue))
            {
                if (!DateTime.TryParseExact(yearMonthString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out yearMonthValue))
                {
                    if (!DateTime.TryParseExact(yearMonthString, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out yearMonthValue))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
