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
    public class ProvinceCadreTrainsController : PartyMemberDataControllerBase<ProvinceCadreTrain>
    {

        public ProvinceCadreTrainsController(ILogger<ProvinceCadreTrainsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: ProvinceCadreTrains
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.ProvinceCadreTrains.Include(p => p.Nation).Include(p => p.ProvinceTrainClass);
            return View(await pMContext.ToListAsync());
        }
        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(string year, Guid? provinceTrainClassId, string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<ProvinceCadreTrain> jsonResult = new JsonResultDatasModel<ProvinceCadreTrain>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<ProvinceCadreTrain>();
                if (year != null)
                {
                    filter = filter.And(d => d.ProvinceTrainClass.Year == year);
                }
                if (provinceTrainClassId != null)
                {
                    filter = filter.And(d => d.ProvinceTrainClassId == provinceTrainClassId);
                }
                if (keyword != null)
                {
                    filter = filter.And(d => d.Name.Contains(keyword) || d.Department.Contains(keyword)|| d.Post.Contains(keyword));
                }
                var data = await _context.Set<ProvinceCadreTrain>().Include(d => d.ProvinceTrainClass).Include(d => d.Nation)
                    .Where(filter)
                    .Where(d => d.ProvinceTrainClass.Enabled == true)
                    .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Count = _context.Set<ProvinceCadreTrain>().Count();
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

        // GET: ProvinceCadreTrains/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var provinceCadreTrain = await _context.ProvinceCadreTrains
                    .Include(p => p.Nation)
                    .Include(p => p.ProvinceTrainClass)
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
            ViewData["NationId"] = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewData["ProvinceTrainClassId"] = new SelectList(_context.Set<ProvinceTrainClass>(), "Id", "Name");
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
            ViewData["NationId"] = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewData["ProvinceTrainClassId"] = new SelectList(_context.Set<ProvinceTrainClass>(), "Id", "Name", provinceCadreTrain.ProvinceTrainClassId);
            return View(provinceCadreTrain);
        }
        /// <summary>
        /// 删除数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("BatchDelete")]
        public async Task<IActionResult> BatchDelete(string idList)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };

            try
            {
                if (string.IsNullOrEmpty(idList))
                    throw new PartyMemberException("未选择删除的人员");
                string[] idListSplit = idList.Split(",");
                foreach (var item in idListSplit)
                {
                    var id = Guid.Parse(item);
                    var data = await _context.Set<ProvinceCadreTrain>().SingleOrDefaultAsync(m => m.Id == id);
                    if (data == null)
                        throw new PartyMemberException("未找到要删除的数据");
                    ValidateDeleteObject(data);
                    _context.Set<ProvinceCadreTrain>().Remove(data);
                }

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
        public override async Task<IActionResult> Save([Bind("ProvinceTrainClassId,Name,IdNumber,Sex,NationId,Department,Post,Phone,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] ProvinceCadreTrain provinceCadreTrain)
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
                        provinceCadreTrainInDb.ProvinceTrainClassId = provinceCadreTrain.ProvinceTrainClassId;
                        provinceCadreTrainInDb.ProvinceTrainClass = provinceCadreTrain.ProvinceTrainClass;
                        provinceCadreTrainInDb.Name = provinceCadreTrain.Name;
                        provinceCadreTrainInDb.IdNumber = provinceCadreTrain.IdNumber;
                        provinceCadreTrainInDb.Sex = provinceCadreTrain.Sex;
                        provinceCadreTrainInDb.NationId = provinceCadreTrain.NationId;
                        provinceCadreTrainInDb.Nation = provinceCadreTrain.Nation;
                        provinceCadreTrainInDb.Department = provinceCadreTrain.Department;
                        provinceCadreTrainInDb.Post = provinceCadreTrain.Post;
                        provinceCadreTrainInDb.Phone = provinceCadreTrain.Phone;
                        provinceCadreTrainInDb.CreateTime = DateTime.Now;
                        provinceCadreTrainInDb.OperatorId = CurrentUser.Id;
                        provinceCadreTrainInDb.Ordinal = _context.ProvinceCadreTrains.Count() + 1;
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
        /// <summary>
        /// 根据年份查询省级干部培训班
        /// </summary>

        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetProvinceTrainClasses(string year)
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


        private bool ProvinceCadreTrainExists(Guid id)
        {
            return _context.ProvinceCadreTrains.Any(e => e.Id == id);
        }
    }
}
