using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AspNetCorePdf.PdfProvider;
using AspNetCorePdf.PdfProvider.DataModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PartyMemberManager.Core.Enums;
using PartyMemberManager.Dal;
using PartyMemberManager.Dal.Entities;
using PartyMemberManager.Models.PrintViewModel;
using PartyMemberManager.PdfProvider.DataModel;

namespace PartyMemberManager.Controllers
{
    public class PrintDataController : Controller
    {
        private readonly IPdfSharpService _pdfService;
        private readonly IMigraDocService _migraDocService;
        private IHttpContextAccessor _accessor;
        protected readonly PMContext _context;
        protected readonly ILogger<PrintDataController> _logger;
        public PrintDataController(ILogger<PrintDataController> logger, PMContext context, IHttpContextAccessor accessor, IPdfSharpService pdfService, IMigraDocService migraDocService)
        {
            _context = context;
            _logger = logger;
            _accessor = accessor;
            _pdfService = pdfService;
            _migraDocService = migraDocService;
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

        [HttpGet]
        public FileStreamResult CreatePdf()
        {
            var data = new PdfData
            {
                //A4 new XSize(595, 842);
                PageSize = new System.Drawing.Size(297,210),
                DocumentTitle = "入党积极分子培训结业证",
                DocumentName = "入党积极分子培训结业证",
                CreatedBy = "预备党员管理系统",
                Description = "预备党员管理系统",
                BackgroundImage = "ActivistTrain.jpg",
                DisplayItems = new List<DisplayItem>
                {
                    new DisplayItem{
                        Text="证书编号",
                        Font="楷体",
                        FontSize=14,
                        Location=new System.Drawing.Point(100,20)
                    },
                    new DisplayItem{
                        Text="证书内容",
                        Font="楷体",
                        FontSize=14,
                        Location=new System.Drawing.Point(100,30)
                    },
                    new DisplayItem{
                        Text="2020",
                        Font="楷体",
                        FontSize=14,
                        Location=new System.Drawing.Point(100,40)
                    },
                    new DisplayItem{
                        Text="08",
                        Font="楷体",
                        FontSize=14,
                        Location=new System.Drawing.Point(150,40)
                    },
                    new DisplayItem{
                        Text="26",
                        Font="楷体",
                        FontSize=14,
                        Location=new System.Drawing.Point(200,40)
                    }
                }

            };
            //var path = _pdfService.CreatePdf(data);

            //var stream = new FileStream(path, FileMode.Open);
            var stream= _pdfService.CreatePdf(data);
            return File(stream, "application/pdf");
        }
    }
}
