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

namespace PartyMemberManager.Controllers
{
    public class ProvinceTrainClassesController : PartyMemberDataControllerBase<ProvinceTrainClass>
    {

        public ProvinceTrainClassesController(ILogger<ProvinceTrainClassesController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: ProvinceTrainClasses
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _context.ProvinceTrainClasses.OrderBy(p => p.Ordinal).GetPagedDataAsync(page));
        }

        public async Task<IActionResult> StatisticsTotal()
        {
            JsonResultDatasModel<ProvinceTrainClass> jsonResult = new JsonResultDatasModel<ProvinceTrainClass>
            {
                Code = 0,
                Msg = "统计成功"
            };
            try
            {
                var provinceTrainClasses = await _context.ProvinceTrainClasses.ToListAsync();
                foreach (var item in provinceTrainClasses)
                {
                    item.Total = _context.ProvinceCadreTrains.Where(d => d.ProvinceTrainClassId == item.Id).Count();
                    _context.Update(item);
                }
                await _context.SaveChangesAsync();
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
        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(string year, string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<ProvinceTrainClass> jsonResult = new JsonResultDatasModel<ProvinceTrainClass>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<ProvinceTrainClass>();
                if (year != null)
                {
                    filter = filter.And(d => d.Year == year);
                }
                if (keyword != null)
                {
                    filter = filter.And(d => d.Name.Contains(keyword));
                }
                var data = await _context.Set<ProvinceTrainClass>()
                    .Where(filter)
                    .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Count = _context.Set<ProvinceTrainClass>().Count();
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

        // GET: ProvinceTrainClasses/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var provinceTrainClass = await _context.ProvinceTrainClasses
            .SingleOrDefaultAsync(m => m.Id == id);
            if (provinceTrainClass == null)
            {
                return NotFoundData();
            }

            return View(provinceTrainClass);
        }

        // GET: ProvinceTrainClasses/Create
        public IActionResult Create()
        {
            ProvinceTrainClass provinceTrainClass = new ProvinceTrainClass();
            provinceTrainClass.Enabled = true;
            return View(provinceTrainClass);
        }


        // GET: ProvinceTrainClasses/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var provinceTrainClass = await _context.ProvinceTrainClasses.SingleOrDefaultAsync(m => m.Id == id);
            if (provinceTrainClass == null)
            {
                return NotFoundData();
            }
            return View(provinceTrainClass);
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
                var provinceCadreTrains = await _context.ProvinceCadreTrains.Where(d => d.ProvinceTrainClassId == id).ToListAsync();
                if (provinceCadreTrains.Count() > 0)
                    throw new PartyMemberException("培训班学员，不能直接删除");
                var data = await _context.Set<ProvinceTrainClass>().Where(DataFilter).SingleOrDefaultAsync(m => m.Id == id);
                if (data == null)
                    throw new PartyMemberException("未找到要删除的数据");
                ValidateDeleteObject(data);
                _context.Set<ProvinceTrainClass>().Remove(data);
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
        public override async Task<IActionResult> Save([Bind("Year,Name,Total,Enabled,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] ProvinceTrainClass provinceTrainClass)
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
                    ProvinceTrainClass provinceTrainClassInDb = await _context.ProvinceTrainClasses.FindAsync(provinceTrainClass.Id);
                    if (provinceTrainClassInDb != null)
                    {
                        provinceTrainClassInDb.Year = provinceTrainClass.Year;
                        provinceTrainClassInDb.Name = provinceTrainClass.Name;
                        provinceTrainClassInDb.Enabled = provinceTrainClass.Enabled;
                        provinceTrainClassInDb.Id = provinceTrainClass.Id;
                        provinceTrainClassInDb.CreateTime = DateTime.Now;
                        provinceTrainClassInDb.OperatorId = CurrentUser.Id;
                        provinceTrainClassInDb.Ordinal = _context.ProvinceTrainClasses.Count()+1;
                        provinceTrainClassInDb.IsDeleted = provinceTrainClass.IsDeleted;
                        _context.Update(provinceTrainClassInDb);
                    }
                    else
                    {
                        //provinceTrainClass.Id = Guid.NewGuid();
                        provinceTrainClass.CreateTime = DateTime.Now;
                        provinceTrainClass.OperatorId = CurrentUser.Id;
                        provinceTrainClass.Ordinal = _context.ProvinceTrainClasses.Count() + 1;
                        _context.Add(provinceTrainClass);
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


        private bool ProvinceTrainClassExists(Guid id)
        {
            return _context.ProvinceTrainClasses.Any(e => e.Id == id);
        }
    }
}
