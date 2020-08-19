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
    public class ActivistTrainResultsController : PartyMemberDataControllerBase<ActivistTrainResult>
    {

        public ActivistTrainResultsController(ILogger<ActivistTrainResultsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: ActivistTrainResults
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.ActivistTrainResults.Include(a => a.PartyActivist).Include(d => d.PartyActivist.TrainClass);
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.OrderBy(d => d.Ordinal).Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.OrderBy(d => d.Ordinal), "Id", "Name");
            return View(await pMContext.ToListAsync());
        }

        public async Task<IActionResult> GetDatasWithFilter(Guid? departmentId, string isPass, string isPrint, Guid? trainClassId, string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<ActivistTrainResult> jsonResult = new JsonResultDatasModel<ActivistTrainResult>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<ActivistTrainResult>();
                if (!string.IsNullOrEmpty(keyword))
                {
                    filter = filter.And(d => d.PartyActivist.Name.Contains(keyword) || d.PartyActivist.JobNo.Contains(keyword));
                }
                if (departmentId != null)
                {
                    filter = filter.And(d => d.PartyActivist.DepartmentId == departmentId);
                }
                //if (!string.IsNullOrEmpty(year))
                //{
                //    filter = filter.And(d => d.PartyActivist.TrainClass.Year==year);
                //}
                //if (!string.IsNullOrEmpty(term))
                //{
                //    filter = filter.And(d => d.PartyActivist.TrainClass.Term ==(Term)Enum.Parse(typeof(Term), term));
                //}
                if (!string.IsNullOrEmpty(isPass))
                {
                    filter = filter.And(d => d.IsPass == (isPass == "true"));
                }
                if (!string.IsNullOrEmpty(isPrint))
                {
                    filter = filter.And(d => d.IsPrint == (isPrint == "true"));
                }
                if (trainClassId != null)
                {
                    filter = filter.And(d => d.PartyActivist.TrainClassId == trainClassId);
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                    var data = await _context.Set<ActivistTrainResult>().Include(d => d.PartyActivist).Include(d => d.PartyActivist.TrainClass).Where(filter).OrderByDescending(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<ActivistTrainResult>().Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<ActivistTrainResult>().Include(d => d.PartyActivist).Include(d => d.PartyActivist.TrainClass).Where(filter).Where(d => d.PartyActivist.DepartmentId == CurrentUser.DepartmentId).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<ActivistTrainResult>().Count();
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

        // GET: ActivistTrainResults/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var activistTrainResult = await _context.ActivistTrainResults
                    .Include(a => a.PartyActivist)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (activistTrainResult == null)
            {
                return NotFoundData();
            }

            return View(activistTrainResult);
        }

        // GET: ActivistTrainResults/Create
        public IActionResult Create()
        {
            ActivistTrainResult activistTrainResult = new ActivistTrainResult();
            ViewData["PartyActivistId"] = new SelectList(_context.PartyActivists, "Id", "ActiveApplicationTime");
            ViewData["TrainClassId"] = new SelectList(_context.TrainClasses, "Id", "Name");
            return View(activistTrainResult);
        }


        // GET: ActivistTrainResults/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var activistTrainResult = await _context.ActivistTrainResults.SingleOrDefaultAsync(m => m.Id == id);
            if (activistTrainResult == null)
            {
                return NotFoundData();
            }
            ViewData["PartyActivistId"] = new SelectList(_context.PartyActivists, "Id", "ActiveApplicationTime", activistTrainResult.PartyActivistId);
            return View(activistTrainResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("PartyActivistId,PsGrade,CsGrade,TotalGrade,IsPass,IsPrint,PrintTime,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] ActivistTrainResult activistTrainResult)
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
                    ActivistTrainResult activistTrainResultInDb = await _context.ActivistTrainResults.FindAsync(activistTrainResult.Id);
                    if (activistTrainResultInDb != null)
                    {
                        activistTrainResultInDb.PartyActivistId = activistTrainResult.PartyActivistId;
                        activistTrainResultInDb.PartyActivist = activistTrainResult.PartyActivist;
                        activistTrainResultInDb.PsGrade = activistTrainResult.PsGrade;
                        activistTrainResultInDb.CsGrade = activistTrainResult.CsGrade;
                        activistTrainResultInDb.TotalGrade = activistTrainResult.TotalGrade;
                        activistTrainResultInDb.IsPass = activistTrainResult.IsPass;
                        activistTrainResultInDb.IsPrint = activistTrainResult.IsPrint;
                        activistTrainResultInDb.PrintTime = activistTrainResult.PrintTime;
                        activistTrainResultInDb.Id = activistTrainResult.Id;
                        activistTrainResultInDb.CreateTime = activistTrainResult.CreateTime;
                        activistTrainResultInDb.OperatorId = activistTrainResult.OperatorId;
                        activistTrainResultInDb.Ordinal = activistTrainResult.Ordinal;
                        activistTrainResultInDb.IsDeleted = activistTrainResult.IsDeleted;
                        _context.Update(activistTrainResultInDb);
                    }
                    else
                    {
                        //activistTrainResult.Id = Guid.NewGuid();
                        _context.Add(activistTrainResult);
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

        [HttpPost]
        public async Task<IActionResult> SaveGradeData(string[] datas)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据保存成功"
            };
            try
            {
                foreach (var item in datas)
                {
                    string[] itemSub = item.Split(",");
                    if (itemSub.Length == 3)
                    {
                        Guid id = Guid.Parse(itemSub[0]);
                        ActivistTrainResult activistTrainResult = await _context.ActivistTrainResults.Include(d => d.PartyActivist.TrainClass).Where(d=>d.Id==id).FirstOrDefaultAsync();
                        if (activistTrainResult != null)
                        {
                            activistTrainResult.PsGrade = int.Parse(itemSub[1]);
                            activistTrainResult.CsGrade = int.Parse(itemSub[2]);
                            activistTrainResult.TotalGrade =Math.Round(activistTrainResult.PartyActivist.TrainClass.PsGradeProportion * activistTrainResult.PsGrade / 100 + activistTrainResult.PartyActivist.TrainClass.CsGradeProportion * activistTrainResult.CsGrade / 100,2);
                            if (activistTrainResult.TotalGrade >= 60)
                                activistTrainResult.IsPass = true;
                            else
                                activistTrainResult.IsPass = false;
                            _context.Update(activistTrainResult);
                        }
                    }

                }
                await _context.SaveChangesAsync();
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
        private bool ActivistTrainResultExists(Guid id)
        {
            return _context.ActivistTrainResults.Any(e => e.Id == id);
        }
    }
}
