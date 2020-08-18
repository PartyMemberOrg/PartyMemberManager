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
    public class PotentialTrainResultsController : PartyMemberDataControllerBase<PotentialTrainResult>
    {

        public PotentialTrainResultsController(ILogger<PotentialTrainResultsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: PotentialTrainResults
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.PotentialTrainResults.Include(p => p.PotentialMember);
            return View(await pMContext.ToListAsync());
        }

        // GET: PotentialTrainResults/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var potentialTrainResult = await _context.PotentialTrainResults
                    .Include(p => p.PotentialMember)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (potentialTrainResult == null)
            {
                return NotFoundData();
            }

            return View(potentialTrainResult);
        }

        // GET: PotentialTrainResults/Create
        public IActionResult Create()
        {
            PotentialTrainResult potentialTrainResult = new PotentialTrainResult();
            ViewData["PotentialMemberId"] = new SelectList(_context.PotentialMembers, "Id", "BirthDate");
            ViewData["TrainClassId"] = new SelectList(_context.TrainClasses, "Id", "Name");
            return View(potentialTrainResult);
        }


        // GET: PotentialTrainResults/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var potentialTrainResult = await _context.PotentialTrainResults.SingleOrDefaultAsync(m => m.Id == id);
            if (potentialTrainResult == null)
            {
                return NotFoundData();
            }
            ViewData["PotentialMemberId"] = new SelectList(_context.PotentialMembers, "Id", "BirthDate", potentialTrainResult.PotentialMemberId);
            return View(potentialTrainResult);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("PotentialMemberId,PsGrade,CsGrade,TotalGrade,IsPass,IsPrint,PrintTime,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PotentialTrainResult potentialTrainResult)
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
                    PotentialTrainResult potentialTrainResultInDb = await _context.PotentialTrainResults.FindAsync(potentialTrainResult.Id);
                    if (potentialTrainResultInDb != null)
                    {
                        potentialTrainResultInDb.PotentialMemberId = potentialTrainResult.PotentialMemberId;
                        potentialTrainResultInDb.PotentialMember = potentialTrainResult.PotentialMember;
                        potentialTrainResultInDb.PsGrade = potentialTrainResult.PsGrade;
                        potentialTrainResultInDb.CsGrade = potentialTrainResult.CsGrade;
                        potentialTrainResultInDb.TotalGrade = potentialTrainResult.TotalGrade;
                        potentialTrainResultInDb.IsPass = potentialTrainResult.IsPass;
                        potentialTrainResultInDb.IsPrint = potentialTrainResult.IsPrint;
                        potentialTrainResultInDb.PrintTime = potentialTrainResult.PrintTime;
                        potentialTrainResultInDb.Id = potentialTrainResult.Id;
                        potentialTrainResultInDb.CreateTime = potentialTrainResult.CreateTime;
                        potentialTrainResultInDb.OperatorId = potentialTrainResult.OperatorId;
                        potentialTrainResultInDb.Ordinal = potentialTrainResult.Ordinal;
                        potentialTrainResultInDb.IsDeleted = potentialTrainResult.IsDeleted;
                        _context.Update(potentialTrainResultInDb);
                    }
                    else
                    {
                        //potentialTrainResult.Id = Guid.NewGuid();
                        _context.Add(potentialTrainResult);
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


        private bool PotentialTrainResultExists(Guid id)
        {
            return _context.PotentialTrainResults.Any(e => e.Id == id);
        }
    }
}
