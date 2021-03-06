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
using PartyMemberManager.Core.Enums;
using NPOI.OpenXmlFormats.Spreadsheet;
using PartyMemberManager.Models.PrintViewModel;
using Microsoft.AspNetCore.Authorization;
using FastReport;
using FastReport.Web;
using PartyMemberManager.Models;
using System.IO;
using System.Data;
using ExcelCore;
using Newtonsoft.Json;
using AspNetCorePdf.PdfProvider.DataModel;
using AspNetCorePdf.PdfProvider;
using Microsoft.VisualBasic;
using PartyMemberManager.PdfProvider.DataModel;

namespace PartyMemberManager.Controllers
{
    public class ActivistTrainResultsController : PartyMemberDataControllerBase<ActivistTrainResult>
    {
        private readonly IPdfSharpService _pdfService;
        private readonly IMigraDocService _migraDocService;
        private readonly string printSessionKey = null;

        public ActivistTrainResultsController(ILogger<ActivistTrainResultsController> logger, PMContext context, IHttpContextAccessor accessor, IPdfSharpService pdfService, IMigraDocService migraDocService) : base(logger, context, accessor)
        {
            _pdfService = pdfService;
            _migraDocService = migraDocService;
            printSessionKey = $"ActivisistTrainResultPrint_{_accessor.HttpContext.Connection.RemoteIpAddress.ToString()}";
        }

        // GET: ActivistTrainResults
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.ActivistTrainResults.Include(a => a.PartyActivist).Include(d => d.PartyActivist.TrainClass);
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClassId = new SelectList(_context.TrainClasses.Include(t => t.YearTerm).Include(d => d.TrainClassType)
                    .Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value && d.YearTerm.Enabled == true)
                    .Where(d => d.TrainClassType.Code == "41")
                    .OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClassId = new SelectList(_context.TrainClasses.Include(t => t.YearTerm).Include(d => d.TrainClassType)
                    .Where(d => d.YearTerm.Enabled == true)
                    .Where(d => d.TrainClassType.Code == "41")
                    .OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderByDescending(d => d.StartYear).ThenByDescending(d => d.Term).Where(d => d.Enabled == true), "Id", "Name");
            ViewBag.TrainClassTypeId = _context.TrainClassTypes.Where(d => d.Code == "41").Select(d => d.Id).SingleOrDefault();
            return View(await pMContext.ToListAsync());
        }

        public async Task<IActionResult> GetDatasWithFilter(Guid? yearTermId, Guid? departmentId, string isPass, string isPrint, Guid? trainClassId, string isBcGrade, string keyword,BatchType batch, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<ActivistTrainResult> jsonResult = new JsonResultDatasModel<ActivistTrainResult>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<ActivistTrainResult>();
                if (!string.IsNullOrEmpty(keyword))
                {
                    filter = filter.And(d => d.PartyActivist.Name.Contains(keyword) || d.PartyActivist.JobNo.Contains(keyword));
                }
                if (departmentId != null)
                {
                    filter = filter.And(d => d.PartyActivist.DepartmentId == departmentId);
                }
                if (yearTermId != null)
                {
                    filter = filter.And(d => d.PartyActivist.YearTermId == yearTermId);
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
                    filter = filter.And(d => d.PartyActivist.IsPrint == (isPrint == "true"));
                }
                if (trainClassId != null)
                {
                    filter = filter.And(d => d.PartyActivist.TrainClassId == trainClassId);
                }
                if ((int)batch > 0)
                {
                    filter = filter.And(d => d.PartyActivist.TrainClass.Batch == batch);
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                    var data = await _context.Set<ActivistTrainResult>().Include(d => d.PartyActivist).Include(d => d.PartyActivist.TrainClass).Include(d => d.PartyActivist.YearTerm)
                        .Where(filter)
                        //.Where(d => d.PartyActivist.YearTerm.Enabled == true)
                        .OrderByDescending(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    SetStatus(data);
                    jsonResult.Count = _context.Set<ActivistTrainResult>().Where(filter).Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<ActivistTrainResult>().Include(d => d.PartyActivist).Include(d => d.PartyActivist.TrainClass).Include(d => d.PartyActivist.YearTerm)
                        .Where(filter).Where(d => d.PartyActivist.YearTerm.Enabled == true)
                        .Where(d => d.PartyActivist.DepartmentId == CurrentUser.DepartmentId).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    SetStatus(data);
                    jsonResult.Count = _context.Set<ActivistTrainResult>().Include(d => d.PartyActivist).Include(d => d.PartyActivist.TrainClass).Include(d => d.PartyActivist.YearTerm)
                        .Where(filter).Where(d => d.PartyActivist.YearTerm.Enabled == true)
                        .Where(d => d.PartyActivist.DepartmentId == CurrentUser.DepartmentId).Count();
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
        /// <summary>
        /// 设置积极分子状态
        /// </summary>
        /// <param name="data"></param>
        private void SetStatus(PagedDataViewModel<ActivistTrainResult> data)
        {
            //增加状态显示
            foreach (var dataDetail in data)
            {
                if (_context.PotentialMembers.Any(p => p.PartyActivistId == dataDetail.PartyActivistId))
                    dataDetail.Status = ActivistTrainStatus.成绩合格并列为发展对象;
                else if (dataDetail.IsPass)
                    dataDetail.Status = ActivistTrainStatus.成绩合格;
                else if (dataDetail.IsPrint)
                    dataDetail.Status = ActivistTrainStatus.成绩合格并打印;
                else
                    dataDetail.Status = ActivistTrainStatus.成绩不合格;
            }
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
            return View(activistTrainResult);
        }
        // GET: ActivistTrainResults/Edit/5
        public async Task<IActionResult> BcGrade(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var activistTrainResult = await _context.ActivistTrainResults.Include(d => d.PartyActivist).Include(d => d.PartyActivist.TrainClass).Include(d => d.PartyActivist.TrainClass.YearTerm).SingleOrDefaultAsync(m => m.Id == id);
            if (activistTrainResult == null)
            {
                return NotFoundData();
            }
            //ViewData["PartyActivistId"] = new SelectList(_context.PartyActivists, "Id", "ActiveApplicationTime", activistTrainResult.PartyActivistId);
            return View(activistTrainResult);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BcGrade([Bind("Id,PartyActivistId,BcGrade")] ActivistTrainResult activistTrainResult)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据保存成功"
            };
            try
            {

                ActivistTrainResult activistTrainResultInDb = await _context.ActivistTrainResults.Include(d => d.PartyActivist.TrainClass).Where(d => d.Id == activistTrainResult.Id).FirstOrDefaultAsync();
                PartyActivist partyActivist = await _context.PartyActivists.FindAsync(activistTrainResult.PartyActivistId);
                var psProp = activistTrainResultInDb.PartyActivist.TrainClass.PsGradeProportion;
                var csProp = activistTrainResultInDb.PartyActivist.TrainClass.CsGradeProportion;
                if (activistTrainResultInDb == null)
                    throw new PartyMemberException("未找到数据");
                if (!activistTrainResult.BcGrade.HasValue)
                    throw new PartyMemberException("请输入补考成绩");
                decimal bcGrade = activistTrainResult.BcGrade.Value;
                decimal psGrade = activistTrainResultInDb.PsGrade.Value;
                if (bcGrade > 100 || bcGrade < 0)
                    throw new PartyMemberException($"【{partyActivist.JobNo}-{partyActivist.Name}】的补考成绩非法");
                activistTrainResultInDb.BcGrade = bcGrade;
                activistTrainResultInDb.TotalGrade = Math.Round(psProp * psGrade / 100 + csProp * bcGrade / 100, 2);
                if (activistTrainResultInDb.TotalGrade >= 60)
                    activistTrainResultInDb.IsPass = true;
                else
                    activistTrainResultInDb.IsPass = false;
                _context.Update(activistTrainResultInDb);
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
        public override async Task<IActionResult> Save([Bind("PartyActivistId,PsGrade,CsGrade,TotalGrade,IsPass,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] ActivistTrainResult activistTrainResult)
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
                        activistTrainResultInDb.PsGrade = activistTrainResult.PsGrade;
                        activistTrainResultInDb.CsGrade = activistTrainResult.CsGrade;
                        activistTrainResultInDb.TotalGrade = activistTrainResult.TotalGrade;
                        activistTrainResultInDb.IsPass = activistTrainResult.IsPass;
                        //activistTrainResultInDb.IsPrint = activistTrainResult.IsPrint;
                        //activistTrainResultInDb.PrintTime = activistTrainResult.PrintTime;
                        //activistTrainResultInDb.Id = activistTrainResult.Id;
                        //activistTrainResultInDb.CreateTime = activistTrainResult.CreateTime;
                        //activistTrainResultInDb.OperatorId = activistTrainResult.OperatorId;
                        //activistTrainResultInDb.Ordinal = activistTrainResult.Ordinal;
                        //activistTrainResultInDb.IsDeleted = activistTrainResult.IsDeleted;
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
                        ActivistTrainResult activistTrainResult = await _context.ActivistTrainResults.Include(d => d.PartyActivist.TrainClass).Where(d => d.Id == id).FirstOrDefaultAsync();
                        PartyActivist partyActivist = await _context.PartyActivists.FindAsync(activistTrainResult.PartyActivistId);
                        var psProp = activistTrainResult.PartyActivist.TrainClass.PsGradeProportion;
                        var sjProp = activistTrainResult.PartyActivist.TrainClass.SjGradeProportion;
                        var csProp = activistTrainResult.PartyActivist.TrainClass.CsGradeProportion;
                        if (activistTrainResult != null && partyActivist.IsPrint == false)
                        {
                            decimal psGrade = 0;
                            decimal sjGrade = 0;
                            decimal csGrade = 0;
                            if (decimal.TryParse(subItem[1], out psGrade))
                                activistTrainResult.PsGrade = psGrade;
                            if (decimal.TryParse(subItem[2], out sjGrade))
                                activistTrainResult.SjGrade = sjGrade;
                            if (decimal.TryParse(subItem[3], out csGrade))
                                activistTrainResult.CsGrade = csGrade;
                            if (psGrade > 100 || psGrade < 0)
                                throw new PartyMemberException($"【{partyActivist.JobNo}-{partyActivist.Name}】的平时成绩非法");
                            if (sjGrade > 100 || sjGrade < 0)
                                throw new PartyMemberException($"【{partyActivist.JobNo}-{partyActivist.Name}】的实践成绩非法");
                            if (csGrade > 100 || csGrade < 0)
                                throw new PartyMemberException($"【{partyActivist.JobNo}-{partyActivist.Name}】的考试成绩非法");
                            activistTrainResult.TotalGrade = Math.Round(psProp * psGrade / 100 + sjProp * sjGrade / 100 + csProp * csGrade / 100, 2);
                            if (activistTrainResult.TotalGrade >= 60 && activistTrainResult.CsGrade>=55)
                                activistTrainResult.IsPass = true;
                            else
                                activistTrainResult.IsPass = false;
                            _context.Update(activistTrainResult);

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
        /// <summary>
        /// 获取入党积极分子结业证打印数据
        /// </summary>
        /// <param name="partyActivistId"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetPrintData(Guid id, DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                dateTime = DateTime.Today;
            PartyActivistPrintViewModel model = await GetReportData(id, dateTime.Value);
            return Json(model);
        }

        private async Task<PartyActivistPrintViewModel> GetReportData(Guid id, DateTime dateTime)
        {
            ActivistTrainResult activistTrainResult = await _context.ActivistTrainResults.FindAsync(id);
            PartyActivist partyActivist = await _context.PartyActivists.FindAsync(activistTrainResult.PartyActivistId);
            YearTerm yearTerm = await _context.YearTerms.FindAsync(partyActivist.YearTermId);
            TrainClass trainClass = await _context.TrainClasses.FindAsync(partyActivist.TrainClassId);
            Department department = await _context.Departments.FindAsync(trainClass.DepartmentId);
            TrainClassType trainClassType = await _context.TrainClassTypes.FindAsync(trainClass.TrainClassTypeId);
            //DateTime dateTime = DateTime.Today;
            //编号可能需要在录入成绩后生成，暂时生成1号结业证编号
            string no = activistTrainResult.CertificateNumber;
            if (string.IsNullOrEmpty(activistTrainResult.CertificateNumber))
            {
                ActivistTrainResult activistTrainResultLast = await _context.ActivistTrainResults
                    .Include(p => p.PartyActivist)
                    .Include(p => p.PartyActivist.TrainClass)
                    .Include(p => p.PartyActivist.TrainClass.YearTerm)
                    .Where(p => p.PartyActivist.TrainClass.TrainClassTypeId == trainClass.TrainClassTypeId
                     && p.PartyActivist.TrainClass.DepartmentId == department.Id
                     && p.PartyActivist.TrainClass.YearTerm.StartYear == yearTerm.StartYear
                    && p.CertificateOrder > 0).OrderByDescending(p => p.CertificateOrder).FirstOrDefaultAsync();
                int certificateOrder = 1;
                if (activistTrainResultLast != null)
                    certificateOrder = activistTrainResultLast.CertificateOrder.Value + 1;
                no = string.Format("{0:yyyy}{1:00}{2:00}{0:MM}{3:000}", trainClass.StartTime.Value, trainClassType.Code, department.Code, certificateOrder);
                //更新证书编号
                activistTrainResult.CertificateOrder = certificateOrder;
                activistTrainResult.CertificateNumber = no;
            }
            //no = string.Format("{0:yyyy}{1:00}{2:00}{0:MM}{3:000}", trainClass.StartTime.Value, trainClassType.Code, department.Code, 1);
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
            await _context.SaveChangesAsync();
            return model;
        }
        private async Task<List<PartyActivistPrintViewModel>> GetReportDatas(Guid[] ids, DateTime dateTime)
        {
            List<PartyActivistPrintViewModel> datas = new List<PartyActivistPrintViewModel>();
            foreach (Guid id in ids)
            {
                ActivistTrainResult activistTrainResult = await _context.ActivistTrainResults.FindAsync(id);
                //如果成绩和补考成绩均不合格，不能打印，也不生成证书编号
                if (!activistTrainResult.IsPass)
                    continue;
                PartyActivist partyActivist = await _context.PartyActivists.FindAsync(activistTrainResult.PartyActivistId);
                YearTerm yearTerm = await _context.YearTerms.FindAsync(partyActivist.YearTermId);
                TrainClass trainClass = await _context.TrainClasses.FindAsync(partyActivist.TrainClassId);
                Department department = await _context.Departments.FindAsync(trainClass.DepartmentId);
                TrainClassType trainClassType = await _context.TrainClassTypes.FindAsync(trainClass.TrainClassTypeId);
                //DateTime dateTime = DateTime.Today;
                //编号可能需要在录入成绩后生成，暂时生成1号结业证编号
                string no = null;
                if (string.IsNullOrEmpty(activistTrainResult.CertificateNumber))
                {
                    ActivistTrainResult activistTrainResultLast = await _context.ActivistTrainResults
                        .Include(p => p.PartyActivist)
                        .Include(p=>p.PartyActivist.TrainClass)
                        .Include(p=>p.PartyActivist.TrainClass.YearTerm)
                        .Where(p => p.PartyActivist.TrainClass.TrainClassTypeId==trainClass.TrainClassTypeId
                         && p.PartyActivist.TrainClass.DepartmentId == department.Id
                         && p.PartyActivist.TrainClass.YearTerm.StartYear == yearTerm.StartYear
                        && p.CertificateOrder > 0).OrderByDescending(p => p.CertificateOrder).FirstOrDefaultAsync();
                    int certificateOrder = 1;
                    if (activistTrainResultLast != null)
                        certificateOrder = activistTrainResultLast.CertificateOrder.Value + 1;
                    no = string.Format("{0:yyyy}{1:00}{2:00}{0:MM}{3:000}", trainClass.StartTime.Value, trainClassType.Code, department.Code, certificateOrder);
                    //更新证书编号
                    activistTrainResult.CertificateOrder = certificateOrder;
                    activistTrainResult.CertificateNumber = no;
                }
                else
                {
                    no = activistTrainResult.CertificateNumber;
                }
                //现在只有入党积极分子表中有isPrint
                //partyActivist.IsPrint = true;
                //partyActivist.PrintTime = DateTime.Now;
                await _context.SaveChangesAsync();
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
                datas.Add(model);
            }
            return datas;
        }

        [Route("ActivistTrainResults/PrintSelected/{isFillBlank}/{dateTime}/{idList?}")]
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

        [Route("ActivistTrainResults/PreviewSelected/{isFillBlank}/{dateTime}/{idList?}")]
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
        /// <param name="isFillBlank">//如果套打，则只打印空</param>
        /// <returns></returns>
        public async Task<Stream> PrintPdf(Guid[] ids, bool isFillBlank = false, bool isPreview = false, DateTime? dateTime = null)
        {
            if (!dateTime.HasValue)
                dateTime = DateTime.Today;
            List<PartyActivistPrintViewModel> partyActivistPrintViewModels = await GetReportDatas(ids, dateTime.Value);
            if (partyActivistPrintViewModels.Count == 0)
                throw new PartyMemberException("选择的所有发展对象成绩均不合格，无法打印");
            List<PdfData> pdfDatas = new List<PdfData>();
            foreach (PartyActivistPrintViewModel partyActivistPrintViewModel in partyActivistPrintViewModels)
            {
                if (isFillBlank)
                {
                    var data = new PdfData
                    {
                        //A4 new XSize(595, 842);
                        PageSize = new System.Drawing.Size(287, 210),
                        DocumentTitle = "入党积极分子培训结业证",
                        DocumentName = "入党积极分子培训结业证",
                        CreatedBy = "预备党员管理系统",
                        Description = "预备党员管理系统",
                        Rotate = 90,
                        BackgroundImage = isFillBlank ? "ActivistTrain.png" : "ActivistTrainEmpty.png",
                        DisplayItems = new List<DisplayItem>
                {
                    new DisplayItem{
                        Text=partyActivistPrintViewModel.No,
                        Font="楷体",
                        FontSize=15,
                        Location=new System.Drawing.PointF(220,22)
                    },
                    //new DisplayItem{
                    //    Text=partyActivistPrintViewModel.Name,
                    //    Font="楷体",
                    //    FontSize=27,
                    //    Location=new System.Drawing.PointF(35,92)
                    //},
                    new DisplayItem{
                        Text=partyActivistPrintViewModel.StartYear,
                        Font="楷体",
                        FontSize=27,
                        Location=new System.Drawing.PointF(116,92)
                    },
                    new DisplayItem{
                        Text=partyActivistPrintViewModel.EndYear,
                        Font="楷体",
                        FontSize=27,
                        Location=new System.Drawing.PointF(155,92)
                    },
                    new DisplayItem{
                        Text=partyActivistPrintViewModel.Term,
                        Font="楷体",
                        FontSize=27,
                        Location=new System.Drawing.PointF(214,92)
                    },
                    new DisplayItem{
                        Text=partyActivistPrintViewModel.Year,
                        Font="楷体",
                        FontSize=26,
                        Location=new System.Drawing.PointF(159,178)
                    },
                    new DisplayItem{
                        Text=partyActivistPrintViewModel.Month,
                        Font="楷体",
                        FontSize=26,
                        Location=new System.Drawing.PointF(190,178)
                    },
                    new DisplayItem{
                        Text=partyActivistPrintViewModel.Day,
                        Font="楷体",
                        FontSize=26,
                        Location=new System.Drawing.PointF(205,178)
                    }
                }

                    };

                    PrintName(partyActivistPrintViewModel, data);
                    if (isPreview)
                    {
                        data.Rotate = 0;
                    }

                    pdfDatas.Add(data);
                }
                else
                {
                    //打印全部文字（不打印背景图片）
                    string name = partyActivistPrintViewModel.Name;
                    if (name.Length < 3)
                        name = " " + name + " ";
                    var data = new PdfData
                    {
                        //A4 new XSize(595, 842);
                        PageSize = new System.Drawing.Size(287, 210),
                        DocumentTitle = "入党积极分子培训结业证",
                        DocumentName = "入党积极分子培训结业证",
                        CreatedBy = "预备党员管理系统",
                        Description = "预备党员管理系统",
                        Rotate = 90,
                        BackgroundImage = isFillBlank ? "ActivistTrain.png" : "ActivistTrainEmpty.png",
                        DisplayItems = new List<DisplayItem>
                {
                    new DisplayItem{
                        Text=$@"党校证字 {partyActivistPrintViewModel.No} 号",
                        Font="楷体",
                        FontSize=15,
                        Location=new System.Drawing.PointF(196,22)
                    },
                    new DisplayItem{
                        Text=$@"同志参加了 {partyActivistPrintViewModel.StartYear} 至 {partyActivistPrintViewModel.EndYear} 学年第 {partyActivistPrintViewModel.Term} 期入党",
                        Font="楷体",
                        FontSize=28f,
                        Location=new System.Drawing.PointF(61.5f,91)
                    },
                    new DisplayItem{
                        Text=$@"积极分子培训班学习，培训考核成绩合格，准予结业。",
                        Font="楷体",
                        FontSize=28f,
                        Location=new System.Drawing.PointF(28,117)
                    },
                    new DisplayItem{
                        Text=$@"党校校长：",
                        Font="隶书",
                        FontSize=30,
                        Location=new System.Drawing.PointF(75,140)
                    },
                    new DisplayItem{
                        Text=$@"中共兰州财经大学委员会党校",
                        Font="楷体",
                        FontSize=25.7f,
                        Location=new System.Drawing.PointF(142,163)
                    },
                    new DisplayItem{
                        Text=$@"{partyActivistPrintViewModel.Year}年{partyActivistPrintViewModel.Month}月{partyActivistPrintViewModel.Day}日",
                        Font="楷体",
                        FontSize=26,
                        Location=new System.Drawing.PointF(159,178)
                    }
                }

                    };
                    PrintName(partyActivistPrintViewModel, data);
                    if (isPreview)
                    {
                        data.Rotate = 0;
                    }
                    pdfDatas.Add(data);

                }
            }
            var stream = _pdfService.CreatePdf(pdfDatas, isPreview);
            //var stream = _migraDocService.CreateMigraDocPdf(pdfDatas);
            return stream;
        }
        /// <summary>
        /// 打印姓名，主要考虑姓名字数太多时如何打印
        /// </summary>
        /// <param name="partyActivistPrintViewModel"></param>
        /// <param name="data"></param>
        private static void PrintName(PartyActivistPrintViewModel partyActivistPrintViewModel, PdfData data)
        {
            string name = partyActivistPrintViewModel.Name;
            if (name.Length <= 30)
            {
                DisplayItem displayItem = null;
                switch (name.Length)
                {
                    case 2:
                        //打印姓名
                        displayItem = new DisplayItem
                        {
                            Text = partyActivistPrintViewModel.Name,
                            Font = "楷体",
                            FontSize = 28,
                            Location = new System.Drawing.PointF(35.5f, 92)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 3:
                        //6个字以内，往左移动
                        //打印姓名
                        displayItem = new DisplayItem
                        {
                            Text = partyActivistPrintViewModel.Name,
                            Font = "楷体",
                            FontSize = 28,
                            Location = new System.Drawing.PointF(30.5f, 92)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 4:
                        //6个字以内，往左移动
                        //打印姓名
                        displayItem = new DisplayItem
                        {
                            Text = partyActivistPrintViewModel.Name,
                            Font = "楷体",
                            FontSize = 25,
                            Location = new System.Drawing.PointF(26.5f, 92)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 5:
                        //6个字以内，往左移动
                        //打印姓名
                        displayItem = new DisplayItem
                        {
                            Text = partyActivistPrintViewModel.Name,
                            Font = "楷体",
                            FontSize = 23,
                            Location = new System.Drawing.PointF(20.5f, 93)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 6:
                        //6各字以上缩小字体
                        //打印姓名
                        displayItem = new DisplayItem
                        {
                            Text = partyActivistPrintViewModel.Name,
                            Font = "楷体",
                            FontSize = 20,
                            Location = new System.Drawing.PointF(20.5f, 93)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 7:
                        //打印姓名
                        string nameLine1 = name.Substring(0, 4);
                        string nameLine2 = name.Substring(4);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine1,
                            Font = "楷体",
                            FontSize = 25,
                            Location = new System.Drawing.PointF(26.5f, 88)
                        };
                        data.DisplayItems.Add(displayItem);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine2,
                            Font = "楷体",
                            FontSize = 25,
                            Location = new System.Drawing.PointF(26.5f, 96)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 8:
                        //打印姓名
                        string nameLine3 = name.Substring(0, 4);
                        string nameLine4 = name.Substring(4);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine3,
                            Font = "楷体",
                            FontSize = 25,
                            Location = new System.Drawing.PointF(26.5f, 88)
                        };
                        data.DisplayItems.Add(displayItem);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine4,
                            Font = "楷体",
                            FontSize = 25,
                            Location = new System.Drawing.PointF(26.5f, 96)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 9:
                        //打印姓名
                        string nameLine5 = name.Substring(0, 5);
                        string nameLine6 = name.Substring(5);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine5,
                            Font = "楷体",
                            FontSize = 23,
                            Location = new System.Drawing.PointF(20.5f, 88)
                        };
                        data.DisplayItems.Add(displayItem);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine6,
                            Font = "楷体",
                            FontSize = 23,
                            Location = new System.Drawing.PointF(20.5f, 96)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 10:
                        //打印姓名
                        string nameLine7 = name.Substring(0, 5);
                        string nameLine8 = name.Substring(5);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine7,
                            Font = "楷体",
                            FontSize = 23,
                            Location = new System.Drawing.PointF(20.5f, 88)
                        };
                        data.DisplayItems.Add(displayItem);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine8,
                            Font = "楷体",
                            FontSize = 23,
                            Location = new System.Drawing.PointF(20.5f, 96)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 11:
                        //打印姓名
                        string nameLine9 = name.Substring(0, 6);
                        string nameLine10 = name.Substring(6);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine9,
                            Font = "楷体",
                            FontSize = 20,
                            Location = new System.Drawing.PointF(20.5f, 88)
                        };
                        data.DisplayItems.Add(displayItem);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine10,
                            Font = "楷体",
                            FontSize = 20,
                            Location = new System.Drawing.PointF(20.5f, 96)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 12:
                        //打印姓名
                        string nameLine11 = name.Substring(0, 6);
                        string nameLine12 = name.Substring(6);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine11,
                            Font = "楷体",
                            FontSize = 20,
                            Location = new System.Drawing.PointF(20.5f, 88)
                        };
                        data.DisplayItems.Add(displayItem);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine12,
                            Font = "楷体",
                            FontSize = 20,
                            Location = new System.Drawing.PointF(20.5f, 96)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 13:
                    case 14:
                        //打印姓名
                        string nameLine13 = name.Substring(0, 6);
                        string nameLine14 = name.Substring(6);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine13,
                            Font = "楷体",
                            FontSize = 16,
                            Location = new System.Drawing.PointF(20.5f, 88)
                        };
                        data.DisplayItems.Add(displayItem);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine14,
                            Font = "楷体",
                            FontSize = 16,
                            Location = new System.Drawing.PointF(20.5f, 96)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    case 18:
                    case 19:
                    case 20:
                    case 21:
                    case 22:
                        //打印姓名
                        string nameLine15 = name.Substring(0, 6);
                        string nameLine16 = name.Substring(6, 6);
                        string nameLine17 = name.Substring(12);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine15,
                            Font = "楷体",
                            FontSize = 15,
                            Location = new System.Drawing.PointF(20.5f, 88)
                        };
                        data.DisplayItems.Add(displayItem);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine16,
                            Font = "楷体",
                            FontSize = 15,
                            Location = new System.Drawing.PointF(20.5f, 96)
                        };
                        data.DisplayItems.Add(displayItem);
                        displayItem = new DisplayItem
                        {
                            Text = nameLine17,
                            Font = "楷体",
                            FontSize = 15,
                            Location = new System.Drawing.PointF(20.5f, 104)
                        };
                        data.DisplayItems.Add(displayItem);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                //换成4行
            }
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
                if (string.IsNullOrEmpty(idList))
                    idList = HttpContext.Session.GetString(printSessionKey);
                Guid[] ids = idList.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => Guid.Parse(s)).ToArray();
                foreach (Guid id in ids)
                {
                    ActivistTrainResult activistTrainResult = await _context.ActivistTrainResults.FindAsync(id);
                    PartyActivist partyActivist = await _context.PartyActivists.FindAsync(activistTrainResult.PartyActivistId);
                    //如果成绩和补考成绩均不合格，不能打印，也不生成证书编号
                    if (!activistTrainResult.IsPass)
                        continue;
                    if (partyActivist.IsPrint)
                        continue;
                    //现在只有入党积极分子表中有isPrint
                    partyActivist.IsPrint = true;
                    partyActivist.PrintTime = DateTime.Now;
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
        /// 导入入党积极分子培训成绩
        /// </summary>
        /// <returns></returns>
        public IActionResult Import()
        {
            ActivistTrainResultImportViewModel model = new ActivistTrainResultImportViewModel
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
            TrainClassType trainClassType = _context.TrainClassTypes.Where(t => t.Code == "41").FirstOrDefault();
            if (trainClassType != null)
                ViewBag.TrainClassTypeId = trainClassType.Id;
            else
                ViewBag.TrainClassTypeId = Guid.Empty;
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderByDescending(d => d.StartYear).ThenByDescending(d => d.Term), "Id", "Name");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Import(ActivistTrainResultImportViewModel model)
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
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{psScoreField}】数据不合法");
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
                                PartyActivist partyActivist = await _context.PartyActivists.Where(p => p.TrainClassId == model.TrainClassId && p.JobNo == empNo).FirstOrDefaultAsync();
                                if (partyActivist == null)
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{empNo}】未找到，请核对工号/学号是否正确");
                                ActivistTrainResult activistTrainResult = await _context.ActivistTrainResults.Where(p => p.PartyActivistId == partyActivist.Id).FirstOrDefaultAsync();
                                var psProp = trainClass.PsGradeProportion;
                                var sjProp = trainClass.SjGradeProportion;
                                var csProp = trainClass.CsGradeProportion;
                                if (activistTrainResult == null)
                                {
                                    activistTrainResult = new ActivistTrainResult
                                    {
                                        Id = Guid.NewGuid(),
                                        PartyActivistId = partyActivist.Id,
                                        CreateTime = DateTime.Now,
                                        OperatorId = CurrentUser.Id,
                                        IsDeleted = false,
                                        Ordinal = _context.ActivistTrainResults.Count() + 1,
                                        PsGrade = psScoreValue,
                                        SjGrade = sjScoreValue,
                                        CsGrade = csScoreValue,
                                        TotalGrade = Math.Round(psProp * psScoreValue / 100 + sjProp * sjScoreValue / 100 + csProp * csScoreValue / 100, 2)
                                    };
                                    _context.ActivistTrainResults.Add(activistTrainResult);
                                }
                                else
                                {
                                    activistTrainResult.PsGrade = psScoreValue;
                                    activistTrainResult.CsGrade = csScoreValue;
                                    activistTrainResult.TotalGrade = Math.Round(psProp * psScoreValue / 100 + csProp * csScoreValue / 100, 2);
                                }
                                if (activistTrainResult.TotalGrade >= 60)
                                    activistTrainResult.IsPass = true;
                                else
                                    activistTrainResult.IsPass = false;
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
                            string fileName = $"入党积极分子成绩错误数据_{CurrentUser.LoginName}.xlsx";
                            string fileWithPath = $"{basePath}{Path.DirectorySeparatorChar}{fileName}";
                            Stream streamOutExcel = OfficeHelper.ExportExcelByOpenXml(tableErrorData);
                            FileStream outExcelFile = new FileStream(fileWithPath, FileMode.Create, System.IO.FileAccess.Write);
                            byte[] bytes = new byte[streamOutExcel.Length];
                            streamOutExcel.Read(bytes, 0, (int)streamOutExcel.Length);
                            outExcelFile.Write(bytes, 0, bytes.Length);
                            outExcelFile.Close();
                            streamOutExcel.Close();
                            jsonResult.ErrorDataFile = $"ActivistTrainResults/GetErrorImportData?fileName={fileName}";
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
                jsonResult.Message = "入党积极分子成绩导入错误";
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
            return File(outExcelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "入党积极分子成绩导入失败数据.xlsx");
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
                DateTime=string.Format("{0:yyyy-MM-dd}",dateTime),
                PrintIdList = idList
            };
            return View(printAndPrevieViewModel);
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
                var data = await _context.Set<ActivistTrainResult>().Include(d=>d.PartyActivist).SingleOrDefaultAsync(m => m.Id == id);
                if (data == null)
                    throw new PartyMemberException("未找到要重置打印的数据");
                data.PartyActivist.IsPrint = false;
                _context.Set<PartyActivist>().Update(data.PartyActivist);
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


        private bool ActivistTrainResultExists(Guid id)
        {
            return _context.ActivistTrainResults.Any(e => e.Id == id);
        }
    }
}
