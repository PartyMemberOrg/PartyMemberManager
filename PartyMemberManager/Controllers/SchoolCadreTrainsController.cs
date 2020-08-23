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
    public class SchoolCadreTrainsController : PartyMemberDataControllerBase<SchoolCadreTrain>
    {

        public SchoolCadreTrainsController(ILogger<SchoolCadreTrainsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: SchoolCadreTrains
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.SchoolCadreTrains.Include(s => s.Department).Include(s => s.YearTerm);
            return View(await pMContext.ToListAsync());
        }
        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(Guid? departmentId, Guid? yearTermId, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<SchoolCadreTrain> jsonResult = new JsonResultDatasModel<SchoolCadreTrain>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<SchoolCadreTrain>();
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
                    var data = await _context.Set<SchoolCadreTrain>().Include(d => d.Department).Include(d => d.YearTerm).Where(filter).OrderByDescending(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<SchoolCadreTrain>().Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<SchoolCadreTrain>().Where(filter).Include(d => d.Department).Include(d => d.YearTerm).Where(d => d.DepartmentId == CurrentUser.DepartmentId).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<SchoolCadreTrain>().Count();
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
        // GET: SchoolCadreTrains/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var schoolCadreTrain = await _context.SchoolCadreTrains
                    .Include(s => s.Department)
                    .Include(s => s.YearTerm)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (schoolCadreTrain == null)
            {
                return NotFoundData();
            }

            return View(schoolCadreTrain);
        }

        // GET: SchoolCadreTrains/Create
        public IActionResult Create()
        {
            SchoolCadreTrain schoolCadreTrain = new SchoolCadreTrain();
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.Where(d => d.Enabled == true), "Id", "Name");
            return View(schoolCadreTrain);
        }


        // GET: SchoolCadreTrains/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var schoolCadreTrain = await _context.SchoolCadreTrains.SingleOrDefaultAsync(m => m.Id == id);
            if (schoolCadreTrain == null)
            {
                return NotFoundData();
            }
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.Where(d => d.Enabled == true), "Id", "Name");
            return View(schoolCadreTrain);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("YearTermId,Organizer,TrainOrganizational,TrainTime,TrainDuration,Name,Phone,IdNumber,Sex,DepartmentId,Post,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] SchoolCadreTrain schoolCadreTrain)
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
                    SchoolCadreTrain schoolCadreTrainInDb = await _context.SchoolCadreTrains.FindAsync(schoolCadreTrain.Id);
                    if (schoolCadreTrainInDb != null)
                    {
                        schoolCadreTrainInDb.YearTermId = schoolCadreTrain.YearTermId;
                        schoolCadreTrainInDb.Organizer = schoolCadreTrain.Organizer;
                        schoolCadreTrainInDb.TrainOrganizational = schoolCadreTrain.TrainOrganizational;
                        schoolCadreTrainInDb.TrainTime = schoolCadreTrain.TrainTime;
                        schoolCadreTrainInDb.TrainDuration = schoolCadreTrain.TrainDuration;
                        schoolCadreTrainInDb.Name = schoolCadreTrain.Name;
                        schoolCadreTrainInDb.Phone = schoolCadreTrain.Phone;
                        schoolCadreTrainInDb.IdNumber = schoolCadreTrain.IdNumber;
                        schoolCadreTrainInDb.Sex = schoolCadreTrain.Sex;
                        schoolCadreTrainInDb.DepartmentId = schoolCadreTrain.DepartmentId;
                        schoolCadreTrainInDb.Department = schoolCadreTrain.Department;
                        schoolCadreTrainInDb.Post = schoolCadreTrain.Post;
                        schoolCadreTrainInDb.Id = schoolCadreTrain.Id;
                        schoolCadreTrainInDb.CreateTime =DateTime.Now;
                        schoolCadreTrainInDb.OperatorId = CurrentUser.Id;
                        schoolCadreTrainInDb.Ordinal = _context.SchoolCadreTrains.Count()+1;
                        schoolCadreTrainInDb.IsDeleted = schoolCadreTrain.IsDeleted;
                        _context.Update(schoolCadreTrainInDb);
                    }
                    else
                    {
                        //schoolCadreTrain.Id = Guid.NewGuid();
                        schoolCadreTrain.CreateTime = DateTime.Now;
                        schoolCadreTrain.OperatorId = CurrentUser.Id;
                        schoolCadreTrain.Ordinal = _context.SchoolCadreTrains.Count() + 1;
                        _context.Add(schoolCadreTrain);
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


        private bool SchoolCadreTrainExists(Guid id)
        {
            return _context.SchoolCadreTrains.Any(e => e.Id == id);
        }
    }
}
