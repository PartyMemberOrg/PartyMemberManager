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
    public class ProvinceCadreTrainsController : PartyMemberDataControllerBase<ProvinceCadreTrain>
    {

        public ProvinceCadreTrainsController(ILogger<ProvinceCadreTrainsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: ProvinceCadreTrains
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.ProvinceCadreTrains.Include(p => p.Department).Include(p => p.YearTerm);
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
            JsonResultDatasModel<ProvinceCadreTrain> jsonResult = new JsonResultDatasModel<ProvinceCadreTrain>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<ProvinceCadreTrain>();
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
                    var data = await _context.Set<ProvinceCadreTrain>().Include(d => d.Department).Include(d => d.YearTerm).Where(filter).OrderByDescending(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<ProvinceCadreTrain>().Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<ProvinceCadreTrain>().Where(filter).Include(d => d.Department).Include(d => d.YearTerm).Where(d => d.DepartmentId == CurrentUser.DepartmentId).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<ProvinceCadreTrain>().Count();
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

        // GET: ProvinceCadreTrains/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var provinceCadreTrain = await _context.ProvinceCadreTrains
                    .Include(p => p.Department)
                    .Include(p => p.YearTerm)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (provinceCadreTrain == null)
            {
                return NotFoundData();
            }

            return View(provinceCadreTrain);
        }

        // GET: ProvinceCadreTrains/Create
        public IActionResult Create()
        {
            ProvinceCadreTrain provinceCadreTrain = new ProvinceCadreTrain();
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId= new SelectList(_context.YearTerms.Where(d=>d.Enabled==true), "Id", "Name");
            return View(provinceCadreTrain);
        }


        // GET: ProvinceCadreTrains/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var provinceCadreTrain = await _context.ProvinceCadreTrains.SingleOrDefaultAsync(m => m.Id == id);
            if (provinceCadreTrain == null)
            {
                return NotFoundData();
            }
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.Where(d => d.Enabled == true), "Id", "Name");
            return View(provinceCadreTrain);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("YearTermId,Organizer,TrainOrganizational,TrainTime,TrainDuration,Name,Phone,IdNumber,Sex,DepartmentId,Post,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] ProvinceCadreTrain provinceCadreTrain)
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
                    ProvinceCadreTrain provinceCadreTrainInDb = await _context.ProvinceCadreTrains.FindAsync(provinceCadreTrain.Id);
                    if (provinceCadreTrainInDb != null)
                    {
                        provinceCadreTrainInDb.YearTermId = provinceCadreTrain.YearTermId;
                        provinceCadreTrainInDb.Organizer = provinceCadreTrain.Organizer;
                        provinceCadreTrainInDb.TrainOrganizational = provinceCadreTrain.TrainOrganizational;
                        provinceCadreTrainInDb.TrainTime = provinceCadreTrain.TrainTime;
                        provinceCadreTrainInDb.TrainDuration = provinceCadreTrain.TrainDuration;
                        provinceCadreTrainInDb.Name = provinceCadreTrain.Name;
                        provinceCadreTrainInDb.Phone = provinceCadreTrain.Phone;
                        provinceCadreTrainInDb.IdNumber = provinceCadreTrain.IdNumber;
                        provinceCadreTrainInDb.Sex = provinceCadreTrain.Sex;
                        provinceCadreTrainInDb.DepartmentId = provinceCadreTrain.DepartmentId;
                        provinceCadreTrainInDb.Post = provinceCadreTrain.Post;
                        provinceCadreTrainInDb.Id = provinceCadreTrain.Id;
                        provinceCadreTrainInDb.CreateTime = DateTime.Now;
                        provinceCadreTrainInDb.OperatorId = CurrentUser.Id;
                        provinceCadreTrainInDb.Ordinal = _context.ProvinceCadreTrains.Count()+1;
                        provinceCadreTrainInDb.IsDeleted = provinceCadreTrain.IsDeleted;
                        _context.Update(provinceCadreTrainInDb);
                    }
                    else
                    {
                        //provinceCadreTrain.Id = Guid.NewGuid();
                        provinceCadreTrain.CreateTime = DateTime.Now;
                        provinceCadreTrain.OperatorId = CurrentUser.Id;
                        provinceCadreTrain.Ordinal = _context.ProvinceCadreTrains.Count() + 1;
                        _context.Add(provinceCadreTrain);
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


        private bool ProvinceCadreTrainExists(Guid id)
        {
            return _context.ProvinceCadreTrains.Any(e => e.Id == id);
        }
    }
}
