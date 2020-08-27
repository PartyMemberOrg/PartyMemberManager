﻿using System;
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
    public class SchoolCadreTrainsController : PartyMemberDataControllerBase<SchoolCadreTrain>
    {

        public SchoolCadreTrainsController(ILogger<SchoolCadreTrainsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: SchoolCadreTrains
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _context.SchoolCadreTrains.OrderBy(s => s.Ordinal).GetPagedDataAsync(page));
        }

        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(string year,  string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<SchoolCadreTrain> jsonResult = new JsonResultDatasModel<SchoolCadreTrain>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<SchoolCadreTrain>();
                if (year != null)
                {
                    filter = filter.And(d => d.Year == year);
                }
                if (keyword != null)
                {
                    filter = filter.And(d => d.Name.Contains(keyword) || d.Organizer.Contains(keyword) || d.TrainClassName.Contains(keyword) || d.TrainOrganizational.Contains(keyword));
                }
                var data = await _context.Set<SchoolCadreTrain>()
                    .Where(filter)
                    .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Count = _context.Set<SchoolCadreTrain>().Count();
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

        // GET: SchoolCadreTrains/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var schoolCadreTrain = await _context.SchoolCadreTrains
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
            return View(schoolCadreTrain);
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
                    var data = await _context.Set<SchoolCadreTrain>().SingleOrDefaultAsync(m => m.Id == id);
                    if (data == null)
                        throw new PartyMemberException("未找到要删除的数据");
                    ValidateDeleteObject(data);
                    _context.Set<SchoolCadreTrain>().Remove(data);
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
        public override async Task<IActionResult> Save([Bind("Year,Name,TrainClassName,Organizer,TrainOrganizational,TrainTime,TrainAddress,TrainDuration,ClassHour,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] SchoolCadreTrain schoolCadreTrain)
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
                        schoolCadreTrainInDb.Year = schoolCadreTrain.Year;
                        schoolCadreTrainInDb.Name = schoolCadreTrain.Name;
                        schoolCadreTrainInDb.TrainClassName = schoolCadreTrain.TrainClassName;
                        schoolCadreTrainInDb.Organizer = schoolCadreTrain.Organizer;
                        schoolCadreTrainInDb.TrainOrganizational = schoolCadreTrain.TrainOrganizational;
                        schoolCadreTrainInDb.TrainTime = schoolCadreTrain.TrainTime;
                        schoolCadreTrainInDb.TrainAddress = schoolCadreTrain.TrainAddress;
                        schoolCadreTrainInDb.TrainDuration = schoolCadreTrain.TrainDuration;
                        schoolCadreTrainInDb.ClassHour = schoolCadreTrain.ClassHour;
                        schoolCadreTrainInDb.Id = schoolCadreTrain.Id;
                        schoolCadreTrainInDb.CreateTime = schoolCadreTrain.CreateTime;
                        schoolCadreTrainInDb.OperatorId = schoolCadreTrain.OperatorId;
                        schoolCadreTrainInDb.Ordinal = schoolCadreTrain.Ordinal;
                        schoolCadreTrainInDb.IsDeleted = schoolCadreTrain.IsDeleted;
                        _context.Update(schoolCadreTrainInDb);
                    }
                    else
                    {
                        //schoolCadreTrain.Id = Guid.NewGuid();
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
