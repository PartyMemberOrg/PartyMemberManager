using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PartyMemberManager.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using PartyMemberManager.Framework.Controllers;
using Microsoft.AspNetCore.Http;
using PartyMemberManager.Dal;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using PartyMemberManager.Core.Exceptions;
using Microsoft.EntityFrameworkCore;
using PartyMemberManager.Dal.Entities;

namespace PartyMemberManager.Controllers
{

    public class QueryScoreController : PartyMemberControllerBase
    {
        public QueryScoreController(ILogger<QueryScoreController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult GetQuery()
        {
            ViewBag.TrainClassTypeId = new SelectList(_context.TrainClassTypes.Where(d => d.Code.StartsWith("4")).OrderByDescending(d => d.Code), "Id", "Name");
            return View("query");
        }
        [AllowAnonymous]
        [HttpPost]
        public IActionResult PostQuery(QueryScoreViewModel model)
        {
            if (!ModelState.IsValid)
            {
                throw new PartyMemberException("查询数据不合法");
            }
            var trainClassType = _context.TrainClassTypes.Find(model.TrainClassTypeId);
            if (trainClassType.Name.Contains("入党积极分子"))
            {
                ActivistTrainResult activistTrainResult = _context.ActivistTrainResults.Include(d => d.PartyActivist)
                    .Where(d => d.PartyActivist.Name == model.Name && d.PartyActivist.IdNumber == model.IdNumber && d.PartyActivist.JobNo == model.JobNo).FirstOrDefault();
                if (activistTrainResult != null)
                {
                    YearTerm yearTerm = _context.YearTerms.Find(activistTrainResult.PartyActivist.YearTermId);
                    if (yearTerm.EnabledQuery == false)
                    {
                        model.IsQuery = false;
                        model.Message = "成绩查询未开通";
                        return View(model);
                    }
                    else if (yearTerm.StartTime > DateTime.Now || yearTerm.EndTime < DateTime.Now)
                    {
                        model.IsQuery = false;
                        model.Message = "请在"+string.Format("{0:yyyy-MM-dd}",yearTerm.StartTime)+"日至" + string.Format("{0:yyyy-MM-dd}", yearTerm.EndTime)+"日查询";
                        return View(model);
                    }
                }
                if (activistTrainResult != null && activistTrainResult.TotalGrade.HasValue)
                {
                    model.TrainClassType = trainClassType;
                    model.IsQuery = true;
                    model.TotalGrade = activistTrainResult.TotalGrade;
                    return View(model);
                }
                else
                {
                    model.IsQuery = false;
                    model.Message = "你的成绩还未录入";
                    return View(model);
                }
            }
            else if (trainClassType.Name.Contains("发展对象"))
            {
                PotentialTrainResult potentialTrainResult = _context.PotentialTrainResults.Include(d => d.PotentialMember)
                   .Where(d => d.PotentialMember.Name == model.Name && d.PotentialMember.IdNumber == model.IdNumber && d.PotentialMember.JobNo == model.JobNo).FirstOrDefault();
                if (potentialTrainResult != null)
                {
                    YearTerm yearTerm = _context.YearTerms.Find(potentialTrainResult.PotentialMember.YearTermId);
                    if (yearTerm.EnabledQuery == false)
                    {
                        model.IsQuery = false;
                        model.Message = "成绩查询未开通";
                        return View(model);
                    }
                    else if (yearTerm.StartTime>DateTime.Now || yearTerm.EndTime < DateTime.Now)
                    {
                        model.IsQuery = false;
                        model.Message = "请在" + string.Format("{0:yyyy-MM-dd}", yearTerm.StartTime) + "日至" + string.Format("{0:yyyy-MM-dd}", yearTerm.EndTime) + "日查询";
                        return View(model);
                    }
                }
                if (potentialTrainResult != null && potentialTrainResult.PotentialMember.IsPrint)
                {
                    model.TrainClassType = trainClassType;
                    model.IsQuery = true;
                    model.TotalGrade = potentialTrainResult.TotalGrade;
                    return View(model);
                }
                else if (potentialTrainResult != null && !potentialTrainResult.PotentialMember.IsPrint)
                {
                    model.IsQuery = false;
                    model.Message = "你的成绩还未录入";
                    return View(model);
                }
            }
            model.IsQuery = false;
            model.Message = "未查询到该学员的信息，请核对";
            return View(model);
        }
    }
}
