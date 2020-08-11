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
    public class TrainClassesController : PartyMemberDataControllerBase<TrainClass>
    {

        public TrainClassesController(ILogger<TrainClassesController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: TrainClasses
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.TrainClasses.Include(t => t.TrainClassType);
            return View(await pMContext.ToListAsync());
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
            ViewData["TrainClassTypeId"] = new SelectList(_context.TrainClassTypes, "Id", "Name");
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
            ViewData["TrainClassTypeId"] = new SelectList(_context.TrainClassTypes, "Id", "Name", trainClass.TrainClassTypeId);
            return View(trainClass);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("Name,Code,TrainClassTypeId,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] TrainClass trainClass)
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
                        trainClassInDb.Name = trainClass.Name;
                        trainClassInDb.Code = trainClass.Code;
                        trainClassInDb.TrainClassTypeId = trainClass.TrainClassTypeId;
                        trainClassInDb.TrainClassType = trainClass.TrainClassType;
                        trainClassInDb.Id = trainClass.Id;
                        trainClassInDb.CreateTime = trainClass.CreateTime;
                        trainClassInDb.OperatorId = trainClass.OperatorId;
                        trainClassInDb.Ordinal = trainClass.Ordinal;
                        trainClassInDb.IsDeleted = trainClass.IsDeleted;
                        _context.Update(trainClassInDb);
                    }
                    else
                    {
                        //trainClass.Id = Guid.NewGuid();
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


        private bool TrainClassExists(Guid id)
        {
            return _context.TrainClasses.Any(e => e.Id == id);
        }
    }
}
