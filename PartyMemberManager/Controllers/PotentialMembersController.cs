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
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.Nations = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            if (CurrentUser.Roles == Role.学院党委)
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "42").Where(d => d.DepartmentId == CurrentUser.DepartmentId.Value).OrderBy(d => d.Ordinal), "Id", "Name");
            else
                ViewBag.TrainClasses = new SelectList(_context.TrainClasses.Include(d => d.TrainClassType).Where(d => d.TrainClassType.Code == "42").OrderBy(d => d.Ordinal), "Id", "Name");
            return View(await pMContext.ToListAsync());
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
            if (idList == null)
            {
                return NotFoundData();
            }
            PotentialMember potentialMember = new PotentialMember();
            ViewBag.IdList = idList.TrimEnd(',');
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
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
            ViewBag.Departments = new SelectList(_context.Departments.OrderBy(d => d.Ordinal), "Id", "Name");
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

        [HttpPost]
        public override async Task<IActionResult> Save(PotentialMember potentialMember)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };
            try
            {
                if (string.IsNullOrEmpty(potentialMember.IdList))
                    throw new PartyMemberException("请选择成绩合格的入党积极分子作为发展对象");
                if (potentialMember.YearTermId == Guid.Empty)
                    throw new PartyMemberException("请选择学年/学期");
                if (potentialMember.TrainClassId == Guid.Empty)
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
                    string[] partyAcitvistsId = potentialMember.IdList.Split(",");
                    foreach (var item in partyAcitvistsId)
                    {
                        Guid partyActivistId = Guid.Parse(item);
                        var potentialMemberOld = _context.PotentialMembers.Where(d => d.PartyActivistId == partyActivistId).FirstOrDefault();
                        if (potentialMemberOld.PartyActivistId != partyActivistId)
                        {
                            var partyActivist = _context.PartyActivists.Where(d => d.Id == partyActivistId).FirstOrDefault();
                            PotentialMember potentialMemberNew = new PotentialMember
                            {
                                ApplicationTime = partyActivist.ApplicationTime,
                                ActiveApplicationTime = partyActivist.ActiveApplicationTime,
                                Name = partyActivist.Name,
                                JobNo = partyActivist.JobNo,
                                IdNumber = partyActivist.IdNumber,
                                PartyMemberType = partyActivist.PartyMemberType,
                                BirthDate = partyActivist.BirthDate,
                                Phone = partyActivist.Phone,
                                Department = partyActivist.Department,
                                Class = partyActivist.Class,
                                Id = partyActivist.Id,
                                CreateTime = DateTime.Now,
                                OperatorId = CurrentUser.Id,
                                Ordinal = _context.PotentialMembers.Count() + 1,
                                IsDeleted = partyActivist.IsDeleted,
                                DepartmentId = partyActivist.DepartmentId,
                                ///后期选择的信息
                                TrainClassId = potentialMember.TrainClassId,
                                PotentialMemberTime = potentialMember.PotentialMemberTime
                            };
                            _context.Add(potentialMemberNew);
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
