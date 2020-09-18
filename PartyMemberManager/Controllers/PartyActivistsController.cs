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
using System.IO;
using Newtonsoft.Json;
using System.Data;
using ExcelCore;
using PartyMemberManager.Models;
using PartyMemberManager.Models.PrintViewModel;
using NPOI.SS.Formula.Functions;

namespace PartyMemberManager.Controllers
{
    public class PartyActivistsController : PartyMemberDataControllerBase<PartyActivist>
    {

        public PartyActivistsController(ILogger<PartyActivistsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: PartyActivists
        public async Task<IActionResult> Index(int page = 1)
        {
            var partyActives = _context.PartyActivists.Include(d => d.Department).Include(d => d.Nation);
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(t => t.YearTerm).Include(d => d.TrainClassType)
                    .Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value && d.YearTerm.Enabled == true)
                    .Where(d => d.TrainClassType.Code == "41")
                    .OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(t => t.YearTerm).Include(d => d.TrainClassType)
                    .Where(d => d.YearTerm.Enabled == true)
                    .Where(d => d.TrainClassType.Code == "41")
                    .OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderByDescending(d => d.StartYear).ThenByDescending(d => d.Term).Where(d => d.Enabled == true), "Id", "Name");
            ViewBag.TrainClassTypeId = _context.TrainClassTypes.Where(d => d.Code == "41").Select(d => d.Id).SingleOrDefault();
            return View(await partyActives.Include(d => d.TrainClass).OrderBy(a => a.Ordinal).GetPagedDataAsync(page));
        }
        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(Guid? yearTermId, Guid? departmentId, Guid? trainClassId, string partyMemberType, string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<PartyActivist> jsonResult = new JsonResultDatasModel<PartyActivist>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
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
                    var data = await _context.Set<PartyActivist>().Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.YearTerm)
                        .Where(filter)
                        //.Where(d => d.YearTerm.Enabled == true)
                        .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PartyActivist>().Where(filter).Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<PartyActivist>().Where(filter).Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.TrainClass).Include(d => d.YearTerm)
                        .Where(filter).Where(d => d.YearTerm.Enabled == true)
                        .Where(d => d.DepartmentId == CurrentUser.DepartmentId)
                        .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PartyActivist>().Where(filter).Count();
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
        // GET: PartyActivists/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var partyActivist = await _context.PartyActivists
                    .Include(p => p.Department)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (partyActivist == null)
            {
                return NotFoundData();
            }

            return View(partyActivist);
        }

        // GET: PartyActivists/Create
        public IActionResult Create()
        {
            PartyActivist partyActivist = new PartyActivist();
            ViewBag.NationId = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(t => t.YearTerm).Include(d => d.TrainClassType)
                    .Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value && d.YearTerm.Enabled == true)
                    .Where(d => d.TrainClassType.Code == "41")
                    .OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(t => t.YearTerm).Include(d => d.TrainClassType)
                    .Where(d => d.YearTerm.Enabled == true)
                    .Where(d => d.TrainClassType.Code == "41")
                    .OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.YearTermId = new SelectList(_context.YearTerms.OrderByDescending(d => d.StartYear).ThenByDescending(d => d.Term).Where(d => d.Enabled == true), "Id", "Name");
            ViewBag.TrainClassTypeId = _context.TrainClassTypes.Where(d => d.Code == "41").Select(d => d.Id).SingleOrDefault();
            return View(partyActivist);
        }


        // GET: PartyActivists/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var partyActivist = await _context.PartyActivists.SingleOrDefaultAsync(m => m.Id == id);
            if (partyActivist == null)
            {
                return NotFoundData();
            }
            ViewBag.NationId = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
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
            return View(partyActivist);
        }
        /// <summary>
        /// 删除数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        public override async Task<IActionResult> Delete(Guid? id)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };

            try
            {
                if (id == null)
                    throw new PartyMemberException("未传入删除项目的Id");
                var data = await _context.Set<PartyActivist>().SingleOrDefaultAsync(m => m.Id == id);
                var dataResult = await _context.Set<ActivistTrainResult>().SingleOrDefaultAsync(m => m.PartyActivistId == id);
                if (data == null)
                    throw new PartyMemberException("未找到要删除的数据");
                if (data.IsPrint && CurrentUser.Roles == Role.学院党委)
                {
                    var noName = "【" + data.Name + "-" + data.JobNo + "】";
                    throw new PartyMemberException(noName + "已经打印证书，请联系管理员删除");
                }
                var dataPotentialMember = await _context.Set<PotentialMember>().SingleOrDefaultAsync(m => m.PartyActivistId == data.Id);
                if (dataPotentialMember != null)
                {
                    var dataResultPotentialMember = await _context.Set<PotentialTrainResult>().SingleOrDefaultAsync(m => m.PotentialMemberId == dataPotentialMember.Id);
                    _context.Set<PotentialTrainResult>().Remove(dataResultPotentialMember);
                    _context.Set<PotentialMember>().Remove(dataPotentialMember);
                }
                ValidateDeleteObject(data);
                _context.Set<ActivistTrainResult>().Remove(dataResult);
                _context.Set<PartyActivist>().Remove(data);
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
        /// <summary>
        /// 删除数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("BatchDelete")]
        public async Task<IActionResult> BatchDelete(string idList)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };

            try
            {
                if (string.IsNullOrEmpty(idList))
                    throw new PartyMemberException("未选择删除的入党积极分子");
                string[] idListSplit = idList.Split(",");
                foreach (var item in idListSplit)
                {
                    var partyActivitId = Guid.Parse(item);
                    var data = await _context.Set<PartyActivist>().SingleOrDefaultAsync(m => m.Id == partyActivitId);
                    var dataResult = await _context.Set<ActivistTrainResult>().SingleOrDefaultAsync(m => m.PartyActivistId == partyActivitId);
                    if (data == null)
                        throw new PartyMemberException("未找到要删除的数据");
                    if (data.IsPrint && CurrentUser.Roles == Role.学院党委)
                    {
                        var noName = "【" + data.Name + "-" + data.JobNo + "】";
                        throw new PartyMemberException(noName + "已经打印证书，请联系管理员删除");
                    }
                    var dataPotentialMember = await _context.Set<PotentialMember>().SingleOrDefaultAsync(m => m.PartyActivistId == partyActivitId);
                    if (dataPotentialMember != null)
                    {
                        var dataResultPotentialMember = await _context.Set<PotentialTrainResult>().SingleOrDefaultAsync(m => m.PotentialMemberId == dataPotentialMember.Id);
                        _context.Set<PotentialTrainResult>().Remove(dataResultPotentialMember);
                        _context.Set<PotentialMember>().Remove(dataPotentialMember);
                    }
                    ValidateDeleteObject(data);
                    _context.Set<ActivistTrainResult>().Remove(dataResult);
                    _context.Set<PartyActivist>().Remove(data);
                }

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("YearTermId,TrainClassId,ApplicationTime,ActiveApplicationTime,Duty,Name,JobNo,IdNumber,Sex,PartyMemberType,BirthDate,NationId,Phone,DepartmentId,Class,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PartyActivist partyActivist)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };
            try
            {
                if (partyActivist.Sex.ToString() == "0")
                    throw new PartyMemberException("请选择性别");
                if (partyActivist.NationId == Guid.Empty || partyActivist.NationId == null)
                    throw new PartyMemberException("请选择民族");
                if (partyActivist.PartyMemberType.ToString() == "0")
                    throw new PartyMemberException("请选择类型");
                if (partyActivist.BirthDate == null)
                    throw new PartyMemberException("出生日期不能为空");
                if (CurrentUser.Roles == Role.学院党委)
                    partyActivist.DepartmentId = CurrentUser.DepartmentId.Value;
                else
                {
                    if (partyActivist.DepartmentId == Guid.Empty || partyActivist.DepartmentId == null)
                        throw new PartyMemberException("请选择部门");
                }
                if (partyActivist.YearTermId == Guid.Empty || partyActivist.YearTermId == null)
                    throw new PartyMemberException("请选择学年/学期");
                if (partyActivist.TrainClassId == null || partyActivist.TrainClassId == null)
                    throw new PartyMemberException("请选择培训班");
                if (partyActivist.ApplicationTime == null)
                    throw new PartyMemberException("申请入党时间不能为空");
                if (partyActivist.ActiveApplicationTime == null)
                    throw new PartyMemberException("确定为入党积极分子时间不能为空");
                if (!StringHelper.ValidateIdNumber(partyActivist.IdNumber))
                    throw new PartyMemberException("身份证号不合法");
                ValidateJobNo(partyActivist);

                if (ModelState.IsValid)
                {
                    PartyActivist partyActivistInDb = await _context.PartyActivists.FindAsync(partyActivist.Id);
                    if (partyActivistInDb != null)
                    {
                        var partyActivistOld = _context.PartyActivists.Where(d => d.Id != partyActivistInDb.Id && (d.JobNo == partyActivist.JobNo || d.IdNumber == partyActivist.IdNumber) && d.TrainClassId == partyActivist.TrainClassId).FirstOrDefault();
                        if (partyActivistOld != null)
                            throw new PartyMemberException("该培训班已经存在相同的学号/工号或身份证号，请核对");
                        if (partyActivistInDb.IsPrint && CurrentUser.Roles == Role.学院党委)
                        {
                            var noName = "【" + partyActivistInDb.Name + "-" + partyActivistInDb.JobNo + "】";
                            throw new PartyMemberException(noName + "已经打印证书，请联系管理员修改");
                        }
                        partyActivistInDb.ApplicationTime = partyActivist.ApplicationTime;
                        partyActivistInDb.ActiveApplicationTime = partyActivist.ActiveApplicationTime;
                        partyActivistInDb.Duty = partyActivist.Duty;
                        partyActivistInDb.Name = partyActivist.Name;
                        partyActivistInDb.JobNo = partyActivist.JobNo;
                        partyActivistInDb.IdNumber = partyActivist.IdNumber;
                        partyActivistInDb.NationId = partyActivist.NationId;
                        partyActivistInDb.PartyMemberType = partyActivist.PartyMemberType;
                        partyActivistInDb.BirthDate = partyActivist.BirthDate;
                        partyActivistInDb.Phone = partyActivist.Phone;
                        partyActivistInDb.Department = partyActivist.Department;
                        partyActivistInDb.Class = partyActivist.Class;
                        partyActivistInDb.Id = partyActivist.Id;
                        partyActivistInDb.CreateTime = DateTime.Now;
                        partyActivistInDb.OperatorId = CurrentUser.Id;
                        partyActivistInDb.Ordinal = _context.PartyActivists.Count() + 1;
                        partyActivistInDb.IsDeleted = partyActivist.IsDeleted;
                        partyActivistInDb.YearTermId = partyActivist.YearTermId;
                        partyActivistInDb.TrainClassId = partyActivist.TrainClassId;
                        partyActivistInDb.DepartmentId = partyActivist.DepartmentId;
                        _context.Update(partyActivistInDb);
                    }
                    else
                    {
                        //partyActivist.Id = Guid.NewGuid();
                        partyActivist.CreateTime = DateTime.Now;
                        partyActivist.OperatorId = CurrentUser.Id;
                        partyActivist.Ordinal = _context.PartyActivists.Count() + 1;
                        var partyActivistOld = _context.PartyActivists.Where(d => (d.JobNo == partyActivist.JobNo || d.IdNumber == partyActivist.IdNumber) && d.TrainClassId == partyActivist.TrainClassId).FirstOrDefault();
                        if (partyActivistOld != null)
                            throw new PartyMemberException("该培训班已经存在相同的学号/工号或身份证号，请核对");

                        ActivistTrainResult activistTrainResult = new ActivistTrainResult
                        {
                            PartyActivistId = partyActivist.Id,
                            CreateTime = DateTime.Now,
                            OperatorId = CurrentUser.Id,
                            IsDeleted = false,
                            Ordinal = _context.ActivistTrainResults.Count() + 1
                        };
                        _context.Add(partyActivist);
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

        /// <summary>
        /// 导入入党积极分子
        /// </summary>
        /// <returns></returns>
        public IActionResult Import()
        {
            PartyActivistImportViewModel model = new PartyActivistImportViewModel
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
        public async Task<IActionResult> Import(PartyActivistImportViewModel model)
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
                        #region 导入入党积极分子
                        DataTable table = OfficeHelper.ReadExcelToDataTable(filePath);
                        DataTable tableErrorData = table.Clone();
                        DataColumn columnErrorMessage = tableErrorData.Columns.Add("错误提示", typeof(string));
                        int rowIndex = 0;
                        int successCount = 0;
                        string fieldsTeacher = "姓名,性别,出生年月,民族,所在部门,工号,身份证号,联系电话,提交入党申请时间,确定入党积极分子时间,备注";
                        string fieldsStudent = "姓名,学号,身份证号,性别,出生年月,民族,所在学院,所在班级,联系电话,提交入党申请时间,担任职务,确定入党积极分子时间,备注";
                        string[] fieldListTeacher = fieldsTeacher.Split(',');
                        string[] fieldListStudent = fieldsStudent.Split(',');
                        bool isTeacher = model.PartyMemberType == PartyMemberType.教师;
                        if (isTeacher)
                        {
                            foreach (string field in fieldListTeacher)
                            {
                                if (!table.Columns.Contains(field))
                                    throw new PartyMemberException($"缺少【{field}】列");
                            }
                        }
                        else
                        {
                            foreach (string field in fieldListStudent)
                            {
                                if (!table.Columns.Contains(field))
                                    throw new PartyMemberException($"缺少【{field}】列");
                            }
                        }
                        foreach (DataRow row in table.Rows)
                        {
                            rowIndex++;
                            try
                            {
                                PartyActivist partyActivist = new PartyActivist
                                {
                                    Id = Guid.NewGuid(),
                                    CreateTime = DateTime.Now,
                                    Ordinal = rowIndex,
                                    OperatorId = CurrentUser.Id,
                                    TrainClassId = trainClass.Id,
                                    YearTermId = trainClass.YearTermId
                                    //Year=trainClass.Year,
                                    //Term=trainClass.Term,
                                };
                                if (isTeacher)
                                {
                                    string nameField = "姓名";
                                    string sexField = "性别";
                                    string birthdayField = "出生年月";
                                    string nationField = "民族";
                                    string departmentField = "所在部门";
                                    string empNoField = "工号";
                                    string idField = "身份证号";
                                    string phoneField = "联系电话";
                                    string timeField = "提交入党申请时间";
                                    string confirmTimeField = "确定入党积极分子时间";
                                    string remarkField = "备注";
                                    string name = row[nameField].ToString();
                                    string sex = row[sexField].ToString();
                                    string birthday = row[birthdayField].ToString();
                                    string nation = row[nationField].ToString();
                                    string department = row[departmentField].ToString();
                                    string empNo = row[empNoField].ToString();
                                    string id = row[idField].ToString();
                                    string phone = row[phoneField].ToString();
                                    string time = row[timeField].ToString();
                                    string confirmTime = row[confirmTimeField].ToString();
                                    string remark = row[remarkField].ToString();
                                    //跳过姓名为空的记录
                                    if (string.IsNullOrEmpty(name)) continue;
                                    birthday = birthday.Replace(".", "-").Replace("/", "-");
                                    time = time.Replace(".", "-").Replace("/", "-");
                                    confirmTime = confirmTime.Replace(".", "-").Replace("/", "-");
                                    DateTime birthdayValue = DateTime.Now;
                                    if (!birthday.Contains("-") && birthday.Length == 6)
                                        birthday = birthday + "01";
                                    else if (birthday.Contains("-") && birthday.IndexOf('-') == birthday.LastIndexOf('-'))
                                        birthday = birthday + "-01";
                                    if (!TryParseYearMonth(birthday, out birthdayValue))
                                    {
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{birthdayField}】年月格式不合法");
                                    }
                                    DateTime timeValue = DateTime.Now;
                                    if (!TryParseDate(time, out timeValue))
                                    {
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{timeField}】日期格式不合法");
                                    }
                                    DateTime confirmTimeValue = DateTime.Now;
                                    if (!TryParseDate(confirmTime, out confirmTimeValue))
                                    {
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{confirmTimeField}】日期格式不合法");
                                    }
                                    if (!StringHelper.ValidateIdNumber(id))
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{idField}】不合法");

                                    if (ValidateImportTeacherNo(empNo))
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{empNoField}】不合法");

                                    Nation nationData = _context.Nations.Where(n => n.Name == nation).FirstOrDefault();
                                    Guid nationId = nationData.Id;
                                    //部门只要有包含（两种包含：导入的名称被部门包含，或者导入的名称包含库中的部门名称）
                                    Department departmentData = _context.Departments.Where(d => d.Name.Contains(department) || department.Contains(d.Name)).FirstOrDefault();
                                    Guid departmentId = departmentData.Id;
                                    partyActivist.Name = name;
                                    partyActivist.Sex = Sex.Parse<Sex>(sex);
                                    partyActivist.BirthDate = birthdayValue;
                                    partyActivist.NationId = nationId;
                                    partyActivist.DepartmentId = departmentId;
                                    partyActivist.JobNo = empNo;
                                    partyActivist.IdNumber = id;
                                    partyActivist.Phone = phone;
                                    partyActivist.ApplicationTime = timeValue;
                                    partyActivist.ActiveApplicationTime = confirmTimeValue;
                                    partyActivist.PartyMemberType = PartyMemberType.教师;
                                }
                                else
                                {
                                    string nameField = "姓名";
                                    string sexField = "性别";
                                    string birthdayField = "出生年月";
                                    string nationField = "民族";
                                    string idField = "身份证号";
                                    string phoneField = "联系电话";
                                    string timeField = "提交入党申请时间";
                                    string confirmTimeField = "确定入党积极分子时间";
                                    string remarkField = "备注";
                                    string studentNoField = "学号";
                                    string collegeField = "所在学院";
                                    string classField = "所在班级";
                                    string titleField = "担任职务";
                                    string name = row[nameField].ToString();
                                    string sex = row[sexField].ToString();
                                    string birthday = row[birthdayField].ToString();
                                    string nation = row[nationField].ToString();
                                    string id = row[idField].ToString();
                                    string phone = row[phoneField].ToString();
                                    string time = row[timeField].ToString();
                                    string confirmTime = row[confirmTimeField].ToString();
                                    string remark = row[remarkField].ToString();
                                    string studentNo = row[studentNoField].ToString();
                                    string college = row[collegeField].ToString();
                                    string @class = row[classField].ToString();
                                    string title = row[titleField].ToString();
                                    //跳过姓名为空的记录
                                    if (string.IsNullOrEmpty(name)) continue;
                                    birthday = birthday.Replace(".", "-").Replace("/", "-");
                                    time = time.Replace(".", "-").Replace("/", "-");
                                    confirmTime = confirmTime.Replace(".", "-").Replace("/", "-");
                                    DateTime birthdayValue = DateTime.Now;
                                    if (!birthday.Contains("-") && birthday.Length == 6)
                                        birthday = birthday + "01";
                                    else if (birthday.Contains("-") && birthday.IndexOf('-') == birthday.LastIndexOf('-'))
                                        birthday = birthday + "-01";
                                    if (!TryParseYearMonth(birthday, out birthdayValue))
                                    {
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{birthdayField}】年月格式不合法");
                                    }
                                    DateTime timeValue = DateTime.Now;
                                    if (!TryParseDate(time, out timeValue))
                                    {
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{timeField}】日期格式不合法");
                                    }
                                    DateTime confirmTimeValue = DateTime.Now;
                                    if (!TryParseDate(confirmTime, out confirmTimeValue))
                                    {
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{confirmTimeField}】日期格式不合法");
                                    }
                                    if (!StringHelper.ValidateIdNumber(id))
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{idField}】不合法");
                                    if(model.PartyMemberType==PartyMemberType.研究生)
                                    {
                                        if (ValidateImportPostgraduateNo(studentNo))
                                            throw new ImportDataErrorException($"第{rowIndex}行数据中的【{studentNoField}】不合法");
                                    }
                                    else
                                    {
                                        if (ValidateImportUndergraduateNo(studentNo))
                                            throw new ImportDataErrorException($"第{rowIndex}行数据中的【{studentNoField}】不合法");
                                    }
                                    Nation nationData = _context.Nations.Where(n => n.Name == nation).FirstOrDefault();
                                    Guid nationId = nationData.Id;
                                    //部门只要有包含（两种包含：导入的名称被部门包含，或者导入的名称包含库中的部门名称）
                                    Department departmentData = _context.Departments.Where(d => d.Name.Contains(college) || college.Contains(d.Name)).FirstOrDefault();
                                    Guid departmentId = departmentData.Id;
                                    partyActivist.Name = name;
                                    partyActivist.Sex = Sex.Parse<Sex>(sex);
                                    partyActivist.BirthDate = birthdayValue;
                                    partyActivist.NationId = nationId;
                                    partyActivist.DepartmentId = departmentId;
                                    partyActivist.JobNo = studentNo;
                                    partyActivist.IdNumber = id;
                                    partyActivist.Phone = phone;
                                    partyActivist.ApplicationTime = timeValue;
                                    partyActivist.ActiveApplicationTime = confirmTimeValue;
                                    partyActivist.Class = @class;
                                    partyActivist.Duty = title;
                                    partyActivist.PartyMemberType = model.PartyMemberType;
                                }
                                ActivistTrainResult activistTrainResult = new ActivistTrainResult
                                {
                                    Id = Guid.NewGuid(),
                                    PartyActivistId = partyActivist.Id,
                                    CreateTime = DateTime.Now,
                                    OperatorId = CurrentUser.Id,
                                    IsDeleted = false,
                                    Ordinal = _context.ActivistTrainResults.Count() + 1
                                };
                                var partyActivistOld = _context.PartyActivists.Where(d => d.JobNo == partyActivist.JobNo && d.TrainClassId == partyActivist.TrainClassId).FirstOrDefault();
                                if (partyActivistOld != null)
                                {
                                    var noName = "【" + partyActivist.Name + "-" + partyActivist.JobNo + "】";
                                    throw new ImportDataErrorException(noName + "该班级中已经存在学号或身份证号相同的学员，请核对");
                                }
                                _context.PartyActivists.Add(partyActivist);
                                _context.ActivistTrainResults.Add(activistTrainResult);
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
                            string fileName = $"入党积极分子错误数据_{CurrentUser.LoginName}.xlsx";
                            string fileWithPath = $"{basePath}{Path.DirectorySeparatorChar}{fileName}";
                            Stream streamOutExcel = OfficeHelper.ExportExcelByOpenXml(tableErrorData);
                            FileStream outExcelFile = new FileStream(fileWithPath, FileMode.Create, System.IO.FileAccess.Write);
                            byte[] bytes = new byte[streamOutExcel.Length];
                            streamOutExcel.Read(bytes, 0, (int)streamOutExcel.Length);
                            outExcelFile.Write(bytes, 0, bytes.Length);
                            outExcelFile.Close();
                            streamOutExcel.Close();
                            jsonResult.ErrorDataFile = $"PartyActivists/GetErrorImportData?fileName={fileName}";
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
            return File(outExcelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "入党积极分子导入失败数据.xlsx");
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
        /// 验证工号和学号
        /// </summary>
        /// <param name="partyActivist"></param>
        public void ValidateJobNo(PartyActivist partyActivist)
        {
            if (!StringHelper.ValidateJobNo(partyActivist.JobNo))
                throw new PartyMemberException("学号/工号不合法");
            int noLength = 10;
            switch (partyActivist.PartyMemberType)
            {
                case PartyMemberType.教师:
                    noLength = 10;
                    break;
                case PartyMemberType.本科生:
                    noLength = 12;
                    break;
                case PartyMemberType.研究生:
                    noLength = 13;
                    break;
            }
            if (partyActivist.JobNo.Trim().Length != noLength)
                throw new PartyMemberException("学号/工号长度不合法");
        }
        /// <summary>
        /// 校验教工工号
        /// </summary>
        /// <param name="no"></param>
        public bool ValidateImportTeacherNo(string no)
        {
            if (string.IsNullOrEmpty(no))
                return false;
            if (!StringHelper.ValidateJobNo(no))
                return false;
            int noLength = 10;
            if (no.Trim().Length != noLength)
                return false;
            return true;
        }
        /// <summary>
        /// 校验本科生学号(仅导入用）
        /// </summary>
        /// <param name="partyActivist"></param>
        public bool ValidateImportUndergraduateNo(string no)
        {
            if (string.IsNullOrEmpty(no))
                return false;
            if (!StringHelper.ValidateJobNo(no))
                return false;
            int noLength = 12;
            if (no.Trim().Length != noLength)
                return false;
            return true;
        }
        /// <summary>
        /// 校验研究生学号(仅导入用）
        /// </summary>
        /// <param name="no"></param>
        public bool ValidateImportPostgraduateNo(string no)
        {
            if (string.IsNullOrEmpty(no))
                return false;
            if (!StringHelper.ValidateJobNo(no))
                return false;
            int noLength = 13;
            if (no.Trim().Length != noLength)
                return false;
            return true;
        }
        private bool PartyActivistExists(Guid id)
        {
            return _context.PartyActivists.Any(e => e.Id == id);
        }
    }
}
