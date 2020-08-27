using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PartyMemberManager.Framework.Controllers;
using EntityExtension;
using PartyMemberManager.Core.Helpers;
using PartyMemberManager.Core.Exceptions;
using Microsoft.Extensions.Logging;
using PartyMemberManager.Framework.Models.JsonModels;
using Microsoft.AspNetCore.Http;
using PartyMemberManager.Dal;
using PartyMemberManager.Dal.Entities;
using PartyMemberManager.Core.Enums;

namespace PartyMemberManager.Controllers
{
    public class ActiveApplicationSurveysController : PartyMemberDataControllerBase<ActiveApplicationSurvey>
    {

        public ActiveApplicationSurveysController(ILogger<ActiveApplicationSurveysController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: ActiveApplicationSurveys
        public async Task<IActionResult> Index(int page = 1)
        {
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderByDescending(d => d.StartYear).ThenByDescending(d => d.Term).Where(d => d.Enabled == true), "Id", "Name");
            return View(await _context.ActiveApplicationSurveies.Include(d => d.Department).OrderBy(a => a.Ordinal).GetPagedDataAsync(page));
        }

        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(Guid? departmentId,Guid? yearTermId, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<ActiveApplicationSurvey> jsonResult = new JsonResultDatasModel<ActiveApplicationSurvey>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<ActiveApplicationSurvey>();
                if (departmentId != null)
                {
                    filter = filter.And(d => d.DepartmentId == departmentId);
                }
                if (yearTermId != null)
                {
                    filter = filter.And(d => d.YearTermId == yearTermId);
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                    var data = await _context.Set<ActiveApplicationSurvey>().Include(d => d.Department).Include(d=>d.YearTerm).Where(filter).OrderByDescending(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<ActiveApplicationSurvey>().Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<ActiveApplicationSurvey>().Where(filter).Include(d => d.Department).Include(d=>d.YearTerm)
                        .Where(d=>d.YearTerm.Enabled==true)
                        .Where(d => d.DepartmentId == CurrentUser.DepartmentId).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<ActiveApplicationSurvey>().Count();
                    jsonResult.Data = data.Data;
                }
            }

            catch (PartyMemberException ex)
            {
                jsonResult.Code = -1;
                jsonResult.Msg = ex.Message;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                jsonResult.Code = -1;
                jsonResult.Msg = "发生系统错误";
            }
            return Json(jsonResult);
        }
        // GET: ActiveApplicationSurveys/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var activeApplicationSurvey = await _context.ActiveApplicationSurveies
            .SingleOrDefaultAsync(m => m.Id == id);
            if (activeApplicationSurvey == null)
            {
                return NotFoundData();
            }

            return View(activeApplicationSurvey);
        }

        // GET: ActiveApplicationSurveys/Create
        public IActionResult Create()
        {
            ActiveApplicationSurvey activeApplicationSurvey = new ActiveApplicationSurvey();
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderByDescending(d => d.StartYear).ThenByDescending(d => d.Term).Where(d => d.Enabled == true), "Id", "Name");
            return View(activeApplicationSurvey);
        }


        // GET: ActiveApplicationSurveys/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var activeApplicationSurvey = await _context.ActiveApplicationSurveies.SingleOrDefaultAsync(m => m.Id == id);
            if (activeApplicationSurvey == null)
            {
                return NotFoundData();
            }
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderByDescending(d => d.StartYear).ThenByDescending(d => d.Term).Where(d => d.Enabled == true), "Id", "Name");
            return View(activeApplicationSurvey);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("SchoolArea,YearTermId,Total,TrainTotal,DepartmentId,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] ActiveApplicationSurvey activeApplicationSurvey)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };
            try
            {
                if (ModelState.IsValid)
                {
                    ActiveApplicationSurvey activeApplicationSurveyInDb = await _context.ActiveApplicationSurveies.FindAsync(activeApplicationSurvey.Id);
                    if (activeApplicationSurveyInDb != null)
                    {
                        activeApplicationSurveyInDb.YearTermId = activeApplicationSurvey.YearTermId;
                        activeApplicationSurveyInDb.Total = activeApplicationSurvey.Total;
                        activeApplicationSurveyInDb.TrainTotal = activeApplicationSurvey.TrainTotal;
                        activeApplicationSurveyInDb.Proportion = Math.Round((double)(activeApplicationSurvey.TrainTotal) / (double)activeApplicationSurvey.Total,2);
                        activeApplicationSurveyInDb.Id = activeApplicationSurvey.Id;
                        activeApplicationSurveyInDb.CreateTime = DateTime.Now;
                        activeApplicationSurveyInDb.OperatorId = CurrentUser.Id;
                        activeApplicationSurveyInDb.Ordinal = _context.ActiveApplicationSurveies.Count() + 1;
                        activeApplicationSurveyInDb.IsDeleted = activeApplicationSurvey.IsDeleted;
                        if (CurrentUser.Roles == Role.学院党委)
                        {
                            activeApplicationSurveyInDb.DepartmentId = CurrentUser.DepartmentId.Value;
                        }
                        else
                        {
                            if (activeApplicationSurvey.DepartmentId == null)
                                throw new PartyMemberException("请选择部门");
                            activeApplicationSurveyInDb.DepartmentId = activeApplicationSurvey.DepartmentId;
                        }
                        _context.Update(activeApplicationSurveyInDb);
                    }
                    else
                    {
                        activeApplicationSurvey.Proportion = (double)(activeApplicationSurvey.TrainTotal) / (double)activeApplicationSurvey.Total;
                        if (CurrentUser.Roles == Role.学院党委)
                        {
                            activeApplicationSurvey.DepartmentId = CurrentUser.DepartmentId.Value;
                        }
                        else
                        {
                            if (activeApplicationSurvey.DepartmentId == null)
                                throw new PartyMemberException("请选择部门");
                            activeApplicationSurvey.DepartmentId = activeApplicationSurvey.DepartmentId;
                        }
                        activeApplicationSurvey.CreateTime = DateTime.Now;
                        activeApplicationSurvey.OperatorId = CurrentUser.Id;
                        activeApplicationSurvey.Ordinal = _context.ActiveApplicationSurveies.Count() + 1;
                        activeApplicationSurvey.IsDeleted = activeApplicationSurvey.IsDeleted;
                        _context.Add(activeApplicationSurvey);
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
                    foreach (string key in ModelState.Keys)
                    {
                        if (ModelState[key].Errors.Count > 0)
                            jsonResult.Errors.Add(new ModelError
                            {
                                Key = key,
                                Message = ModelState[key].Errors[0].ErrorMessage
                            });
                    }
                    jsonResult.Code = -1;
                    jsonResult.Message = "数据错误";
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, ex.Message);
                jsonResult.Code = -1;
                jsonResult.Message = "更新数据库错误";
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


        private bool ActiveApplicationSurveyExists(Guid id)
        {
            return _context.ActiveApplicationSurveies.Any(e => e.Id == id);
        }
    }
}
