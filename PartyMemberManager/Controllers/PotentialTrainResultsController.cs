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
    public class PotentialTrainResultsController : PartyMemberDataControllerBase<PotentialTrainResult>
    {

        public PotentialTrainResultsController(ILogger<PotentialTrainResultsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: PotentialTrainResults
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.PotentialTrainResults.Include(p => p.PotentialMember);
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.OrderBy(d => d.Ordinal).Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.OrderBy(d => d.Ordinal), "Id", "Name");
            return View(await pMContext.ToListAsync());
        }
        public async Task<IActionResult> GetDatasWithFilter(Guid? yearTermId, Guid? departmentId, string isPass, string isPrint, Guid? trainClassId, string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<PotentialTrainResult> jsonResult = new JsonResultDatasModel<PotentialTrainResult>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<PotentialTrainResult>();
                if (!string.IsNullOrEmpty(keyword))
                {
                    filter = filter.And(d => d.PotentialMember.Name.Contains(keyword) || d.PotentialMember.JobNo.Contains(keyword));
                }
                if (departmentId != null)
                {
                    filter = filter.And(d => d.PotentialMember.DepartmentId == departmentId);
                }
                if (yearTermId != null)
                {
                    filter = filter.And(d => d.PotentialMember.YearTermId == yearTermId);
                }
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
                    filter = filter.And(d => d.PotentialMember.TrainClassId == trainClassId);
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                    var data = await _context.Set<PotentialTrainResult>().Include(d => d.PotentialMember).Include(d => d.PotentialMember.TrainClass).Include(d => d.PotentialMember.YearTerm)
                        .Where(filter).Where(d => d.PotentialMember.YearTerm.Enabled == true)
                        .OrderByDescending(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PotentialTrainResult>().Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<PotentialTrainResult>().Include(d => d.PotentialMember).Include(d => d.PotentialMember.TrainClass).Include(d => d.PotentialMember.YearTerm)
                        .Where(filter).Where(d => d.PotentialMember.YearTerm.Enabled == true)
                        .Where(d => d.PotentialMember.DepartmentId == CurrentUser.DepartmentId).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PotentialTrainResult>().Count();
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

        // GET: PotentialTrainResults/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var potentialTrainResult = await _context.PotentialTrainResults
                    .Include(p => p.PotentialMember)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (potentialTrainResult == null)
            {
                return NotFoundData();
            }

            return View(potentialTrainResult);
        }

        // GET: PotentialTrainResults/Create
        public IActionResult Create()
        {
            PotentialTrainResult potentialTrainResult = new PotentialTrainResult();
            ViewData["PotentialMemberId"] = new SelectList(_context.PotentialMembers, "Id", "BirthDate");
            ViewData["TrainClassId"] = new SelectList(_context.TrainClasses, "Id", "Name");
            return View(potentialTrainResult);
        }


        // GET: PotentialTrainResults/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var potentialTrainResult = await _context.PotentialTrainResults.SingleOrDefaultAsync(m => m.Id == id);
            if (potentialTrainResult == null)
            {
                return NotFoundData();
            }
            ViewData["PotentialMemberId"] = new SelectList(_context.PotentialMembers, "Id", "BirthDate", potentialTrainResult.PotentialMemberId);
            return View(potentialTrainResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("PotentialMemberId,PsGrade,CsGrade,TotalGrade,IsPass,IsPrint,PrintTime,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PotentialTrainResult potentialTrainResult)
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
                    PotentialTrainResult potentialTrainResultInDb = await _context.PotentialTrainResults.FindAsync(potentialTrainResult.Id);
                    if (potentialTrainResultInDb != null)
                    {
                        potentialTrainResultInDb.PotentialMemberId = potentialTrainResult.PotentialMemberId;
                        potentialTrainResultInDb.PotentialMember = potentialTrainResult.PotentialMember;
                        potentialTrainResultInDb.PsGrade = potentialTrainResult.PsGrade;
                        potentialTrainResultInDb.CsGrade = potentialTrainResult.CsGrade;
                        potentialTrainResultInDb.TotalGrade = potentialTrainResult.TotalGrade;
                        potentialTrainResultInDb.IsPass = potentialTrainResult.IsPass;
                        potentialTrainResultInDb.IsPrint = potentialTrainResult.IsPrint;
                        potentialTrainResultInDb.PrintTime = potentialTrainResult.PrintTime;
                        potentialTrainResultInDb.Id = potentialTrainResult.Id;
                        potentialTrainResultInDb.CreateTime = potentialTrainResult.CreateTime;
                        potentialTrainResultInDb.OperatorId = potentialTrainResult.OperatorId;
                        potentialTrainResultInDb.Ordinal = potentialTrainResult.Ordinal;
                        potentialTrainResultInDb.IsDeleted = potentialTrainResult.IsDeleted;
                        _context.Update(potentialTrainResultInDb);
                    }
                    else
                    {
                        //potentialTrainResult.Id = Guid.NewGuid();
                        _context.Add(potentialTrainResult);
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
                    var subItem = item.Split(",");
                    if (subItem.Length == 3)
                    {
                        Guid id = Guid.Parse(subItem[0]);
                        PotentialTrainResult potentialTrainResult = await _context.PotentialTrainResults.Include(d => d.PotentialMember.TrainClass).Where(d => d.Id == id).FirstOrDefaultAsync();
                        var psProp = potentialTrainResult.PotentialMember.TrainClass.PsGradeProportion;
                        var csProp = potentialTrainResult.PotentialMember.TrainClass.CsGradeProportion;
                        if (potentialTrainResult != null)
                        {
                            potentialTrainResult.PsGrade = decimal.Parse(subItem[1]);
                            potentialTrainResult.CsGrade = decimal.Parse(subItem[2]);
                            potentialTrainResult.TotalGrade = Math.Round(psProp * potentialTrainResult.PsGrade.Value / 100 + csProp * potentialTrainResult.CsGrade.Value / 100, 2);
                            if (potentialTrainResult.TotalGrade >= 60)
                                potentialTrainResult.IsPass = true;
                            else
                                potentialTrainResult.IsPass = false;
                            _context.Update(potentialTrainResult);

                            await _context.SaveChangesAsync();
                        }
                    }
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

        private bool PotentialTrainResultExists(Guid id)
        {
            return _context.PotentialTrainResults.Any(e => e.Id == id);
        }
    }
}
