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
    public class ActivistTrainResultsController : PartyMemberDataControllerBase<ActivistTrainResult>
    {

        public ActivistTrainResultsController(ILogger<ActivistTrainResultsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: ActivistTrainResults
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.ActivistTrainResults.Include(a => a.PartyActivist).Include(a => a.TrainClass);
            return View(await pMContext.ToListAsync());
        }

        // GET: ActivistTrainResults/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var activistTrainResult = await _context.ActivistTrainResults
                    .Include(a => a.PartyActivist)
                    .Include(a => a.TrainClass)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (activistTrainResult == null)
            {
                return NotFoundData();
            }

            return View(activistTrainResult);
        }

        // GET: ActivistTrainResults/Create
        public IActionResult Create()
        {
            ActivistTrainResult activistTrainResult = new ActivistTrainResult();
            ViewData["PartyActivistId"] = new SelectList(_context.PartyActivists, "Id", "ActiveApplicationTime");
            ViewData["TrainClassId"] = new SelectList(_context.TrainClasses, "Id", "Name");
            return View(activistTrainResult);
        }


        // GET: ActivistTrainResults/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var activistTrainResult = await _context.ActivistTrainResults.SingleOrDefaultAsync(m => m.Id == id);
            if (activistTrainResult == null)
            {
                return NotFoundData();
            }
            ViewData["PartyActivistId"] = new SelectList(_context.PartyActivists, "Id", "ActiveApplicationTime", activistTrainResult.PartyActivistId);
            ViewData["TrainClassId"] = new SelectList(_context.TrainClasses, "Id", "Name", activistTrainResult.TrainClassId);
            return View(activistTrainResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("PartyActivistId,TrainClassId,PsGrade,CsGrade,TotalGrade,IsPass,IsPrint,PrintTime,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] ActivistTrainResult activistTrainResult)
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
                    ActivistTrainResult activistTrainResultInDb = await _context.ActivistTrainResults.FindAsync(activistTrainResult.Id);
                    if (activistTrainResultInDb != null)
                    {
                        activistTrainResultInDb.PartyActivistId = activistTrainResult.PartyActivistId;
                        activistTrainResultInDb.PartyActivist = activistTrainResult.PartyActivist;
                        activistTrainResultInDb.TrainClassId = activistTrainResult.TrainClassId;
                        activistTrainResultInDb.PsGrade = activistTrainResult.PsGrade;
                        activistTrainResultInDb.CsGrade = activistTrainResult.CsGrade;
                        activistTrainResultInDb.TotalGrade = activistTrainResult.TotalGrade;
                        activistTrainResultInDb.IsPass = activistTrainResult.IsPass;
                        activistTrainResultInDb.IsPrint = activistTrainResult.IsPrint;
                        activistTrainResultInDb.PrintTime = activistTrainResult.PrintTime;
                        activistTrainResultInDb.Id = activistTrainResult.Id;
                        activistTrainResultInDb.CreateTime = activistTrainResult.CreateTime;
                        activistTrainResultInDb.OperatorId = activistTrainResult.OperatorId;
                        activistTrainResultInDb.Ordinal = activistTrainResult.Ordinal;
                        activistTrainResultInDb.IsDeleted = activistTrainResult.IsDeleted;
                        _context.Update(activistTrainResultInDb);
                    }
                    else
                    {
                        //activistTrainResult.Id = Guid.NewGuid();
                        _context.Add(activistTrainResult);
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


        private bool ActivistTrainResultExists(Guid id)
        {
            return _context.ActivistTrainResults.Any(e => e.Id == id);
        }
    }
}
