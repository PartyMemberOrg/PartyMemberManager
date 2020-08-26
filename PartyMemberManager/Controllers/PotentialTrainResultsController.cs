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
        public async Task<IActionResult> GetDatasWithFilter(Guid? yearTermId, Guid? departmentId, string isPass, string isPrint, Guid? trainClassId, string keyword, int page = 1, int limit = 10)
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
                if (!string.IsNullOrEmpty(isPrint))
                {
                    filter = filter.And(d => d.IsPrint == (isPrint == "true"));
                }
                if (trainClassId != null)
                {
                    filter = filter.And(d => d.PotentialMember.TrainClassId == trainClassId);
                }
                if (CurrentUser.Roles > Role.学院党委)
                {
                    var data = await _context.Set<PotentialTrainResult>().Include(d => d.PotentialMember).Include(d => d.PotentialMember.TrainClass).Include(d => d.PotentialMember.YearTerm)
                        .Where(filter)
                        //.Where(d => d.PotentialMember.YearTerm.Enabled == true)
                        .OrderByDescending(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PotentialTrainResult>().Count();
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
                    jsonResult.Count = _context.Set<PotentialTrainResult>().Count();
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
                        PotentialTrainResult potentialTrainResult = await _context.PotentialTrainResults.Include(d => d.PotentialMember.TrainClass).Where(d => d.Id == id).FirstOrDefaultAsync();
                        PotentialMember potentialMember = await _context.PotentialMembers.FindAsync(potentialTrainResult.PotentialMemberId);
                        var psProp = potentialTrainResult.PotentialMember.TrainClass.PsGradeProportion;
                        var csProp = potentialTrainResult.PotentialMember.TrainClass.CsGradeProportion;
                        if (potentialTrainResult != null)
                        {
                            decimal psGrade = 0;
                            decimal csGrade = 0;
                            if (decimal.TryParse(subItem[1], out psGrade))
                                potentialTrainResult.PsGrade = psGrade;
                            if (decimal.TryParse(subItem[2], out csGrade))
                                potentialTrainResult.CsGrade = csGrade;
                            if (psGrade > 100 || psGrade < 0)
                                throw new PartyMemberException($"【{potentialMember.JobNo}-{potentialMember.Name}】的平时成绩非法");
                            if (csGrade > 100 || csGrade < 0)
                                throw new PartyMemberException($"【{potentialMember.JobNo}-{potentialMember.Name}】的考试成绩非法");
                            potentialTrainResult.TotalGrade = Math.Round(psProp * psGrade / 100 + csProp * csGrade / 100, 2);
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
                            PotentialMember potentialMember = await _context.PotentialMembers.Where(p => p.TrainClassId == model.TrainClassId && p.JobNo == empNo).FirstOrDefaultAsync();
                            if (potentialMember == null)
                                throw new PartyMemberException($"第{rowIndex}行数据中的【{empNo}】未找到，请核对工号/学号是否正确");
                            PotentialTrainResult potentialTrainResult = await _context.PotentialTrainResults.Where(p => p.PotentialMemberId == potentialMember.Id).FirstOrDefaultAsync();
                            var psProp = trainClass.PsGradeProportion;
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
                                    CsGrade = csScoreValue,
                                    TotalGrade= Math.Round(psProp * psScoreValue / 100 + csProp * csScoreValue / 100, 2)
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

        private bool PotentialTrainResultExists(Guid id)
        {
            return _context.PotentialTrainResults.Any(e => e.Id == id);
        }
    }
}
