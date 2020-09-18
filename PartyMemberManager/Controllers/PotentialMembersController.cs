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
using System.Data;
using System.IO;
using ExcelCore;

namespace PartyMemberManager.Controllers
{
    public class PotentialMembersController : PartyMemberDataControllerBase<PotentialMember>
    {

        public PotentialMembersController(ILogger<PotentialMembersController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: PotentialMembers
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.PotentialMembers.Include(p => p.Department).Include(p => p.Nation).Include(p => p.TrainClass);
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
            ViewBag.TrainClassTypeId = _context.TrainClassTypes.Where(d => d.Code == "42").Select(d => d.Id).SingleOrDefault(); return View(await pMContext.ToListAsync());
        }
        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(Guid? yearTermId, Guid? departmentId, Guid? trainClassId, string partyMemberType, string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<PotentialMember> jsonResult = new JsonResultDatasModel<PotentialMember>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<PotentialMember>();
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
                    var data = await _context.Set<PotentialMember>().Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.YearTerm)
                        .Where(filter)
                        //.Where(d => d.YearTerm.Enabled == true)
                        .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PotentialMember>().Where(filter).Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<PotentialMember>().Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.TrainClass).Include(d => d.YearTerm)
                        .Where(filter).Where(d => d.YearTerm.Enabled == true)
                        .Where(d => d.DepartmentId == CurrentUser.DepartmentId)
                        .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PotentialMember>().Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.TrainClass).Include(d => d.YearTerm)
                        .Where(filter).Where(d => d.YearTerm.Enabled == true)
                        .Where(d => d.DepartmentId == CurrentUser.DepartmentId)
                        .Count();
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

        // GET: PotentialMembers/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var potentialMember = await _context.PotentialMembers
                    .Include(p => p.Department)
                    .Include(p => p.Nation)
                    .Include(p => p.TrainClass)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (potentialMember == null)
            {
                return NotFoundData();
            }

            return View(potentialMember);
        }

        // GET: PotentialMembers/Create
        public IActionResult Create(string idList)
        {
            PotentialMember potentialMember = new PotentialMember();
            if (!string.IsNullOrEmpty(idList))
                ViewBag.IdList = idList.TrimEnd(',');
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
            return View(potentialMember);
        }


        // GET: PotentialMembers/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }
            var potentialMember = await _context.PotentialMembers.SingleOrDefaultAsync(m => m.Id == id);
            if (potentialMember == null)
            {
                return NotFoundData();
            }
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
            return View(potentialMember);
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
                    var potentialMemberId = Guid.Parse(item);
                    var data = await _context.Set<PotentialMember>().SingleOrDefaultAsync(m => m.Id == potentialMemberId);
                    var dataResult = await _context.Set<PotentialTrainResult>().SingleOrDefaultAsync(m => m.PotentialMemberId == potentialMemberId);
                    if (data == null)
                        throw new PartyMemberException("未找到要删除的数据");
                    if (data.IsPrint && CurrentUser.Roles == Role.学院党委)
                    {
                        var noName = "【" + data.Name + "-" + data.JobNo + "】";
                        throw new PartyMemberException(noName + "已经打印证书，请联系管理员删除");
                    }
                    ValidateDeleteObject(data);
                    _context.Set<PotentialTrainResult>().Remove(dataResult);
                    _context.Set<PotentialMember>().Remove(data);
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
                var data = await _context.Set<PotentialMember>().SingleOrDefaultAsync(m => m.Id == id);
                var dataResult = await _context.Set<PotentialTrainResult>().SingleOrDefaultAsync(m => m.PotentialMemberId == id);
                if (data == null)
                    throw new PartyMemberException("未找到要删除的数据");
                if (data.IsPrint && CurrentUser.Roles==Role.学院党委)
                {
                    var noName = "【" + data.Name + "-" + data.JobNo + "】";
                    throw new PartyMemberException(noName + "已经打印证书，请联系管理员删除");
                }
                ValidateDeleteObject(data);
                _context.Set<PotentialTrainResult>().Remove(dataResult);
                _context.Set<PotentialMember>().Remove(data);
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
        public override async Task<IActionResult> Save(PotentialMember potentialMember)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };
            try
            {
                if (potentialMember.YearTermId == null)
                    throw new PartyMemberException("请选择学年/学期");
                if (potentialMember.TrainClassId == null)
                    throw new PartyMemberException("请选择培训班");
                if (potentialMember.PotentialMemberTime == null)
                    throw new PartyMemberException("请选择发展时间");
                PotentialMember potentialMemberInDb = await _context.PotentialMembers.FindAsync(potentialMember.Id);
                if (potentialMemberInDb != null)
                {
                    if (potentialMemberInDb.IsPrint && CurrentUser.Roles == Role.学院党委)
                    {
                        var noName = "【" + potentialMemberInDb.Name + "-" + potentialMemberInDb.JobNo + "】";
                        throw new PartyMemberException(noName + "已经打印证书，请联系管理员修改");
                    }
                    potentialMemberInDb.PotentialMemberTime = potentialMember.PotentialMemberTime;
                    potentialMemberInDb.TrainClassId = potentialMember.TrainClassId;
                    //potentialMemberInDb.CreateTime = DateTime.Now;
                    //potentialMemberInDb.OperatorId = CurrentUser.Id;
                    //potentialMemberInDb.Ordinal = _context.PotentialMembers.Count() + 1;
                    //potentialMemberInDb.IsDeleted = potentialMember.IsDeleted;
                    _context.Update(potentialMemberInDb);
                }
                else
                {
                    if (string.IsNullOrEmpty(potentialMember.IdList))
                        throw new PartyMemberException("请选择成绩合格的入党积极分子作为发展对象");
                    string[] partyAcitvistsId = potentialMember.IdList.Split(",");
                    foreach (var item in partyAcitvistsId)
                    {
                        Guid activistTrainResultId = Guid.Parse(item);
                        var activistTrainResult = await _context.ActivistTrainResults.Include(d=>d.PartyActivist).Where(d=>d.Id== activistTrainResultId).FirstOrDefaultAsync();
                        var potentialMemberOld = _context.PotentialMembers.Where(d => d.PartyActivistId == activistTrainResult.PartyActivistId && d.TrainClassId == potentialMember.TrainClassId).FirstOrDefault();

                        if (potentialMemberOld != null)
                        {
                            var noName = "【" + potentialMemberOld.Name + "-" + potentialMemberOld.JobNo + "】" + "已在该培训班";
                            throw new PartyMemberException(noName);
                        }
                        if (!activistTrainResult.PartyActivist.IsPrint)
                        {
                            var noName = "【" + activistTrainResult.PartyActivist.Name + "-" + activistTrainResult.PartyActivist.JobNo + "】" + "尚未打印结业证，还不是合格的入党积极分子";
                            throw new PartyMemberException(noName);
                        }
                        var partyActivist = await _context.PartyActivists.FindAsync(activistTrainResult.PartyActivistId);
                        PotentialMember potentialMemberNew = new PotentialMember
                        {
                            ApplicationTime = partyActivist.ApplicationTime,
                            ActiveApplicationTime = partyActivist.ActiveApplicationTime,
                            Name = partyActivist.Name,
                            JobNo = partyActivist.JobNo,
                            IdNumber = partyActivist.IdNumber,
                            NationId = partyActivist.NationId,
                            PartyMemberType = partyActivist.PartyMemberType,
                            BirthDate = partyActivist.BirthDate,
                            Phone = partyActivist.Phone,
                            Department = partyActivist.Department,
                            Class = partyActivist.Class,
                            CreateTime = DateTime.Now,
                            OperatorId = CurrentUser.Id,
                            Ordinal = _context.PotentialMembers.Count() + 1,
                            IsDeleted = partyActivist.IsDeleted,
                            DepartmentId = partyActivist.DepartmentId,
                            PartyActivistId = partyActivist.Id,
                            Sex = partyActivist.Sex,
                            ///后期选择的信息
                            TrainClassId = potentialMember.TrainClassId,
                            YearTermId = potentialMember.YearTermId,
                            PotentialMemberTime = potentialMember.PotentialMemberTime
                        };
                        PotentialTrainResult potentialTrainResult = new PotentialTrainResult
                        {
                            PotentialMemberId = potentialMemberNew.Id,
                            CreateTime = DateTime.Now,
                            OperatorId = CurrentUser.Id,
                            IsDeleted = false,
                            Ordinal = _context.PotentialMembers.Count() + 1
                        };
                        _context.Add(potentialMemberNew);
                        _context.Add(potentialTrainResult);
                    }
                }
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

        /// <summary>
        /// 导出所有学生数据
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Export(Guid? yearTermId, Guid? departmentId, Guid? trainClassId, string partyMemberType, string keyword)
        {
            try
            {
                string fileName = "发展对象导出名单.xlsx";
                List<PotentialMember> potentialMembers = null;
                var filter = PredicateBuilder.True<PotentialMember>();
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
                    potentialMembers = await _context.Set<PotentialMember>().Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.YearTerm)
                        .Where(filter)
                        .OrderByDescending(d => d.Ordinal)
                        .ToListAsync();
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    potentialMembers = await _context.Set<PotentialMember>().Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.TrainClass).Include(d => d.YearTerm)
                        .Where(filter).Where(d => d.YearTerm.Enabled == true)
                        .Where(d => d.DepartmentId == CurrentUser.DepartmentId)
                        .OrderByDescending(d => d.Ordinal).ToListAsync();
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
                table.Columns.Add("确定入党积极分子时间", typeof(string));
                table.Columns.Add("备注", typeof(string));
                table.Columns.Add("平时成绩", typeof(string));
                table.Columns.Add("实践成绩", typeof(string));
                table.Columns.Add("考试成绩", typeof(string));
                table.Columns.Add("补考成绩", typeof(string));
                table.Columns.Add("总评成绩", typeof(string));
                foreach (PotentialMember potentialMember in potentialMembers)
                {
                    PotentialTrainResult potentialTrainResult = await _context.PotentialTrainResults.FirstOrDefaultAsync(a => a.PotentialMemberId == potentialMember.Id);
                    DataRow row = table.NewRow();
                    row["学年学期"] = potentialMember.YearTermDisplay;
                    row["培训班"] = potentialMember.TrainClassDisplay;
                    row["类型"] = potentialMember.PartyMemberTypeDisplay;
                    row["学号"] = potentialMember.JobNo;
                    row["姓名"] = potentialMember.Name;
                    row["身份证号"] = potentialMember.IdNumber;
                    row["性别"] = potentialMember.Sex;
                    row["出生年月"] = string.Format("{0:yyyy-MM}", potentialMember.BirthDate);
                    row["民族"] = potentialMember.NationDisplay;
                    row["所在学院"] = potentialMember.DepartmentDisplay;
                    row["所在班级"] = potentialMember.Class;
                    row["联系电话"] = potentialMember.Phone;
                    row["提交入党申请时间"] = string.Format("{0:yyyy-MM-dd}", potentialMember.ApplicationTime);
                    row["确定入党积极分子时间"] = string.Format("{0:yyyy-MM-dd}", potentialMember.ActiveApplicationTime);
                    row["备注"] = "";
                    row["平时成绩"] = string.Format("{0:#.#}", potentialTrainResult.PsGrade);
                    row["实践成绩"] = string.Format("{0:#.#}", potentialTrainResult.SjGrade);
                    row["考试成绩"] = string.Format("{0:#.#}", potentialTrainResult.CsGrade);
                    row["补考成绩"] = string.Format("{0:#.#}", potentialTrainResult.BcGrade);
                    row["总评成绩"] = string.Format("{0:#.#}", potentialTrainResult.TotalGrade);
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


        [HttpPost]

        public async Task<IActionResult> AddToPotential(string[] datas, Guid trainClassId, DateTime potentialMemberTime)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "添加成功"
            };
            try
            {
                foreach (var item in datas)
                {
                    Guid id = Guid.Parse(item);
                    PartyActivist partyActivist = await _context.PartyActivists.FindAsync(id);
                    PotentialMember potentialMemberInDb = await _context.PotentialMembers.Where(d => d.JobNo == partyActivist.JobNo).FirstOrDefaultAsync();
                    if (potentialMemberInDb != null)
                    {
                        jsonResult.Code = -2;
                        jsonResult.Message += potentialMemberInDb.JobNo + " ";
                    }
                    PotentialMember potentialMember = new PotentialMember
                    {
                        Name = partyActivist.Name,
                        JobNo = partyActivist.JobNo,
                        TrainClassId = trainClassId,
                        IdNumber = partyActivist.IdNumber,
                        Sex = partyActivist.Sex,
                        PartyMemberType = partyActivist.PartyMemberType,
                        BirthDate = partyActivist.BirthDate,
                        Nation = partyActivist.Nation,
                        Phone = partyActivist.Phone,
                        DepartmentId = partyActivist.DepartmentId,
                        Class = partyActivist.Class,
                        ActiveApplicationTime = partyActivist.ActiveApplicationTime,
                        ApplicationTime = partyActivist.ApplicationTime,
                        PotentialMemberTime = potentialMemberTime

                    };
                    _context.PotentialMembers.Add(potentialMember);
                }
                if (jsonResult.Code == -2)
                    jsonResult.Message.Replace("添加成功", "已经存在的学号/工号:");
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
        private bool PotentialMemberExists(Guid id)
        {
            return _context.PotentialMembers.Any(e => e.Id == id);
        }
    }
}
