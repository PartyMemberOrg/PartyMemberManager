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
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.Nations = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "41").Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value).OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "41").OrderBy(d => d.Ordinal), "Id", "Name");
            return View(await partyActives.Include(d => d.TrainClass).OrderBy(a => a.Ordinal).GetPagedDataAsync(page));
        }
        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(Guid? departmentId, string term, string partyMemberType, string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<PartyActivist> jsonResult = new JsonResultDatasModel<PartyActivist>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<PartyActivist>();
                if (departmentId != null)
                {
                    filter = filter.And(d => d.DepartmentId == departmentId);
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
                    var data = await _context.Set<PartyActivist>().Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Where(filter)
                        .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PartyActivist>().Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<PartyActivist>().Where(filter).Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Where(d => d.DepartmentId == CurrentUser.DepartmentId)
                        .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PartyActivist>().Count();
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
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.Nations = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "41").Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value).OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "41").OrderBy(d => d.Ordinal), "Id", "Name");
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
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.Nations = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "41").Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value).OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "41").OrderBy(d => d.Ordinal), "Id", "Name");
            return View(partyActivist);
        }
        /// <summary>
        /// 删除数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        public override  async Task<IActionResult> Delete(Guid? id)
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
                var dataResult = await _context.Set<ActivistTrainResult>().SingleOrDefaultAsync(m => m.PartyActivistId== id);
                if (data == null)
                    throw new PartyMemberException("未找到要删除的数据");
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("TrainClassId,ApplicationTime,ActiveApplicationTime,Duty,Name,JobNo,IdNumber,Sex,PartyMemberType,BirthDate,NationId,Phone,DepartmentId,Class,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PartyActivist partyActivist)
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
                    PartyActivist partyActivistInDb = await _context.PartyActivists.FindAsync(partyActivist.Id);
                    if (partyActivist.NationId == null)
                        throw new PartyMemberException("请选择民族");
                    if (partyActivist.Sex.ToString() == null)
                        throw new PartyMemberException("请选择性别");
                    if (partyActivist.TrainClassId == null)
                        throw new PartyMemberException("请选择培训班");
                    if (CurrentUser.Roles == Role.学院党委)
                        partyActivist.DepartmentId = CurrentUser.DepartmentId.Value;
                    else
                    {
                        if (partyActivist.DepartmentId == null)
                            throw new PartyMemberException("请选择部门");
                    }
                    if (partyActivistInDb != null)
                    {
                        partyActivistInDb.ApplicationTime = partyActivist.ApplicationTime;
                        partyActivistInDb.ActiveApplicationTime = partyActivist.ActiveApplicationTime;
                        partyActivistInDb.Duty = partyActivist.Duty;
                        partyActivistInDb.Name = partyActivist.Name;
                        partyActivistInDb.JobNo = partyActivist.JobNo;
                        partyActivistInDb.IdNumber = partyActivist.IdNumber;
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
                        var partyActivistOld = _context.PartyActivists.Where(d => d.JobNo == partyActivist.JobNo).FirstOrDefault();
                        if (partyActivistOld != null)
                            throw new PartyMemberException("该学号或工号已经存在");

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
            //计算当前的学年和学期
            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;
            Term term = Term.第一学期;
            if (month > 2 && month < 8)
            {
                term = Term.第二学期;
                year--;
            }
            else
            {
                term = Term.第一学期;
                if (month <= 2)
                    year--;
            }
            PartyActivistImportViewModel model = new PartyActivistImportViewModel
            {
                YearBegin = year,
                Term = term,
                DepartmentId = CurrentUser.DepartmentId.HasValue ? CurrentUser.DepartmentId.Value : Guid.Empty,
            };
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "41").Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value).OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "41").OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClassTypeId = new SelectList(_context.TrainClassTypes.Where(d => d.Code.StartsWith("4")).OrderByDescending(d => d.Code), "Id", "Name");
            else
                ViewBag.TrainClassTypeId = new SelectList(_context.TrainClassTypes.OrderBy(d => d.Code), "Id", "Name");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Import(PartyActivistImportViewModel model)
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
                        #region 导入一般事项
                        DataTable table = OfficeHelper.ReadExcelToDataTable(filePath);
                        int rowIndex = 0;
                        string fieldsTeacher = "姓名,性别,出生年月,民族,所在部门,工号,身份证号,联系电话,提交入党申请时间,备注";
                        string fieldsStudent = "姓名,学号,身份证号,性别,出生年月,民族,所在学院,所在班级,联系电话,提交入党申请时间,担任职务,备注";
                        string[] fieldListTeacher = fieldsTeacher.Split(',');
                        string[] fieldListStudent = fieldsStudent.Split(',');
                        bool isTeacher = true;
                        if (table.Columns.Contains("学号"))
                            isTeacher = false;
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
                            PartyActivist partyActivist = new PartyActivist
                            {
                                Id = Guid.NewGuid(),
                                CreateTime = DateTime.Now,
                                Ordinal = rowIndex,
                                OperatorId = CurrentUser.Id,
                                TrainClassId = trainClass.Id,
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
                                string remark = row[remarkField].ToString();
                                //跳过姓名为空的记录
                                if (string.IsNullOrEmpty(name)) continue;
                                birthday = birthday.Replace(".", "").Replace("/", "").Replace("-", "");
                                time = time.Replace(".", "").Replace("/", "").Replace("-", "");
                                DateTime birthdayValue = DateTime.Now;
                                if (birthday.Length < 6)
                                    birthday = birthday + "01";
                                if (!TryParseYearMonth(birthday, out birthdayValue))
                                {
                                    throw new PartyMemberException($"第{rowIndex}行数据中的【{birthdayField}】年月格式不合法");
                                }
                                DateTime timeValue = DateTime.Now;
                                if (!TryParseDate(time, out timeValue))
                                {
                                    throw new PartyMemberException($"第{rowIndex}行数据中的【{timeField}】日期格式不合法");
                                }
                                Nation nationData = _context.Nations.Where(n => n.Name == nation).FirstOrDefault();
                                Guid nationId = nationData.Id;
                                //部门只要有包含（两种包含：导入的名称被部门包含，或者导入的名称包含库中的部门名称）
                                Department departmentData = _context.Departments.Where(d => d.Name.Contains(department)||department.Contains(d.Name)).FirstOrDefault();
                                Guid departmentId = departmentData.Id;
                                partyActivist.Name = name;
                                partyActivist.Sex = Sex.Parse<Sex>(sex);
                                partyActivist.BirthDate = birthday;
                                partyActivist.NationId = nationId;
                                partyActivist.DepartmentId = departmentId;
                                partyActivist.JobNo = empNo;
                                partyActivist.IdNumber = id;
                                partyActivist.Phone = phone;
                                partyActivist.ApplicationTime = time;
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
                                string remark = row[remarkField].ToString();
                                string studentNo = row[studentNoField].ToString();
                                string college = row[collegeField].ToString();
                                string @class = row[classField].ToString();
                                string title = row[titleField].ToString();
                                //跳过姓名为空的记录
                                if (string.IsNullOrEmpty(name)) continue;
                                birthday = birthday.Replace(".", "-").Replace("/", "-");
                                time = time.Replace(".", "-").Replace("/", "-");
                                DateTime birthdayValue = DateTime.Now;
                                if (birthday.Length < 6)
                                    birthday = birthday + "01";
                                if (!TryParseYearMonth(birthday, out birthdayValue))
                                {
                                    throw new PartyMemberException($"第{rowIndex}行数据中的【{birthdayField}】年月格式不合法");
                                }
                                DateTime timeValue = DateTime.Now;
                                if (!TryParseDate(time, out timeValue))
                                {
                                    throw new PartyMemberException($"第{rowIndex}行数据中的【{timeField}】日期格式不合法");
                                }
                                Nation nationData = _context.Nations.Where(n => n.Name == nation).FirstOrDefault();
                                Guid nationId = nationData.Id;
                                //部门只要有包含（两种包含：导入的名称被部门包含，或者导入的名称包含库中的部门名称）
                                Department departmentData = _context.Departments.Where(d => d.Name.Contains(college)||college.Contains(d.Name)).FirstOrDefault();
                                Guid departmentId = departmentData.Id;
                                partyActivist.Name = name;
                                partyActivist.Sex = Sex.Parse<Sex>(sex);
                                partyActivist.BirthDate = birthday;
                                partyActivist.NationId = nationId;
                                partyActivist.DepartmentId = departmentId;
                                partyActivist.JobNo = studentNo;
                                partyActivist.IdNumber = id;
                                partyActivist.Phone = phone;
                                partyActivist.ApplicationTime = time;
                                //该字段不允许为空有问题
                                partyActivist.ActiveApplicationTime = time;
                                partyActivist.Class = @class;
                                partyActivist.Duty = title;
                            }
                            _context.PartyActivists.Add(partyActivist);
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
        private bool PartyActivistExists(Guid id)
        {
            return _context.PartyActivists.Any(e => e.Id == id);
        }

    }
}
