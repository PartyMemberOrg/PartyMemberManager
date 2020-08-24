using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PartyMemberManager.Core.Enums;
using PartyMemberManager.Dal;
using PartyMemberManager.Dal.Entities;
using PartyMemberManager.Models.PrintViewModel;

namespace PartyMemberManager.Controllers
{
    public class PrintDataController : Controller
    {
        private IHttpContextAccessor _accessor;
        protected readonly PMContext _context;
        protected readonly ILogger<PrintDataController> _logger;
        public PrintDataController(ILogger<PrintDataController> logger, PMContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _logger = logger;
            _accessor = accessor;
        }
        /// <summary>
        /// 获取入党积极分子结业证打印数据(设计报表用，因此只返回第一条)
        /// </summary>
        /// <param name="partyActivistId"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetActivistTrainPrintData()
        {
            List<PartyActivistPrintViewModel> models = new List<PartyActivistPrintViewModel>();
            List<ActivistTrainResult> activistTrainResults = await _context.ActivistTrainResults.ToListAsync();
            foreach (ActivistTrainResult activistTrainResult in activistTrainResults)
            {
                PartyActivist partyActivist = await _context.PartyActivists.FindAsync(activistTrainResult.PartyActivistId);
                YearTerm yearTerm = await _context.YearTerms.FindAsync(partyActivist.YearTermId);
                TrainClass trainClass = await _context.TrainClasses.FindAsync(partyActivist.TrainClassId);
                Department department = await _context.Departments.FindAsync(trainClass.DepartmentId);
                TrainClassType trainClassType = await _context.TrainClassTypes.FindAsync(trainClass.TrainClassTypeId);
                DateTime dateTime = DateTime.Today;
                //编号可能需要在录入成绩后生成，暂时生成1号结业证编号
                string no = string.Format("{0:yyyy}{1:00}{2:00}{0:MM}{3:000}", trainClass.StartTime.Value, trainClassType.Code, department.Code, 1);
                PartyActivistPrintViewModel model = new PartyActivistPrintViewModel
                {
                    No = no,
                    Name = partyActivist.Name,
                    StartYear = partyActivist.YearTerm.StartYear.ToString(),
                    EndYear = partyActivist.YearTerm.EndYear.ToString(),
                    Term = partyActivist.YearTerm.Term == Term.第一学期 ? "一" : "二",
                    Year = dateTime.Year.ToString(),
                    Month = dateTime.Month.ToString(),
                    Day = dateTime.Day.ToString()
                };
                models.Add(model);
            }
            return Json(models);
        }
    }
}
