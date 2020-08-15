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
    public class PartyActivistsController : PartyMemberDataControllerBase<PartyActivist>
    {

        public PartyActivistsController(ILogger<PartyActivistsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: PartyActivists
        public async Task<IActionResult> Index(int page = 1)
        {
            var partyActives = _context.PartyActivists.Include(d=>d.Department).Include(d=>d.Nation);
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.Nations = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            return View(await partyActives.OrderBy(a => a.Ordinal).GetPagedDataAsync(page));
        }
        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(Guid? departmentId,string term, string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<PartyActivist> jsonResult = new JsonResultDatasModel<PartyActivist>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<PartyActivist>();
                if (departmentId != null)
                {
                    filter = filter.And(d => d.DepartmentId == departmentId);
                }
                if (keyword != null)
                {
                    filter = filter.And(d => d.Name.Contains(keyword)|| d.JobNo.Contains(keyword));
                }
                if (term != null)
                {
                    filter = filter.And(d => d.Term==(Term)Enum.Parse(typeof(Term),term));
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                    var data = await _context.Set<PartyActivist>().Include(d => d.Department).Include(d=>d.Nation).Where(filter)
                        .OrderByDescending(o => o.Year).ThenByDescending(d=>d.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PartyActivist>().Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<PartyActivist>().Where(filter).Include(d => d.Department).Include(d => d.Nation).Where(d => d.DepartmentId == CurrentUser.DepartmentId)
                        .OrderByDescending(o => o.Year).ThenByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PartyActivist>().Count();
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
        // GET: PartyActivists/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var partyActivist = await _context.PartyActivists
                    .Include(p => p.Department)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (partyActivist == null)
            {
                return NotFoundData();
            }

            return View(partyActivist);
        }

        // GET: PartyActivists/Create
        public IActionResult Create()
        {
            PartyActivist partyActivist = new PartyActivist();
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.Nations = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            return View(partyActivist);
        }


        // GET: PartyActivists/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var partyActivist = await _context.PartyActivists.SingleOrDefaultAsync(m => m.Id == id);
            if (partyActivist == null)
            {
                return NotFoundData();
            }
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.Nations = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            return View(partyActivist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("Year,Term,ApplicationTime,ActiveApplicationTime,Duty,Name,JobNo,IdNumber,Sex,PartyMemberType,BirthDate,NationId,Phone,DepartmentId,Class,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PartyActivist partyActivist)
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
                    PartyActivist partyActivistInDb = await _context.PartyActivists.FindAsync(partyActivist.Id);
                    if (partyActivistInDb != null)
                    {
                        partyActivistInDb.Year = partyActivist.Year;
                        partyActivistInDb.ApplicationTime = partyActivist.ApplicationTime;
                        partyActivistInDb.ActiveApplicationTime = partyActivist.ActiveApplicationTime;
                        partyActivistInDb.Duty = partyActivist.Duty;
                        partyActivistInDb.Name = partyActivist.Name;
                        partyActivistInDb.JobNo = partyActivist.JobNo;
                        partyActivistInDb.IdNumber = partyActivist.IdNumber;
                        partyActivistInDb.PartyMemberType = partyActivist.PartyMemberType;
                        partyActivistInDb.BirthDate = partyActivist.BirthDate;
                        partyActivistInDb.Phone = partyActivist.Phone;
                        partyActivistInDb.Department = partyActivist.Department;
                        partyActivistInDb.Class = partyActivist.Class;
                        partyActivistInDb.Id = partyActivist.Id;
                        partyActivistInDb.CreateTime = DateTime.Now;
                        partyActivistInDb.OperatorId = CurrentUser.Id;
                        partyActivistInDb.Ordinal = _context.PartyActivists.Count() + 1;
                        partyActivistInDb.IsDeleted = partyActivist.IsDeleted;
                        if (partyActivist.NationId == null)
                            throw new PartyMemberException("请选择民族");
                        else
                            partyActivistInDb.NationId = partyActivist.NationId;
                        if (partyActivist.Sex.ToString() == null)
                            throw new PartyMemberException("请选择性别");
                        else
                            partyActivistInDb.Sex = partyActivist.Sex;
                        if (partyActivist.Term.ToString() == null)
                            throw new PartyMemberException("请选择学期");
                        else
                            partyActivistInDb.Term = partyActivist.Term;

                        if (CurrentUser.Roles == Role.学院党委)
                            partyActivistInDb.DepartmentId = CurrentUser.DepartmentId.Value;
                        else
                        {
                            if (partyActivist.DepartmentId == null)
                                throw new PartyMemberException("请选择部门");
                            partyActivistInDb.DepartmentId = partyActivist.DepartmentId;
                        }
                        _context.Update(partyActivistInDb);
                    }
                    else
                    {
                        //partyActivist.Id = Guid.NewGuid();
                        if (partyActivist.NationId == null)
                            throw new PartyMemberException("请选择民族");
                        if (partyActivist.Sex.ToString() == null)
                            throw new PartyMemberException("请选择性别");
                        if (partyActivist.Term.ToString() == null)
                            throw new PartyMemberException("请选择学期");
                        if (CurrentUser.Roles == Role.学院党委)
                            partyActivist.DepartmentId = CurrentUser.DepartmentId.Value;
                        else
                        {
                            if (partyActivist.DepartmentId == null)
                                throw new PartyMemberException("请选择部门");
                        }
                        _context.Add(partyActivist);
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


        private bool PartyActivistExists(Guid id)
        {
            return _context.PartyActivists.Any(e => e.Id == id);
        }
    }
}
