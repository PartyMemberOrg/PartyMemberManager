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
                        .Where(filter).Where(d => d.YearTerm.Enabled == true)
                        .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PotentialMember>().Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    if (CurrentUser.DepartmentId == null)
                        throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
                    var data = await _context.Set<PotentialMember>().Where(filter).Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.TrainClass).Include(d => d.YearTerm)
                        .Where(filter).Where(d => d.YearTerm.Enabled == true)
                        .Where(d => d.DepartmentId == CurrentUser.DepartmentId)
                        .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<PotentialMember>().Count();
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
                    potentialMemberInDb.PotentialMemberTime = potentialMember.PotentialMemberTime;
                    potentialMemberInDb.TrainClassId = potentialMember.TrainClassId;
                    potentialMemberInDb.CreateTime = DateTime.Now;
                    potentialMemberInDb.OperatorId = CurrentUser.Id;
                    potentialMemberInDb.Ordinal = _context.PotentialMembers.Count() + 1;
                    potentialMemberInDb.IsDeleted = potentialMember.IsDeleted;
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
                        var activistTrainResult = await _context.ActivistTrainResults.FindAsync(activistTrainResultId);
                        var potentialMemberOld = _context.PotentialMembers.Where(d => d.PartyActivistId == activistTrainResult.PartyActivistId).FirstOrDefault();
                        
                        if (potentialMemberOld==null)
                        {
                            var partyActivist = await _context.PartyActivists.FindAsync(activistTrainResult.PartyActivistId);
                            PotentialMember potentialMemberNew = new PotentialMember
                            {
                                ApplicationTime = partyActivist.ApplicationTime,
                                ActiveApplicationTime = partyActivist.ActiveApplicationTime,
                                Name = partyActivist.Name,
                                JobNo = partyActivist.JobNo,
                                IdNumber = partyActivist.IdNumber,
                                NationId=partyActivist.NationId,
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
                                PartyActivistId= partyActivist.Id,
                                Sex=partyActivist.Sex,
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
