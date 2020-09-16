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
using System.Transactions;

namespace PartyMemberManager.Controllers
{
    public class TrainClassesController : PartyMemberDataControllerBase<TrainClass>
    {

        public TrainClassesController(ILogger<TrainClassesController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: TrainClasses
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.TrainClasses.Include(t => t.TrainClassType).Include(t => t.YearTerm).Include(t => t.Department);
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClassTypeId = new SelectList(_context.TrainClassTypes.Where(d => d.Code.StartsWith("4")).OrderByDescending(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClassTypeId = new SelectList(_context.TrainClassTypes.OrderByDescending(d => d.Ordinal), "Id", "Name");
            //if (CurrentUser.Roles == Role.学院党委)
            //    ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Where(d => d.DepartmentId == CurrentUser.DepartmentId).OrderByDescending(d => d.Ordinal), "Id", "Name");
            //else
            //    ViewBag.TrainClasses = new SelectList(_context.TrainClassTypes.OrderByDescending(d => d.Ordinal), "Id", "Name");
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderBy(d => d.Ordinal), "Id", "Name");
            return View(await pMContext.ToListAsync());
        }
        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(Guid? trainClassTypeId, Guid? departmentId, string keyword, Guid? yearTermId, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<TrainClass> jsonResult = new JsonResultDatasModel<TrainClass>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<TrainClass>();
                if (yearTermId != null)
                {
                    filter = filter.And(d => d.YearTermId == yearTermId);
                }
                if (trainClassTypeId != null)
                {
                    filter = filter.And(d => d.TrainClassTypeId == trainClassTypeId);
                }
                if (departmentId != null)
                {
                    filter = filter.And(d => d.DepartmentId == departmentId);
                }
                if (keyword != null)
                {
                    filter = filter.And(d => d.Name.Contains(keyword));
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                    var data = await _context.Set<TrainClass>().Include(d => d.TrainClassType).Include(d => d.YearTerm).Include(d => d.Department)
                        //.Where(d => d.YearTerm.Enabled == true)
                        .Where(filter).OrderByDescending(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<TrainClass>().Where(filter).Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<TrainClass>().Include(d => d.TrainClassType).Include(d => d.YearTerm).Include(d => d.Department)
                        .Where(filter)
                        .Where(d => d.YearTerm.Enabled == true)
                        .Where(d => d.DepartmentId == CurrentUser.DepartmentId).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<TrainClass>().Include(d => d.TrainClassType).Include(d => d.YearTerm).Include(d => d.Department)
                        .Where(filter)
                        .Where(d => d.YearTerm.Enabled == true)
                        .Where(d => d.DepartmentId == CurrentUser.DepartmentId).Count();
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
        // GET: TrainClasses/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var trainClass = await _context.TrainClasses
                    .Include(t => t.TrainClassType)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (trainClass == null)
            {
                return NotFoundData();
            }

            return View(trainClass);
        }

        // GET: TrainClasses/Create
        public IActionResult Create()
        {
            TrainClass trainClass = new TrainClass();
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClassTypeId = new SelectList(_context.TrainClassTypes.Where(d => d.Code.StartsWith("4")).OrderByDescending(d => d.Code), "Id", "Name");
            else
                ViewBag.TrainClassTypeId = new SelectList(_context.TrainClassTypes.OrderByDescending(d => d.Code), "Id", "Name");
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderByDescending(d => d.StartYear).ThenByDescending(d => d.Term).Where(d => d.Enabled == true), "Id", "Name");
            return View(trainClass);
        }


        // GET: TrainClasses/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var trainClass = await _context.TrainClasses.SingleOrDefaultAsync(m => m.Id == id);
            if (trainClass == null)
            {
                return NotFoundData();
            }
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClassTypeId = new SelectList(_context.TrainClassTypes.Where(d => d.Code.StartsWith("4")).OrderByDescending(d => d.Code), "Id", "Name", trainClass.TrainClassTypeId);
            else
                ViewBag.TrainClassTypeId = new SelectList(_context.TrainClassTypes.OrderByDescending(d => d.Code), "Id", "Name", trainClass.TrainClassTypeId);
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name", trainClass.DepartmentId);
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderByDescending(d => d.StartYear).ThenByDescending(d => d.Term).Where(d => d.Enabled == true), "Id", "Name");
            return View(trainClass);
        }
        /// <summary>
        /// 删除数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        public override async Task<IActionResult> Delete(Guid? id)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };

            try
            {
                if (id == null)
                    throw new PartyMemberException("未传入删除项目的Id");
                var partyActivits = await _context.PartyActivists.Where(d => d.TrainClassId == id).ToListAsync();
                if(partyActivits.Count()>0)
                    throw new PartyMemberException("培训班中有入党积极分子，不能直接删除");
                var potentialMembers = await _context.PotentialMembers.Where(d => d.TrainClassId == id).ToListAsync();
                if (potentialMembers.Count() > 0)
                    throw new PartyMemberException("培训班中有发展对象，不能直接删除");
                var data = await _context.Set<TrainClass>().Where(DataFilter).SingleOrDefaultAsync(m => m.Id == id);
                if (data == null)
                    throw new PartyMemberException("未找到要删除的数据");
                ValidateDeleteObject(data);
                _context.Set<TrainClass>().Remove(data);
                await _context.SaveChangesAsync();
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
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("YearTermId,Name,TrainClassTypeId,StartTime,PsGradeProportion,CsGradeProportion,SjGradeProportion,DepartmentId,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] TrainClass trainClass)
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
                    TrainClass trainClassInDb = await _context.TrainClasses.FindAsync(trainClass.Id);
                    if (trainClassInDb != null)
                    {
                        trainClassInDb.YearTermId = trainClass.YearTermId;
                        trainClassInDb.Name = trainClass.Name;
                        trainClassInDb.TrainClassTypeId = trainClass.TrainClassTypeId;
                        trainClassInDb.TrainClassType = trainClass.TrainClassType;
                        trainClassInDb.StartTime = trainClass.StartTime;
                        trainClassInDb.PsGradeProportion = trainClass.PsGradeProportion;
                        trainClassInDb.CsGradeProportion = trainClass.CsGradeProportion;
                        trainClassInDb.SjGradeProportion = trainClass.SjGradeProportion;
                        trainClassInDb.Id = trainClass.Id;
                        trainClassInDb.CreateTime = DateTime.Now;
                        trainClassInDb.OperatorId = CurrentUser.Id;
                        trainClassInDb.Ordinal = _context.TrainClasses.Count() + 1;
                        if (CurrentUser.Roles == Role.学院党委)
                            trainClassInDb.DepartmentId = CurrentUser.DepartmentId.Value;
                        else
                            trainClassInDb.DepartmentId = trainClass.DepartmentId;
                        _context.Update(trainClassInDb);
                    }
                    else
                    {
                        //trainClass.Id = Guid.NewGuid();
                        trainClass.CreateTime = DateTime.Now;
                        trainClass.OperatorId = CurrentUser.Id;
                        trainClass.Ordinal = _context.TrainClasses.Count() + 1;
                        if (CurrentUser.Roles == Role.学院党委)
                            trainClass.DepartmentId = CurrentUser.DepartmentId.Value;
                        //trainClass.YearTerm = trainClass.Year + "-" + (int.Parse(trainClass.Year) + 1) + "学年" + trainClass.Term;
                        _context.Add(trainClass);
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
        /// <summary>
        /// 重载获取数据函数，主要史需要Include
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public override async Task<IActionResult> GetDatas(int page = 1, int limit = 10)
        {
            JsonResultDatasModel<TrainClass> jsonResult = new JsonResultDatasModel<TrainClass>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var data = await _context.Set<TrainClass>().Include(d => d.TrainClassType).Include(d => d.YearTerm).Include(d => d.Department).Where(DataFilter).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Count = _context.Set<TrainClass>().Where(DataFilter).Count();
                jsonResult.Data = data.Data;
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

        private bool TrainClassExists(Guid id)
        {
            return _context.TrainClasses.Any(e => e.Id == id);
        }
    }
}
