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

        public async Task<IActionResult> GetDatasWithFilter(Guid? yearTermId, Guid? departmentId, string isPass, string isPrint, Guid? trainClassId, string keyword, int page = 1, int limit = 10)
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
                if (!string.IsNullOrEmpty(isPrint))
                {
                    filter = filter.And(d => d.IsPrint == (isPrint == "true"));
                }
                if (trainClassId != null)
                {
                    filter = filter.And(d => d.PartyActivist.TrainClassId == trainClassId);
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                    var data = await _context.Set<ActivistTrainResult>().Include(d => d.PartyActivist).Include(d => d.PartyActivist.TrainClass).Include(d => d.PartyActivist.YearTerm)
                        .Where(filter).Where(d => d.PartyActivist.YearTerm.Enabled == true)
                        .OrderByDescending(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<ActivistTrainResult>().Count();
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
                    jsonResult.Count = _context.Set<ActivistTrainResult>().Count();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("PartyActivistId,PsGrade,CsGrade,TotalGrade,IsPass,IsPrint,PrintTime,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] ActivistTrainResult activistTrainResult)
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
                    if (subItem.Length == 3)
                    {
                        Guid id = Guid.Parse(subItem[0]);
                        ActivistTrainResult activistTrainResult = await _context.ActivistTrainResults.Include(d => d.PartyActivist.TrainClass).Where(d => d.Id == id).FirstOrDefaultAsync();
                        PartyActivist partyActivist = await _context.PartyActivists.FindAsync(activistTrainResult.PartyActivistId);
                        var psProp = activistTrainResult.PartyActivist.TrainClass.PsGradeProportion;
                        var csProp = activistTrainResult.PartyActivist.TrainClass.CsGradeProportion;
                        if (activistTrainResult != null)
                        {
                            decimal psGrade = 0;
                            decimal csGrade = 0;
                            if (decimal.TryParse(subItem[1], out psGrade))
                                activistTrainResult.PsGrade = psGrade;
                            if (decimal.TryParse(subItem[2], out csGrade))
                                activistTrainResult.CsGrade = csGrade;
                            if (psGrade > 100 || psGrade < 0)
                                throw new PartyMemberException($"【{partyActivist.JobNo}-{partyActivist.Name}】的平时成绩非法");
                            if (csGrade > 100 || csGrade < 0)
                                throw new PartyMemberException($"【{partyActivist.JobNo}-{partyActivist.Name}】的考试成绩非法");
                            activistTrainResult.TotalGrade = Math.Round(psProp * psGrade / 100 + csProp * csGrade / 100, 2);
                            if (activistTrainResult.TotalGrade >= 60)
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
        public async Task<IActionResult> GetPrintData(Guid id)
        {
            PartyActivistPrintViewModel model = await GetReportData(id);
            return Json(model);
        }

        private async Task<PartyActivistPrintViewModel> GetReportData(Guid id)
        {
            ActivistTrainResult activistTrainResult = await _context.ActivistTrainResults.FindAsync(id);
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
            return model;
        }
        private async Task<List<PartyActivistPrintViewModel>> GetReportDatas(Guid[] ids)
        {
            List<PartyActivistPrintViewModel> datas = new List<PartyActivistPrintViewModel>();
            foreach (Guid id in ids)
            {
                ActivistTrainResult activistTrainResult = await _context.ActivistTrainResults.FindAsync(id);
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
                datas.Add(model);
            }

            return datas;
        }

        public async Task<IActionResult> Print(Guid id)
        {
            PartyActivistPrintViewModel model = await GetReportData(id);
            List<PartyActivistPrintViewModel> partyActivistPrintViewModels = new List<PartyActivistPrintViewModel>();
            partyActivistPrintViewModels.Add(model);
            string reportFile = System.IO.Path.Combine(AppContext.BaseDirectory, "Reports", "ActivistTrain.frx");
            WebReport webReport = new WebReport();
            webReport.Report.RegisterData(partyActivistPrintViewModels, "datas");
            webReport.Report.Load(reportFile);
            webReport.Report.Prepare();
            return View(webReport);
        }

        public async Task<IActionResult> PrintSelected(string idList)
        {
            Guid[] ids = idList.Split(',',StringSplitOptions.RemoveEmptyEntries).Select(s=>Guid.Parse(s)).ToArray();
            List<PartyActivistPrintViewModel> partyActivistPrintViewModels = await GetReportDatas(ids); ;
            string reportFile = System.IO.Path.Combine(AppContext.BaseDirectory, "Reports", "ActivistTrain.frx");
            WebReport webReport = new WebReport();
            webReport.Report.RegisterData(partyActivistPrintViewModels, "datas");
            webReport.Report.Load(reportFile);
            webReport.Report.Prepare();
            return View(webReport);
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
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据保存成功"
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
                        int rowIndex = 0;
                        string fieldsTeacher = "工号/学号,平时成绩,考试成绩";
                        string[] fieldListTeacher = fieldsTeacher.Split(',');
                        foreach (string field in fieldListTeacher)
                        {
                            if (!table.Columns.Contains(field))
                                throw new PartyMemberException($"缺少【{field}】列");
                        }
                        foreach (DataRow row in table.Rows)
                        {
                            rowIndex++;
                            string empNoField = "工号/学号";
                            string psScoreField = "平时成绩";
                            string csScoreField = "考试成绩";
                            string psScore = row[psScoreField].ToString();
                            string csScore = row[csScoreField].ToString();
                            string empNo = row[empNoField].ToString();
                            //跳过工号/学号为空的记录
                            if (string.IsNullOrEmpty(empNoField)) continue;
                            decimal psScoreValue = 0;
                            decimal csScoreValue = 0;
                            if (!decimal.TryParse(psScore, out psScoreValue))
                            {
                                throw new PartyMemberException($"第{rowIndex}行数据中的【{psScoreField}】数据不合法");
                            }
                            if (!decimal.TryParse(csScore, out csScoreValue))
                            {
                                throw new PartyMemberException($"第{rowIndex}行数据中的【{csScoreField}】数据不合法");
                            }
                            if (psScoreValue < 0 || psScoreValue > 100)
                            {
                                throw new PartyMemberException($"第{rowIndex}行数据中的【{psScoreField}】数据不合法");
                            }
                            if (csScoreValue < 0 || csScoreValue > 100)
                            {
                                throw new PartyMemberException($"第{rowIndex}行数据中的【{csScoreField}】数据不合法");
                            }
                            PartyActivist partyActivist = await _context.PartyActivists.Where(p => p.TrainClassId == model.TrainClassId && p.JobNo == empNo).FirstOrDefaultAsync();
                            if (partyActivist == null)
                                throw new PartyMemberException($"第{rowIndex}行数据中的【{empNo}】未找到，请核对工号/学号是否正确");
                            ActivistTrainResult activistTrainResult = await _context.ActivistTrainResults.Where(p => p.PartyActivistId == partyActivist.Id).FirstOrDefaultAsync();
                            var psProp = trainClass.PsGradeProportion;
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
                                    CsGrade = csScoreValue,
                                    TotalGrade = Math.Round(psProp * psScoreValue / 100 + csProp * csScoreValue / 100, 2)
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
                            await _context.SaveChangesAsync();
                        }
                        #endregion
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
                jsonResult.Message = "入党积极分子导入错误";
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

        private bool ActivistTrainResultExists(Guid id)
        {
            return _context.ActivistTrainResults.Any(e => e.Id == id);
        }
    }
}
