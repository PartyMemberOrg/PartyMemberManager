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
    public class TrainClassTypesController : PartyMemberDataControllerBase<TrainClassType>
    {

        public TrainClassTypesController(ILogger<TrainClassTypesController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: TrainClassTypes
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _context.TrainClassTypes.OrderBy(t => t.Ordinal).GetPagedDataAsync(page));
        }

        // GET: TrainClassTypes/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var trainClassType = await _context.TrainClassTypes
            .SingleOrDefaultAsync(m => m.Id == id);
            if (trainClassType == null)
            {
                return NotFoundData();
            }

            return View(trainClassType);
        }

        // GET: TrainClassTypes/Create
        public IActionResult Create()
        {
            TrainClassType trainClassType = new TrainClassType();
            return View(trainClassType);
        }


        // GET: TrainClassTypes/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var trainClassType = await _context.TrainClassTypes.SingleOrDefaultAsync(m => m.Id == id);
            if (trainClassType == null)
            {
                return NotFoundData();
            }
            return View(trainClassType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("Name,Code,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] TrainClassType trainClassType)
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
                    TrainClassType trainClassTypeInDb = await _context.TrainClassTypes.FindAsync(trainClassType.Id);
                    if (trainClassTypeInDb != null)
                    {
                        trainClassTypeInDb.Name = trainClassType.Name;
                        trainClassTypeInDb.Code = trainClassType.Code;
                        trainClassTypeInDb.Id = trainClassType.Id;
                        trainClassTypeInDb.CreateTime = DateTime.Now;
                        trainClassTypeInDb.OperatorId = CurrentUser.Id;
                        trainClassTypeInDb.Ordinal = _context.TrainClassTypes.Count()+1;
                        trainClassTypeInDb.IsDeleted = trainClassType.IsDeleted;
                        _context.Update(trainClassTypeInDb);
                    }
                    else
                    {
                        //trainClassType.Id = Guid.NewGuid();
                        trainClassType.CreateTime = DateTime.Now;
                        trainClassType.OperatorId = CurrentUser.Id;
                        trainClassType.Ordinal = _context.TrainClassTypes.Count() + 1;
                        _context.Add(trainClassType);
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


        private bool TrainClassTypeExists(Guid id)
        {
            return _context.TrainClassTypes.Any(e => e.Id == id);
        }
    }
}
