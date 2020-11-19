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
using PartyMemberManager.Core.Enums;
using PartyMemberManager.Models;
using System.IO;
using System.Data;
using ExcelCore;
using Newtonsoft.Json;
using PartyMemberManager.Models.PrintViewModel;
using AspNetCorePdf.PdfProvider.DataModel;
using PartyMemberManager.PdfProvider.DataModel;
using AspNetCorePdf.PdfProvider;
using Microsoft.DotNet.PlatformAbstractions;

namespace PartyMemberManager.Controllers
{
    public class PotentialTrainResultsController : PartyMemberDataControllerBase<PotentialTrainResult>
    {

        private readonly IPdfSharpService _pdfService;
        private readonly IMigraDocService _migraDocService;
        private readonly string printSessionKey = null;
        public PotentialTrainResultsController(ILogger<PotentialTrainResultsController> logger, PMContext context, IHttpContextAccessor accessor, IPdfSharpService pdfService, IMigraDocService migraDocService) : base(logger, context, accessor)
        {
            _pdfService = pdfService;
            _migraDocService = migraDocService;
            printSessionKey = $"PotentialTrainResultPrint_{_accessor.HttpContext.Connection.RemoteIpAddress.ToString()}";
        }

        // GET: PotentialTrainResults
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.PotentialTrainResults.Include(p => p.PotentialMember);
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClassId = new SelectList(_context.TrainClasses.Include(t => t.YearTerm).Include(d => d.TrainClassType)
                    .Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value && d.YearTerm.Enabled == true)
                    .Where(d => d.TrainClassType.Code == "42")
                    .OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClassId = new SelectList(_context.TrainClasses.Include(t => t.YearTerm).Include(d => d.TrainClassType)
                    .Where(d => d.YearTerm.Enabled == true)
                    .Where(d => d.TrainClassType.Code == "42")
                    .OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderByDescending(d => d.StartYear).ThenByDescending(d => d.Term).Where(d => d.Enabled == true), "Id", "Name");
            ViewBag.TrainClassTypeId = _context.TrainClassTypes.Where(d => d.Code == "42").Select(d => d.Id).SingleOrDefault();
            return View(await pMContext.ToListAsync());
        }
        public async Task<IActionResult> GetDatasWithFilter(Guid? yearTermId, Guid? departmentId, string isPass, string isPrint, string isBcGrade, Guid? trainClassId, string keyword,BatchType batch, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<PotentialTrainResult> jsonResult = new JsonResultDatasModel<PotentialTrainResult>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<PotentialTrainResult>();
                if (!string.IsNullOrEmpty(keyword))
                {
                    filter = filter.And(d => d.PotentialMember.Name.Contains(keyword) || d.PotentialMember.JobNo.Contains(keyword));
                }
                if (departmentId != null)
                {
                    filter = filter.And(d => d.PotentialMember.DepartmentId == departmentId);
                }
                if (yearTermId != null)
                {
                    filter = filter.And(d => d.PotentialMember.YearTermId == yearTermId);
                }
                if (!string.IsNullOrEmpty(isPass))
                {
                    filter = filter.And(d => d.IsPass == (isPass == "true"));
                }
                if (!string.IsNullOrEmpty(isBcGrade))
                {
                    if (isBcGrade == "true")
                        filter = filter.And(d => d.BcGrade != null);
                    else if (isBcGrade == "false")
                        filter = filter.And(d => d.BcGrade == null);
                }
                if (!string.IsNullOrEmpty(isPrint))
                {
                    filter = filter.And(d => d.PotentialMember.IsPrint == (isPrint == "true"));
                }
                if (trainClassId != null)
                {
                    filter = filter.And(d => d.PotentialMember.TrainClassId == trainClassId);
                }
                if ((int)batch > 0)
                {
                    filter = filter.And(d => d.PotentialMember.TrainClass.Batch == batch);
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                    var data = await _context.Set<PotentialTrainResult>().Include(d => d.PotentialMember).Include(d => d.PotentialMember.TrainClass).Include(d => d.PotentialMember.YearTerm)
                        .Where(filter)
                        //.Where(d => d.PotentialMember.YearTerm.Enabled == true)
                        .OrderByDescending(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PotentialTrainResult>().Where(filter).Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<PotentialTrainResult>().Include(d => d.PotentialMember).Include(d => d.PotentialMember.TrainClass).Include(d => d.PotentialMember.YearTerm)
                        .Where(filter).Where(d => d.PotentialMember.YearTerm.Enabled == true)
                        .Where(d => d.PotentialMember.DepartmentId == CurrentUser.DepartmentId).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PotentialTrainResult>().Include(d => d.PotentialMember).Include(d => d.PotentialMember.TrainClass).Include(d => d.PotentialMember.YearTerm)
                        .Where(filter).Where(d => d.PotentialMember.YearTerm.Enabled == true)
                        .Where(d => d.PotentialMember.DepartmentId == CurrentUser.DepartmentId).Count();
                    jsonResult.Data = data.Data;
                }
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
        // GET: ActivistTrainResults/Edit/5
        public async Task<IActionResult> BcGrade(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var potentialTrainResult = await _context.PotentialTrainResults.Include(d => d.PotentialMember).Include(d => d.PotentialMember.TrainClass).Include(d => d.PotentialMember.TrainClass.YearTerm).SingleOrDefaultAsync(m => m.Id == id);
            if (potentialTrainResult == null)
            {
                return NotFoundData();
            }
            //ViewData["PartyActivistId"] = new SelectList(_context.PartyActivists, "Id", "ActiveApplicationTime", activistTrainResult.PartyActivistId);
            return View(potentialTrainResult);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BcGrade([Bind("Id,PotentialMemberId,BcGrade")] PotentialTrainResult potentialTrainResult)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据保存成功"
            };
            try
            {

                PotentialTrainResult potentialTrainResultInDb = await _context.PotentialTrainResults.Include(d => d.PotentialMember.TrainClass).Where(d => d.Id == potentialTrainResult.Id).FirstOrDefaultAsync();
                PotentialMember potentialMember = await _context.PotentialMembers.FindAsync(potentialTrainResult.PotentialMemberId);
                var psProp = potentialTrainResultInDb.PotentialMember.TrainClass.PsGradeProportion;
                var csProp = potentialTrainResultInDb.PotentialMember.TrainClass.CsGradeProportion;
                if (potentialTrainResultInDb == null)
                    throw new PartyMemberException("未找到数据");
                if (!potentialTrainResult.BcGrade.HasValue)
                    throw new PartyMemberException("请输入补考成绩");
                decimal bcGrade = potentialTrainResult.BcGrade.Value;
                decimal psGrade = potentialTrainResultInDb.PsGrade.Value;
                if (bcGrade > 100 || bcGrade < 0)
                    throw new PartyMemberException($"【{potentialMember.JobNo}-{potentialMember.Name}】的补考成绩非法");
                potentialTrainResultInDb.BcGrade = bcGrade;
                potentialTrainResultInDb.TotalGrade = Math.Round(psProp * psGrade / 100 + csProp * bcGrade / 100, 2);
                if (potentialTrainResultInDb.TotalGrade >= 60)
                    potentialTrainResultInDb.IsPass = true;
                else
                    potentialTrainResultInDb.IsPass = false;
                _context.Update(potentialTrainResultInDb);
                await _context.SaveChangesAsync();
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("PotentialMemberId,PsGrade,CsGrade,TotalGrade,IsPass,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PotentialTrainResult potentialTrainResult)
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
                        //potentialTrainResultInDb.IsPrint = potentialTrainResult.IsPrint;
                        //potentialTrainResultInDb.PrintTime = potentialTrainResult.PrintTime;
                        //potentialTrainResultInDb.Id = potentialTrainResult.Id;
                        //potentialTrainResultInDb.CreateTime = potentialTrainResult.CreateTime;
                        //potentialTrainResultInDb.OperatorId = potentialTrainResult.OperatorId;
                        //potentialTrainResultInDb.Ordinal = potentialTrainResult.Ordinal;
                        //potentialTrainResultInDb.IsDeleted = potentialTrainResult.IsDeleted;
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
        [HttpPost]
        public async Task<IActionResult> SaveGradeData(string[] datas)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据保存成功"
            };
            try
            {
                foreach (var item in datas)
                {
                    var subItem = item.Split(",");
                    if (subItem.Length == 4)
                    {
                        Guid id = Guid.Parse(subItem[0]);
                        PotentialTrainResult potentialTrainResult = await _context.PotentialTrainResults.Include(d => d.PotentialMember.TrainClass).Where(d => d.Id == id).FirstOrDefaultAsync();
                        PotentialMember potentialMember = await _context.PotentialMembers.FindAsync(potentialTrainResult.PotentialMemberId);
                        var psProp = potentialTrainResult.PotentialMember.TrainClass.PsGradeProportion;
                        var sjProp = potentialTrainResult.PotentialMember.TrainClass.SjGradeProportion;
                        var csProp = potentialTrainResult.PotentialMember.TrainClass.CsGradeProportion;
                        if (potentialTrainResult != null && potentialMember.IsPrint == false)
                        {
                            decimal psGrade = 0;
                            decimal sjGrade = 0;
                            decimal csGrade = 0;
                            if (decimal.TryParse(subItem[1], out psGrade))
                                potentialTrainResult.PsGrade = psGrade;
                            if (decimal.TryParse(subItem[2], out sjGrade))
                                potentialTrainResult.SjGrade = sjGrade;
                            if (decimal.TryParse(subItem[3], out csGrade))
                                potentialTrainResult.CsGrade = csGrade;
                            if (psGrade > 100 || psGrade < 0)
                                throw new PartyMemberException($"【{potentialMember.JobNo}-{potentialMember.Name}】的平时成绩非法");
                            if (sjGrade > 100 || sjGrade < 0)
                                throw new PartyMemberException($"【{potentialMember.JobNo}-{potentialMember.Name}】的实践成绩非法");
                            if (csGrade > 100 || csGrade < 0)
                                throw new PartyMemberException($"【{potentialMember.JobNo}-{potentialMember.Name}】的考试成绩非法");
                            potentialTrainResult.TotalGrade = Math.Round(psProp * psGrade / 100 + sjProp * sjGrade / 100 + csProp * csGrade / 100, 2);
                            if (potentialTrainResult.TotalGrade >= 60)
                                potentialTrainResult.IsPass = true;
                            else
                                potentialTrainResult.IsPass = false;
                            _context.Update(potentialTrainResult);

                            await _context.SaveChangesAsync();
                        }
                    }
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
        private async Task<List<PotentialMemberPrintViewModel>> GetReportDatas(Guid[] ids, DateTime dateTime)
        {
            List<PotentialMemberPrintViewModel> datas = new List<PotentialMemberPrintViewModel>();
            foreach (Guid id in ids)
            {
                PotentialTrainResult potentialTrainResult = await _context.PotentialTrainResults.FindAsync(id);
                //如果成绩和补考成绩均不合格，不能打印，也不生成证书编号
                if (!potentialTrainResult.IsPass)
                    continue;
                PotentialMember potentialMember = await _context.PotentialMembers.FindAsync(potentialTrainResult.PotentialMemberId);
                YearTerm yearTerm = await _context.YearTerms.FindAsync(potentialMember.YearTermId);
                TrainClass trainClass = await _context.TrainClasses.FindAsync(potentialMember.TrainClassId);
                Department department = await _context.Departments.FindAsync(trainClass.DepartmentId);
                TrainClassType trainClassType = await _context.TrainClassTypes.FindAsync(trainClass.TrainClassTypeId);
                //DateTime dateTime = DateTime.Today;
                //编号可能需要在录入成绩后生成，暂时生成1号结业证编号
                string no = null;
                if (string.IsNullOrEmpty(potentialTrainResult.CertificateNumber))
                {
                    PotentialTrainResult potentialTrainResultLast = await _context.PotentialTrainResults.Include(p => p.PotentialMember).Where(p => p.PotentialMember.TrainClassId == trainClass.Id && p.CertificateOrder > 0).OrderByDescending(p => p.CertificateOrder).FirstOrDefaultAsync();
                    int certificateOrder = 1;
                    if (potentialTrainResultLast != null)
                        certificateOrder = potentialTrainResultLast.CertificateOrder.Value + 1;
                    no = string.Format("{0:yyyy}{1:00}{2:00}{0:MM}{3:000}", trainClass.StartTime.Value, trainClassType.Code, department.Code, certificateOrder);
                    //更新证书编号
                    potentialTrainResult.CertificateOrder = certificateOrder;
                    potentialTrainResult.CertificateNumber = no;
                }
                else
                {
                    no = potentialTrainResult.CertificateNumber;
                }
                //现在只有发展对象表中有isPrint
                //potentialMember.IsPrint = true;
                //potentialMember.PrintTime = DateTime.Now;
                await _context.SaveChangesAsync();
                PotentialMemberPrintViewModel model = new PotentialMemberPrintViewModel
                {
                    No = no,
                    Name = potentialMember.Name,
                    StartYear = potentialMember.YearTerm.StartYear.ToString(),
                    EndYear = potentialMember.YearTerm.EndYear.ToString(),
                    Term = potentialMember.YearTerm.Term == Term.第一学期 ? "一" : "二",
                    Year = dateTime.Year.ToString(),
                    Month = dateTime.Month.ToString(),
                    Day = dateTime.Day.ToString()
                };
                datas.Add(model);
            }
            await _context.SaveChangesAsync();
            return datas;
        }

        [Route("PotentialTrainResults/PrintSelected/{isFillBlank}/{dateTime}/{idList?}")]
        public async Task<IActionResult> PrintSelected(string idList, bool isFillBlank = false, DateTime? dateTime = null)
        {
            try
            {
                if (string.IsNullOrEmpty(idList))
                    idList = HttpContext.Session.GetString(printSessionKey);
                Guid[] ids = idList.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => Guid.Parse(s)).ToArray();
                var stream = await PrintPdf(ids, isFillBlank, false, dateTime);
                FileStreamResult fileStreamResult = File(stream, "application/pdf");
                return fileStreamResult;
            }
            catch (PartyMemberException ex)
            {
                return View("PrintError", ex);
            }
            catch (Exception ex)
            {
                return View("PrintError", ex);
            }
        }
        [Route("PotentialTrainResults/PreviewSelected/{isFillBlank}/{dateTime}/{idList?}")]
        public async Task<IActionResult> PreviewSelected(string idList, bool isFillBlank = false, DateTime? dateTime = null)
        {
            try
            {
                if (string.IsNullOrEmpty(idList))
                    idList = HttpContext.Session.GetString(printSessionKey);
                Guid[] ids = idList.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => Guid.Parse(s)).ToArray();
                var stream = await PrintPdf(ids, isFillBlank, true, dateTime);
                FileStreamResult fileStreamResult = File(stream, "application/pdf");
                return fileStreamResult;
            }
            catch (PartyMemberException ex)
            {
                return View("PrintError", ex);
            }
            catch (Exception ex)
            {
                return View("PrintError", ex);
            }
        }

        /// <summary>
        /// 打印PDF格式
        /// </summary>
        /// <param name="ids"></param>
        /// <param name="isFillBlank">如果套打，则只打印空</param>
        /// <returns></returns>
        public async Task<Stream> PrintPdf(Guid[] ids, bool isFillBlank = false, bool isPreview = false, DateTime? dateTime = null)
        {
            if (!dateTime.HasValue)
                dateTime = DateTime.Today;
            List<PotentialMemberPrintViewModel> potentialMemberPrintViewModels = await GetReportDatas(ids, dateTime.Value);
            if (potentialMemberPrintViewModels.Count == 0)
                throw new PartyMemberException("选择的所有发展对象成绩均不合格，无法打印");
            List<PdfData> pdfDatas = new List<PdfData>();
            foreach (PotentialMemberPrintViewModel potentialMemberPrintViewModel in potentialMemberPrintViewModels)
            {
                if (isFillBlank)
                {
                    var data = new PdfData
                    {
                        //A4 new XSize(595, 842);
                        PageSize = new System.Drawing.Size(210, 210),
                        DocumentTitle = "发展对象培训结业证",
                        DocumentName = "发展对象培训结业证",
                        CreatedBy = "预备党员管理系统",
                        Description = "预备党员管理系统",
                        BackgroundImage = isFillBlank ? "PotentialTrain.png" : "PotentialTrainEmpty.png",
                        Rotate = 90,
                        DisplayItems = new List<DisplayItem>
                {
                    new DisplayItem{
                        Text=potentialMemberPrintViewModel.No,
                        Font="楷体",
                        FontSize=10,
                        Location=new System.Drawing.PointF(82.5f,20f)
                    },
                    new DisplayItem{
                        Text=potentialMemberPrintViewModel.Name,
                        Font="楷体",
                        FontSize=18,
                        Location=new System.Drawing.PointF(30,93.5f)
                    },
                    new DisplayItem{
                        Text=potentialMemberPrintViewModel.StartYear,
                        Font="楷体",
                        FontSize=18,
                        Location=new System.Drawing.PointF(60,108.5f)
                    },
                    new DisplayItem{
                        Text=potentialMemberPrintViewModel.EndYear,
                        Font="楷体",
                        FontSize=18,
                        Location=new System.Drawing.PointF(90,108.5f)
                    },
                    new DisplayItem{
                        Text=potentialMemberPrintViewModel.Term,
                        Font="楷体",
                        FontSize=18,
                        Location=new System.Drawing.PointF(40,123)
                    },
                    new DisplayItem{
                        Text=potentialMemberPrintViewModel.Year,
                        Font="楷体",
                        FontSize=16,
                        Location=new System.Drawing.PointF(55,173)
                    },
                    new DisplayItem{
                        Text=potentialMemberPrintViewModel.Month,
                        Font="楷体",
                        FontSize=16,
                        Location=new System.Drawing.PointF(74,173)
                    },
                    new DisplayItem{
                        Text=potentialMemberPrintViewModel.Day,
                        Font="楷体",
                        FontSize=16,
                        Location=new System.Drawing.PointF(84,173)
                    }
                }

                    };
                    pdfDatas.Add(data);
                    if (isPreview)
                    {
                        data.PageSize = new System.Drawing.Size(143, 210);
                        data.Rotate = 0;
                    }
                }
                else
                {
                    //打印全部文字（不打印背景图片）
                    string name = potentialMemberPrintViewModel.Name;
                    if (name.Length < 3)
                        name = " " + name + " ";
                    var data = new PdfData
                    {
                        //A4 new XSize(595, 842);
                        PageSize = new System.Drawing.Size(210, 210),
                        DocumentTitle = "发展对象培训结业证",
                        DocumentName = "发展对象培训结业证",
                        CreatedBy = "预备党员管理系统",
                        Description = "预备党员管理系统",
                        Rotate = 90,
                        BackgroundImage = isFillBlank ? "PotentialTrain.png" : "PotentialTrainEmpty.png",
                        DisplayItems = new List<DisplayItem>
                {
                    new DisplayItem{
                        Text=$@"党校证字 {potentialMemberPrintViewModel.No} 号",
                        Font="楷体",
                        FontSize=10.6f,
                        Location=new System.Drawing.PointF(64,20)
                    },
                    new DisplayItem{
                        Text=$@"{name} 同志：",
                        Font="黑体",
                        FontSize=16.8f,
                        Location=new System.Drawing.PointF(27,95)
                    },
                    new DisplayItem{
                        Text=$@"参加了  {potentialMemberPrintViewModel.StartYear} 至  {potentialMemberPrintViewModel.EndYear} 学年",
                        Font="黑体",
                        FontSize=16.8f,
                        Location=new System.Drawing.PointF(37,109)
                    },
                    new DisplayItem{
                        Text=$@"第  {potentialMemberPrintViewModel.Term}   期发展对象培训班学习，",
                        Font="黑体",
                        FontSize=16.8f,
                        Location=new System.Drawing.PointF(25.2f,124)
                    },
                    new DisplayItem{
                        Text=$@"培训考核成绩合格，准予结业。",
                        Font="黑体",
                        FontSize=16.8f,
                        Location=new System.Drawing.PointF(25.2f,138)
                    },
                    new DisplayItem{
                        Text=$@"中共兰州财经大学委员会党校",
                        Font="楷体",
                        FontSize=16.4f,
                        Location=new System.Drawing.PointF(43,164.5f)
                    },
                    new DisplayItem{
                        Text=$@"{potentialMemberPrintViewModel.Year}年{potentialMemberPrintViewModel.Month}月{potentialMemberPrintViewModel.Day}日",
                        Font="楷体",
                        FontSize=17.3f,
                        Location=new System.Drawing.PointF(55,173)
                    }
                }

                    };
                    pdfDatas.Add(data);
                    if (isPreview)
                    {
                        data.PageSize = new System.Drawing.Size(143, 210);
                        data.Rotate = 0;
                    }

                }
            }
            var stream = _pdfService.CreatePdf(pdfDatas, isPreview);
            return stream;
        }

        /// <summary>
        /// 导入发展对象培训成绩
        /// </summary>
        /// <returns></returns>
        public IActionResult Import()
        {
            PotentialTrainResultImportViewModel model = new PotentialTrainResultImportViewModel
            {
                DepartmentId = CurrentUser.DepartmentId.HasValue ? CurrentUser.DepartmentId.Value : Guid.Empty,
            };
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "41").Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value).OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "41").OrderBy(d => d.Ordinal), "Id", "Name");
            //if (CurrentUser.Roles == Role.学院党委)
            //    ViewBag.TrainClassTypeId = new SelectList(_context.TrainClassTypes.Where(d => d.Code.StartsWith("4")).OrderByDescending(d => d.Code), "Id", "Name");
            //else
            //    ViewBag.TrainClassTypeId = new SelectList(_context.TrainClassTypes.OrderBy(d => d.Code), "Id", "Name");
            TrainClassType trainClassType = _context.TrainClassTypes.Where(t => t.Code == "42").FirstOrDefault();
            if (trainClassType != null)
                ViewBag.TrainClassTypeId = trainClassType.Id;
            else
                ViewBag.TrainClassTypeId = Guid.Empty;
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderByDescending(d => d.StartYear).ThenByDescending(d => d.Term), "Id", "Name");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Import(PotentialTrainResultImportViewModel model)
        {
            JsonResultImport jsonResult = new JsonResultImport
            {
                Code = 0,
                Message = "数据导入成功"
            };
            try
            {
                if (ModelState.IsValid)
                {
                    IFormFile file = model.File;
                    if (file != null)
                    {
                        TrainClass trainClass = await _context.TrainClasses.FindAsync(model.TrainClassId);
                        Stream stream = file.OpenReadStream();
                        var filePath = Path.GetTempFileName();
                        var fileStream = System.IO.File.Create(filePath);
                        await file.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                        fileStream.Close();
                        #region 导入成绩
                        DataTable table = OfficeHelper.ReadExcelToDataTable(filePath);
                        DataTable tableErrorData = table.Clone();
                        DataColumn columnErrorMessage = null;
                        if (!tableErrorData.Columns.Contains("错误提示"))
                            columnErrorMessage = tableErrorData.Columns.Add("错误提示", typeof(string));
                        else
                            columnErrorMessage = tableErrorData.Columns["错误提示"];
                        int rowIndex = 0;
                        int successCount = 0;
                        string fieldsTeacher = "工号/学号,平时成绩,实践成绩,考试成绩";
                        string[] fieldListTeacher = fieldsTeacher.Split(',');
                        foreach (string field in fieldListTeacher)
                        {
                            if (!table.Columns.Contains(field))
                                throw new PartyMemberException($"缺少【{field}】列");
                        }
                        foreach (DataRow row in table.Rows)
                        {
                            rowIndex++;
                            try
                            {
                                string empNoField = "工号/学号";
                                string psScoreField = "平时成绩";
                                string sjScoreField = "实践成绩";
                                string csScoreField = "考试成绩";
                                string psScore = row[psScoreField].ToString();
                                string sjScore = row[sjScoreField].ToString();
                                string csScore = row[csScoreField].ToString();
                                string empNo = row[empNoField].ToString();
                                //跳过工号/学号为空的记录
                                if (string.IsNullOrEmpty(empNoField)) continue;
                                decimal psScoreValue = 0;
                                decimal sjScoreValue = 0;
                                decimal csScoreValue = 0;
                                if (!decimal.TryParse(psScore, out psScoreValue))
                                {
                                    throw new PartyMemberException($"第{rowIndex}行数据中的【{psScoreField}】数据不合法");
                                }
                                if (!decimal.TryParse(sjScore, out sjScoreValue))
                                {
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{sjScoreField}】数据不合法");
                                }
                                if (!decimal.TryParse(csScore, out csScoreValue))
                                {
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{csScoreField}】数据不合法");
                                }
                                if (psScoreValue < 0 || psScoreValue > 100)
                                {
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{psScoreField}】数据不合法");
                                }
                                if (sjScoreValue < 0 || sjScoreValue > 100)
                                {
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{sjScoreField}】数据不合法");
                                }
                                if (csScoreValue < 0 || csScoreValue > 100)
                                {
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{csScoreField}】数据不合法");
                                }
                                PotentialMember potentialMember = await _context.PotentialMembers.Where(p => p.TrainClassId == model.TrainClassId && p.JobNo == empNo).FirstOrDefaultAsync();
                                if (potentialMember == null)
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{empNo}】未找到，请核对工号/学号是否正确");
                                PotentialTrainResult potentialTrainResult = await _context.PotentialTrainResults.Where(p => p.PotentialMemberId == potentialMember.Id).FirstOrDefaultAsync();
                                var psProp = trainClass.PsGradeProportion;
                                var sjProp = trainClass.SjGradeProportion;
                                var csProp = trainClass.CsGradeProportion;
                                if (potentialTrainResult == null)
                                {
                                    potentialTrainResult = new PotentialTrainResult
                                    {
                                        Id = Guid.NewGuid(),
                                        PotentialMemberId = potentialMember.Id,
                                        CreateTime = DateTime.Now,
                                        OperatorId = CurrentUser.Id,
                                        IsDeleted = false,
                                        Ordinal = _context.ActivistTrainResults.Count() + 1,
                                        PsGrade = psScoreValue,
                                        SjGrade = sjScoreValue,
                                        CsGrade = csScoreValue,
                                        TotalGrade = Math.Round(psProp * psScoreValue / 100 + sjProp * sjScoreValue / 100 + csProp * csScoreValue / 100, 2)
                                    };
                                    _context.PotentialTrainResults.Add(potentialTrainResult);
                                }
                                else
                                {
                                    potentialTrainResult.PsGrade = psScoreValue;
                                    potentialTrainResult.CsGrade = csScoreValue;
                                    potentialTrainResult.TotalGrade = Math.Round(psProp * psScoreValue / 100 + csProp * csScoreValue / 100, 2);
                                }
                                if (potentialTrainResult.TotalGrade >= 60)
                                    potentialTrainResult.IsPass = true;
                                else
                                    potentialTrainResult.IsPass = false;
                                successCount++;
                                await _context.SaveChangesAsync();
                            }
                            catch (ImportDataErrorException ex)
                            {
                                //捕获到数据错误时，继续导入，将错误信息反馈给用户
                                DataRow rowErrorData = tableErrorData.NewRow();
                                foreach (DataColumn column in table.Columns)
                                    rowErrorData[column.ColumnName] = row[column.ColumnName];
                                rowErrorData[columnErrorMessage] = ex.Message;
                                tableErrorData.Rows.Add(rowErrorData);
                            }

                        }
                        #endregion
                        if (tableErrorData.Rows.Count > 0)
                        {
                            string basePath = GetErrorImportDataFilePath();
                            string fileName = $"发展对象成绩错误数据_{CurrentUser.LoginName}.xlsx";
                            string fileWithPath = $"{basePath}{Path.DirectorySeparatorChar}{fileName}";
                            Stream streamOutExcel = OfficeHelper.ExportExcelByOpenXml(tableErrorData);
                            FileStream outExcelFile = new FileStream(fileWithPath, FileMode.Create, System.IO.FileAccess.Write);
                            byte[] bytes = new byte[streamOutExcel.Length];
                            streamOutExcel.Read(bytes, 0, (int)streamOutExcel.Length);
                            outExcelFile.Write(bytes, 0, bytes.Length);
                            outExcelFile.Close();
                            streamOutExcel.Close();
                            jsonResult.ErrorDataFile = $"PotentialTrainResults/GetErrorImportData?fileName={fileName}";
                            jsonResult.FailCount = tableErrorData.Rows.Count;
                            jsonResult.SuccessCount = successCount;
                            jsonResult.Code = -2;
                            jsonResult.Message = "部分数据导入错误";
                        }
                    }
                    else
                    {
                        jsonResult.Code = -1;
                        jsonResult.Message = "请选择文件";
                    }
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
                jsonResult.Message = "发展对象成绩导入错误";
            }
            catch (PartyMemberException ex)
            {
                jsonResult.Code = -1;
                jsonResult.Message = ex.Message;
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, ex.Message);
                jsonResult.Code = -1;
                jsonResult.Message = "JSON文件内容格式错误";
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
        /// 返回导入错误文件的存放路径
        /// </summary>
        /// <returns></returns>
        private static string GetErrorImportDataFilePath()
        {
            string basePath = AppContext.BaseDirectory;
            if (!basePath.EndsWith(Path.DirectorySeparatorChar))
                basePath += Path.DirectorySeparatorChar;
            basePath += $"ErrorImportData";
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
            return basePath;
        }
        /// <summary>
        /// 返回错误导入数据
        /// </summary>
        /// <returns></returns>
        public IActionResult GetErrorImportData(string fileName)
        {
            string basePath = GetErrorImportDataFilePath();
            string fileWithPath = $"{basePath}{Path.DirectorySeparatorChar}{fileName}";
            FileStream outExcelFile = new FileStream(fileWithPath, FileMode.Open);
            return File(outExcelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "发展对象成绩导入失败数据.xlsx");
        }
        /// <summary>
        /// 导出所有学生数据
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Export(Guid? yearTermId, Guid? departmentId, Guid? trainClassId, string partyMemberType, string keyword)
        {
            try
            {
                string fileName = "入党积极分子导出名单.xlsx";
                //string fieldsStudent = "姓名,学号,身份证号,性别,出生年月,民族,所在学院,所在班级,联系电话,提交入党申请时间,担任职务,确定入党积极分子时间,备注";
                List<PartyActivist> partyActivists = null;
                var filter = PredicateBuilder.True<PartyActivist>();
                if (yearTermId != null)
                {
                    filter = filter.And(d => d.YearTermId == yearTermId);
                }
                if (departmentId != null)
                {
                    filter = filter.And(d => d.DepartmentId == departmentId);
                }
                if (trainClassId != null)
                {
                    filter = filter.And(d => d.TrainClassId == trainClassId);
                }
                if (keyword != null)
                {
                    filter = filter.And(d => d.Name.Contains(keyword) || d.JobNo.Contains(keyword));
                }
                if (partyMemberType != null)
                {
                    filter = filter.And(d => d.PartyMemberType == (PartyMemberType)Enum.Parse(typeof(PartyMemberType), partyMemberType));
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                    partyActivists = await _context.Set<PartyActivist>().Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.YearTerm)
                        .Where(filter)
                        .ToListAsync();
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    partyActivists = await _context.Set<PartyActivist>().Where(filter).Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.TrainClass).Include(d => d.YearTerm)
                        .Where(filter).Where(d => d.YearTerm.Enabled == true)
                        .Where(d => d.DepartmentId == CurrentUser.DepartmentId)
                        .ToListAsync();
                }
                DataTable table = new DataTable();
                table.Columns.Add("学年学期", typeof(string));
                table.Columns.Add("培训班", typeof(string));
                table.Columns.Add("类型", typeof(string));
                table.Columns.Add("学号", typeof(string));
                table.Columns.Add("姓名", typeof(string));
                table.Columns.Add("身份证号", typeof(string));
                table.Columns.Add("性别", typeof(string));
                table.Columns.Add("出生年月", typeof(string));
                table.Columns.Add("民族", typeof(string));
                table.Columns.Add("所在学院", typeof(string));
                table.Columns.Add("所在班级", typeof(string));
                table.Columns.Add("联系电话", typeof(string));
                table.Columns.Add("提交入党申请时间", typeof(string));
                table.Columns.Add("担任职务", typeof(string));
                table.Columns.Add("确定入党积极分子时间", typeof(string));
                table.Columns.Add("备注", typeof(string));
                table.Columns.Add("平时成绩", typeof(string));
                table.Columns.Add("实践成绩", typeof(string));
                table.Columns.Add("考试成绩", typeof(string));
                table.Columns.Add("补考成绩", typeof(string));
                table.Columns.Add("总评成绩", typeof(string));
                foreach (PartyActivist partyActivist in partyActivists)
                {
                    ActivistTrainResult activistTrainResult = await _context.ActivistTrainResults.FirstOrDefaultAsync(a => a.PartyActivistId == partyActivist.Id);
                    DataRow row = table.NewRow();
                    row["学年学期"] = partyActivist.YearTermDisplay;
                    row["培训班"] = partyActivist.TrainClassDisplay;
                    row["类型"] = partyActivist.PartyMemberTypeDisplay;
                    row["学号"] = partyActivist.JobNo;
                    row["姓名"] = partyActivist.Name;
                    row["身份证号"] = partyActivist.IdNumber;
                    row["性别"] = partyActivist.Sex;
                    row["出生年月"] = string.Format("{0:yyyy-MM}", partyActivist.BirthDate);
                    row["民族"] = partyActivist.NationDisplay;
                    row["所在学院"] = partyActivist.DepartmentDisplay;
                    row["所在班级"] = partyActivist.Class;
                    row["联系电话"] = partyActivist.Phone;
                    row["提交入党申请时间"] = string.Format("{0:yyyy-MM-dd}", partyActivist.ApplicationTime);
                    row["担任职务"] = partyActivist.Duty;
                    row["确定入党积极分子时间"] = string.Format("{0:yyyy-MM-dd}", partyActivist.ActiveApplicationTime);
                    row["备注"] = "";
                    row["平时成绩"] = string.Format("{0:#.#}", activistTrainResult.PsGrade);
                    row["实践成绩"] = string.Format("{0:#.#}", activistTrainResult.SjGrade);
                    row["考试成绩"] = string.Format("{0:#.#}", activistTrainResult.CsGrade);
                    row["补考成绩"] = string.Format("{0:#.#}", activistTrainResult.BcGrade);
                    row["总评成绩"] = string.Format("{0:#.#}", activistTrainResult.TotalGrade);
                    table.Rows.Add(row);
                }
                Stream datas = OfficeHelper.ExportExcelByOpenXml(table);
                return File(datas, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return View("ShowErrorMessage", ex);
            }
        }


        /// <summary>
        /// 打印和预览
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        public IActionResult PrintAndPreview(string idList, bool fillBlank = false, DateTime? dateTime = null)
        {
            //为了解决idList超过260各字符，get请求会报错，采用session解决
            HttpContext.Session.SetString(printSessionKey, idList);
            PrintAndPrevieViewModel printAndPrevieViewModel = new PrintAndPrevieViewModel
            {
                IsFillBlank = fillBlank,
                DateTime = string.Format("{0:yyyy-MM-dd}", dateTime),
                PrintIdList = idList
            };
            return View(printAndPrevieViewModel);
        }
        /// <summary>
        /// 更新打印状态
        /// </summary>
        /// <param name="idList"></param>
        /// <returns></returns>
        public async Task<IActionResult> UpdatePrintStatus(string idList)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "打印状态更新成功"
            };
            try
            {
                Guid[] ids = idList.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => Guid.Parse(s)).ToArray();
                foreach (Guid id in ids)
                {
                    PotentialTrainResult potentialTrainResult = await _context.PotentialTrainResults.FindAsync(id);
                    PotentialMember potentialMember = await _context.PotentialMembers.FindAsync(potentialTrainResult.PotentialMemberId);
                    //如果成绩和补考成绩均不合格，不能打印，也不生成证书编号
                    if (!potentialTrainResult.IsPass)
                        continue;
                    if (potentialMember.IsPrint)
                        continue;
                    //现在只有入党积极分子表中有isPrint
                    potentialMember.IsPrint = true;
                    potentialMember.PrintTime = DateTime.Now;
                    await _context.SaveChangesAsync();
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, ex.Message);
                jsonResult.Code = -1;
                jsonResult.Message = "更新打印状态时发生错误";
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
        /// 重置打印数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("ResetPrint")]
        public virtual async Task<IActionResult> ResetPrint(Guid? id)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "重置打印成功"
            };

            try
            {
                if (id == null)
                    throw new PartyMemberException("未传入重置项目的Id");
                var data = await _context.Set<PotentialTrainResult>().Include(d => d.PotentialMember).SingleOrDefaultAsync(m => m.Id == id);
                if (data == null)
                    throw new PartyMemberException("未找到要重置打印的数据");
                data.PotentialMember.IsPrint = false;
                _context.Set<PotentialMember>().Update(data.PotentialMember);
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
        private bool PotentialTrainResultExists(Guid id)
        {
            return _context.PotentialTrainResults.Any(e => e.Id == id);
        }
    }
}
