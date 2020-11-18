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
using PartyMemberManager.Models;
using Newtonsoft.Json;

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

        // GET: PotentialMembers/CreateNoActivist 
        public IActionResult CreateNoActivist()
        {
            PotentialMember potentialMember = new PotentialMember();
            ViewBag.NationId = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.DepartmentId = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(t => t.YearTerm).Include(d => d.TrainClassType)
                    .Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value && d.YearTerm.Enabled == true)
                    .Where(d => d.TrainClassType.Code == "42")
                    .OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(t => t.YearTerm).Include(d => d.TrainClassType)
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

        // GET: PartyActivists/Edit/5
        public async Task<IActionResult> EditNoActivist(Guid? id)
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
            ViewBag.NationId = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public  async Task<IActionResult> SaveNoActivist([Bind("YearTermId,TrainClassId,ApplicationTime,ActiveApplicationTime,PotentialMemberTime,Duty,Name,JobNo,IdNumber,Sex,PartyMemberType,BirthDate,NationId,Phone,DepartmentId,Class,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PotentialMember potentialMember)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };
            try
            {
                if (potentialMember.Sex.ToString() == "0")
                    throw new PartyMemberException("请选择性别");
                if (potentialMember.NationId == Guid.Empty || potentialMember.NationId == null)
                    throw new PartyMemberException("请选择民族");
                if (potentialMember.PartyMemberType.ToString() == "0")
                    throw new PartyMemberException("请选择类型");
                if (potentialMember.BirthDate == null)
                    throw new PartyMemberException("出生日期不能为空");
                if (CurrentUser.Roles == Role.学院党委)
                    potentialMember.DepartmentId = CurrentUser.DepartmentId.Value;
                else
                {
                    if (potentialMember.DepartmentId == Guid.Empty || potentialMember.DepartmentId == null)
                        throw new PartyMemberException("请选择部门");
                }
                if (potentialMember.YearTermId == Guid.Empty || potentialMember.YearTermId == null)
                    throw new PartyMemberException("请选择学年/学期");
                if (potentialMember.TrainClassId == null || potentialMember.TrainClassId == null)
                    throw new PartyMemberException("请选择培训班");
                if (potentialMember.ApplicationTime == null)
                    throw new PartyMemberException("申请入党时间不能为空");
                if (potentialMember.ActiveApplicationTime == null)
                    throw new PartyMemberException("确定为入党积极分子时间不能为空");
                if (potentialMember.PotentialMemberTime == null)
                    throw new PartyMemberException("确定为发展对象时间不能为空");
                if (!StringHelper.ValidateIdNumber(potentialMember.IdNumber))
                    throw new PartyMemberException("身份证号不合法");
                ValidateJobNo(potentialMember);

                if (ModelState.IsValid)
                {
                    PotentialMember potentialMemberInDb = await _context.PotentialMembers.FindAsync(potentialMember.Id);
                    if (potentialMemberInDb != null)
                    {
                        var potentialMemberOld = _context.PotentialMembers.Where(d => d.Id != potentialMemberInDb.Id && (d.JobNo == potentialMember.JobNo || d.IdNumber == potentialMember.IdNumber) && d.TrainClassId == potentialMember.TrainClassId).FirstOrDefault();
                        if (potentialMemberOld != null)
                            throw new PartyMemberException("该培训班已经存在相同的学号/工号或身份证号，请核对");
                        if (potentialMemberInDb.IsPrint && CurrentUser.Roles == Role.学院党委)
                        {
                            var noName = "【" + potentialMemberInDb.Name + "-" + potentialMemberInDb.JobNo + "】";
                            throw new PartyMemberException(noName + "已经打印证书，请联系管理员修改");
                        }
                        potentialMemberInDb.ApplicationTime = potentialMember.ApplicationTime;
                        potentialMemberInDb.ActiveApplicationTime = potentialMember.ActiveApplicationTime;
                        potentialMemberInDb.PotentialMemberTime = potentialMember.PotentialMemberTime;
                        //partyActivistInDb.Duty = potentialMember.Duty;
                        potentialMemberInDb.Name = potentialMember.Name;
                        potentialMemberInDb.JobNo = potentialMember.JobNo;
                        potentialMemberInDb.IdNumber = potentialMember.IdNumber;
                        potentialMemberInDb.NationId = potentialMember.NationId;
                        potentialMemberInDb.PartyMemberType = potentialMember.PartyMemberType;
                        potentialMemberInDb.BirthDate = potentialMember.BirthDate;
                        potentialMemberInDb.Phone = potentialMember.Phone;
                        potentialMemberInDb.Department = potentialMember.Department;
                        potentialMemberInDb.Class = potentialMember.Class;
                        potentialMemberInDb.Id = potentialMember.Id;
                        //partyActivistInDb.CreateTime = DateTime.Now;
                        //partyActivistInDb.OperatorId = CurrentUser.Id;
                        //partyActivistInDb.Ordinal = _context.PartyActivists.Count() + 1;
                        //partyActivistInDb.IsDeleted = partyActivist.IsDeleted;
                        potentialMemberInDb.YearTermId = potentialMember.YearTermId;
                        potentialMemberInDb.TrainClassId = potentialMember.TrainClassId;
                        potentialMemberInDb.DepartmentId = potentialMember.DepartmentId;
                        _context.Update(potentialMemberInDb);
                    }
                    else
                    {
                        //partyActivist.Id = Guid.NewGuid();
                        potentialMember.CreateTime = DateTime.Now;
                        potentialMember.OperatorId = CurrentUser.Id;
                        potentialMember.Ordinal = _context.PartyActivists.Count() + 1;
                        var potentialMemberOld = _context.PartyActivists.Where(d => (d.JobNo == potentialMember.JobNo || d.IdNumber == potentialMember.IdNumber) && d.TrainClassId == potentialMember.TrainClassId).FirstOrDefault();
                        if (potentialMemberOld != null)
                            throw new PartyMemberException("该培训班已经存在相同的学号/工号或身份证号，请核对");

                        PotentialTrainResult potentialTrainResult = new PotentialTrainResult
                        {
                            PotentialMemberId = potentialMember.Id,
                            CreateTime = DateTime.Now,
                            OperatorId = CurrentUser.Id,
                            IsDeleted = false,
                            Ordinal = _context.PotentialTrainResults.Count() + 1
                        };
                        _context.Add(potentialMember);
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


        /// <summary>
        /// 导入发展对象
        /// </summary>
        /// <returns></returns>
        public IActionResult Import()
        {
            ProtentialMemberImportViewModel model = new ProtentialMemberImportViewModel
            {
                DepartmentId = CurrentUser.DepartmentId.HasValue ? CurrentUser.DepartmentId.Value : Guid.Empty,
            };
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "42").Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value).OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "42").OrderBy(d => d.Ordinal), "Id", "Name");
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
        public async Task<IActionResult> Import(ProtentialMemberImportViewModel model)
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
                        DataColumn columnErrorMessage = null;
                        if (!tableErrorData.Columns.Contains("错误提示"))
                            columnErrorMessage = tableErrorData.Columns.Add("错误提示", typeof(string));
                        else
                            columnErrorMessage = tableErrorData.Columns["错误提示"];
                        int rowIndex = 0;
                        int successCount = 0;
                        string fieldsTeacher = "姓名,性别,出生年月,民族,所在部门,工号,身份证号,联系电话,提交入党申请时间,确定入党积极分子时间,确定发展对象时间,备注";
                        string fieldsStudent = "姓名,学号,身份证号,性别,出生年月,民族,所在学院,所在班级,联系电话,提交入党申请时间,确定入党积极分子时间,确定发展对象时间,备注";
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
                                PotentialMember potentialMember = new PotentialMember
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
                                    string potentialMemberTimeField = "确定发展对象时间";
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
                                    string potentialMemberTime = row[potentialMemberTimeField].ToString();
                                    string remark = row[remarkField].ToString();
                                    //跳过姓名为空的记录
                                    if (string.IsNullOrEmpty(name)) continue;
                                    name = name.Trim();
                                    id = id.Trim();
                                    empNo = empNo.Trim();
                                    department = department.Trim();
                                    birthday = birthday.Replace(".", "-").Replace("/", "-");
                                    time = time.Replace(".", "-").Replace("/", "-");
                                    confirmTime = confirmTime.Replace(".", "-").Replace("/", "-");
                                    potentialMemberTime = potentialMemberTime.Replace(".", "-").Replace("/", "-");
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
                                    DateTime potentialMemberTimeValue = DateTime.Now;
                                    if (!TryParseDate(potentialMemberTime, out potentialMemberTimeValue))
                                    {
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{potentialMemberTimeField}】日期格式不合法");
                                    }
                                    if (!StringHelper.ValidateIdNumber(id))
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{idField}】不合法");

                                    if (!ValidateImportTeacherNo(empNo))
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{empNoField}】不合法");

                                    nation = nation.Trim();
                                    Nation nationData = _context.Nations.Where(n => n.Name == nation).FirstOrDefault();
                                    if (nationData == null)
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{nationField}】不合法");
                                    Guid nationId = nationData.Id;
                                    //部门只要有包含（两种包含：导入的名称被部门包含，或者导入的名称包含库中的部门名称）
                                    Department departmentData = _context.Departments.Where(d => d.Name.Contains(department) || department.Contains(d.Name)).FirstOrDefault();
                                    if (departmentData == null)
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{departmentField}】不合法");
                                    Guid departmentId = departmentData.Id;
                                    potentialMember.Name = name;
                                    potentialMember.Sex = Sex.Parse<Sex>(sex);
                                    potentialMember.BirthDate = birthdayValue;
                                    potentialMember.NationId = nationId;
                                    potentialMember.DepartmentId = departmentId;
                                    potentialMember.JobNo = empNo;
                                    potentialMember.IdNumber = id;
                                    potentialMember.Phone = phone;
                                    potentialMember.ApplicationTime = timeValue;
                                    potentialMember.ActiveApplicationTime = confirmTimeValue;
                                    potentialMember.PotentialMemberTime = potentialMemberTimeValue;
                                    potentialMember.PartyMemberType = PartyMemberType.教师;
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
                                    string potentialMemberTimeField = "确定发展对象时间";
                                    string remarkField = "备注";
                                    string studentNoField = "学号";
                                    string collegeField = "所在学院";
                                    string classField = "所在班级";
                                    //string titleField = "担任职务";
                                    string name = row[nameField].ToString();
                                    string sex = row[sexField].ToString();
                                    string birthday = row[birthdayField].ToString();
                                    string nation = row[nationField].ToString();
                                    string id = row[idField].ToString();
                                    string phone = row[phoneField].ToString();
                                    string time = row[timeField].ToString();
                                    string confirmTime = row[confirmTimeField].ToString();
                                    string potentialMemberTime = row[potentialMemberTimeField].ToString();
                                    string remark = row[remarkField].ToString();
                                    string studentNo = row[studentNoField].ToString();
                                    string college = row[collegeField].ToString();
                                    string @class = row[classField].ToString();
                                    //string title = row[titleField].ToString();
                                    //跳过姓名为空的记录
                                    if (string.IsNullOrEmpty(name)) continue;
                                    name = name.Trim();
                                    if (!string.IsNullOrEmpty(college))
                                        id = id.Trim();
                                    else
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{idField}】不能为空 ");
                                    if (!string.IsNullOrEmpty(college))
                                        studentNo = studentNo.Trim();
                                    else
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{studentNoField}】不能为空 ");
                                    if (!string.IsNullOrEmpty(college))
                                        college = college.Trim();
                                    else
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{collegeField}】不能为空 ");
                                    birthday = birthday.Replace(".", "-").Replace("/", "-");
                                    time = time.Replace(".", "-").Replace("/", "-");
                                    confirmTime = confirmTime.Replace(".", "-").Replace("/", "-");
                                    potentialMemberTime = potentialMemberTime.Replace(".", "-").Replace("/", "-");
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
                                    DateTime potentialMemberTimeValue = DateTime.Now;
                                    if (!TryParseDate(potentialMemberTime, out potentialMemberTimeValue))
                                    {
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{potentialMemberTimeField}】日期格式不合法");
                                    }
                                    if (!StringHelper.ValidateIdNumber(id))
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{idField}】不合法");
                                    if (model.PartyMemberType == PartyMemberType.研究生)
                                    {
                                        if (!ValidateImportPostgraduateNo(studentNo))
                                            throw new ImportDataErrorException($"第{rowIndex}行数据中的【{studentNoField}】不合法");
                                    }
                                    else if (model.PartyMemberType == PartyMemberType.预科生)
                                    {
                                        if (!ValidateImportPreparatoryStudentNo(studentNo))
                                            throw new ImportDataErrorException($"第{rowIndex}行数据中的【{studentNoField}】不合法");
                                    }
                                    else
                                    {
                                        if (!ValidateImportUndergraduateNo(studentNo))
                                            throw new ImportDataErrorException($"第{rowIndex}行数据中的【{studentNoField}】不合法");
                                    }
                                    nation = nation.Trim();
                                    Nation nationData = _context.Nations.Where(n => n.Name == nation).FirstOrDefault();
                                    if (nationData == null)
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{nationField}】不合法");
                                    Guid nationId = nationData.Id;
                                    //部门只要有包含（两种包含：导入的名称被部门包含，或者导入的名称包含库中的部门名称）
                                    Department departmentData = _context.Departments.Where(d => d.Name.Contains(college) || college.Contains(d.Name)).FirstOrDefault();
                                    if (departmentData == null)
                                        throw new ImportDataErrorException($"第{rowIndex}行数据中的【{collegeField}】不合法");
                                    Guid departmentId = departmentData.Id;
                                    potentialMember.Name = name;
                                    potentialMember.Sex = Sex.Parse<Sex>(sex);
                                    potentialMember.BirthDate = birthdayValue;
                                    potentialMember.NationId = nationId;
                                    potentialMember.DepartmentId = departmentId;
                                    potentialMember.JobNo = studentNo;
                                    potentialMember.IdNumber = id;
                                    potentialMember.Phone = phone;
                                    potentialMember.ApplicationTime = timeValue;
                                    potentialMember.ActiveApplicationTime = confirmTimeValue;
                                    potentialMember.PotentialMemberTime = potentialMemberTimeValue;
                                    potentialMember.Class = @class;
                                    //partyActivist.Duty = title;
                                    potentialMember.PartyMemberType = model.PartyMemberType;
                                }
                                PotentialTrainResult potentialTrainResult = new PotentialTrainResult
                                {
                                    Id = Guid.NewGuid(),
                                    PotentialMemberId = potentialMember.Id,
                                    CreateTime = DateTime.Now,
                                    OperatorId = CurrentUser.Id,
                                    IsDeleted = false,
                                    Ordinal = _context.ActivistTrainResults.Count() + 1
                                };
                                var potentialMemberOld = _context.PotentialMembers.Where(d => d.JobNo == potentialMember.JobNo && d.TrainClassId == potentialMember.TrainClassId).FirstOrDefault();
                                if (potentialMemberOld != null)
                                {
                                    var noName = "【" + potentialMember.Name + "-" + potentialMember.JobNo + "】";
                                    throw new ImportDataErrorException(noName + "该班级中已经存在学号或身份证号相同的学员，请核对");
                                }
                                _context.PotentialMembers.Add(potentialMember);
                                _context.PotentialTrainResults.Add(potentialTrainResult);
                                await _context.SaveChangesAsync();
                                successCount++;
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
                            string fileName = $"发展对象错误数据_{CurrentUser.LoginName}.xlsx";
                            string fileWithPath = $"{basePath}{Path.DirectorySeparatorChar}{fileName}";
                            Stream streamOutExcel = OfficeHelper.ExportExcelByOpenXml(tableErrorData);
                            FileStream outExcelFile = new FileStream(fileWithPath, FileMode.Create, System.IO.FileAccess.Write);
                            byte[] bytes = new byte[streamOutExcel.Length];
                            streamOutExcel.Read(bytes, 0, (int)streamOutExcel.Length);
                            outExcelFile.Write(bytes, 0, bytes.Length);
                            outExcelFile.Close();
                            streamOutExcel.Close();
                            jsonResult.ErrorDataFile = $"PotentialMembers/GetErrorImportData?fileName={fileName}";
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
                jsonResult.Message = "发展对象导入错误";
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
        /// 验证工号和学号
        /// </summary>
        /// <param name="partyActivist"></param>
        public void ValidateJobNo(PotentialMember potentialMember)
        {
            if (!StringHelper.ValidateJobNo(potentialMember.JobNo))
                throw new PartyMemberException("学号/工号不合法");
            int noLength = 10;
            switch (potentialMember.PartyMemberType)
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
                case PartyMemberType.预科生:
                    noLength = 9;
                    break;
            }
            if (potentialMember.JobNo.Trim().Length != noLength)
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
        /// 校验预科生学号(仅导入用）
        /// </summary>
        /// <param name="partyActivist"></param>
        public bool ValidateImportPreparatoryStudentNo(string no)
        {
            if (string.IsNullOrEmpty(no))
                return false;
            if (!StringHelper.ValidateJobNo(no))
                return false;
            int noLength = 9;
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
        private bool PotentialMemberExists(Guid id)
        {
            return _context.PotentialMembers.Any(e => e.Id == id);
        }
    }
}
