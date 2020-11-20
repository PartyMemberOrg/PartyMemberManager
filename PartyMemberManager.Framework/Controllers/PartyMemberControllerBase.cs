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
        protected IHttpContextAccessor _accessor;
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
        /// 根据校区查询部门
        /// </summary>
        /// <param name="schoolArea"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDepartmentDatas(string schoolArea)
        {
            JsonResultModel<Department> jsonResult = new JsonResultModel<Department>
            {
                Code = 0,
                Message = "",
                Datas = new List<Department>()
            };

            try
            {
                var filter = PredicateBuilder.True<Department>();
                if (schoolArea != "0")
                {
                    filter = filter.And(d => d.SchoolArea.ToString() == schoolArea);
                }
                var data = await _context.Set<Department>()
                    .Where(filter)
                    .OrderByDescending(d => d.Ordinal).ToListAsync();
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Datas = data;
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
        /// 根据培训班类型、年份和部门查询培训班
        /// </summary>
        /// <param name="trainClassTypeId"></param>
        /// <param name="year"></param>
        /// <param name="term"></param>
        /// <param name="departmentId"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetTrainClassDatas(Guid? trainClassTypeId, Guid? yearTermId, Guid? departmentId,BatchType batch)
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
                if ((int)batch >0)
                {
                    filter = filter.And(d => d.Batch == batch);
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                        var data = await _context.Set<TrainClass>()
                            .Where(filter).Include(d => d.YearTerm).Where(d => d.YearTerm.Enabled == true).Include(d=>d.Department)
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
                        .Where(filter).Include(d => d.YearTerm).Where(d => d.YearTerm.Enabled == true)
                        .Where(t => t.DepartmentId == departmentId)
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
        /// 根据年份返回培训班
        /// </summary>
        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetProvinceTrainClassDatas(string year)
        {
            JsonResultModel<ProvinceTrainClass> jsonResult = new JsonResultModel<ProvinceTrainClass>
            {
                Code = 0,
                Message = "",
                Datas = new List<ProvinceTrainClass>()
            };

            try
            {
                var filter = PredicateBuilder.True<ProvinceTrainClass>();
                if (year != null)
                {
                    filter = filter.And(d => d.Year == year);
                }
                var data = await _context.Set<ProvinceTrainClass>()
                    .Where(filter).Where(d => d.Enabled == true)
                    .OrderByDescending(d => d.Ordinal).ToListAsync();
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Datas = data;
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
            //TODO:考虑用正则表达式先判断，再转换
            if (!DateTime.TryParseExact(dateString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dateValue))
            {
                if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dateValue))
                {
                    if (!DateTime.TryParseExact(dateString, "yyyy-M-d", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dateValue))
                    {
                        if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd HH:mm", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dateValue))
                        {
                            if (!DateTime.TryParseExact(dateString, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dateValue))
                            {
                                if (!DateTime.TryParseExact(dateString, "yyyy-M-d H:m", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dateValue))
                                {
                                    if (!DateTime.TryParseExact(dateString, "yyyy-M-d H:m:s", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out dateValue))
                                    {
                                        return false;
                                    }
                                }
                            }
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
            //TODO:考虑用正则表达式先判断，再转换
            if (!DateTime.TryParseExact(yearMonthString, "yyyyMMdd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out yearMonthValue))
            {
                if (!DateTime.TryParseExact(yearMonthString, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out yearMonthValue))
                {
                    if (!DateTime.TryParseExact(yearMonthString, "yyyy-M-d", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out yearMonthValue))
                    {
                        if (!DateTime.TryParseExact(yearMonthString, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out yearMonthValue))
                        {
                            if (!DateTime.TryParseExact(yearMonthString, "yyyy-M-d H:m:s", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AdjustToUniversal, out yearMonthValue))
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// 打印日期
        /// </summary>
        /// <returns></returns>
        public virtual IActionResult PrintDate()
        {
            return View("PrintDate");
        }
    }
}
