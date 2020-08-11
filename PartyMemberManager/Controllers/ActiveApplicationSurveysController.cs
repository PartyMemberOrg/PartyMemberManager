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
    public class ActiveApplicationSurveysController : PartyMemberDataControllerBase<ActiveApplicationSurvey>
    {

        public ActiveApplicationSurveysController(ILogger<ActiveApplicationSurveysController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: ActiveApplicationSurveys
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _context.ActiveApplicationSurveies.OrderBy(a => a.Ordinal).GetPagedDataAsync(page));
        }

        // GET: ActiveApplicationSurveys/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var activeApplicationSurvey = await _context.ActiveApplicationSurveies
            .SingleOrDefaultAsync(m => m.Id == id);
            if (activeApplicationSurvey == null)
            {
                return NotFoundData();
            }

            return View(activeApplicationSurvey);
        }

        // GET: ActiveApplicationSurveys/Create
        public IActionResult Create()
        {
            ActiveApplicationSurvey activeApplicationSurvey = new ActiveApplicationSurvey();
            return View(activeApplicationSurvey);
        }


        // GET: ActiveApplicationSurveys/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var activeApplicationSurvey = await _context.ActiveApplicationSurveies.SingleOrDefaultAsync(m => m.Id == id);
            if (activeApplicationSurvey == null)
            {
                return NotFoundData();
            }
            return View(activeApplicationSurvey);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("Year,SchoolArea,Term,Total,TrainTotal,Proportion,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] ActiveApplicationSurvey activeApplicationSurvey)
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
                    ActiveApplicationSurvey activeApplicationSurveyInDb = await _context.ActiveApplicationSurveies.FindAsync(activeApplicationSurvey.Id);
                    if (activeApplicationSurveyInDb != null)
                    {
                        activeApplicationSurveyInDb.Year = activeApplicationSurvey.Year;
                        activeApplicationSurveyInDb.SchoolArea = activeApplicationSurvey.SchoolArea;
                        activeApplicationSurveyInDb.Department = activeApplicationSurvey.Department;
                        activeApplicationSurveyInDb.Term = activeApplicationSurvey.Term;
                        activeApplicationSurveyInDb.Total = activeApplicationSurvey.Total;
                        activeApplicationSurveyInDb.TrainTotal = activeApplicationSurvey.TrainTotal;
                        activeApplicationSurveyInDb.Proportion = activeApplicationSurvey.Proportion;
                        activeApplicationSurveyInDb.Id = activeApplicationSurvey.Id;
                        activeApplicationSurveyInDb.CreateTime = activeApplicationSurvey.CreateTime;
                        activeApplicationSurveyInDb.OperatorId = activeApplicationSurvey.OperatorId;
                        activeApplicationSurveyInDb.Ordinal = activeApplicationSurvey.Ordinal;
                        activeApplicationSurveyInDb.IsDeleted = activeApplicationSurvey.IsDeleted;
                        _context.Update(activeApplicationSurveyInDb);
                    }
                    else
                    {
                        //activeApplicationSurvey.Id = Guid.NewGuid();
                        _context.Add(activeApplicationSurvey);
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


        private bool ActiveApplicationSurveyExists(Guid id)
        {
            return _context.ActiveApplicationSurveies.Any(e => e.Id == id);
        }
    }
}
