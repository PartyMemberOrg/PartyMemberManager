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
        public IActionResult Create()
        {
            PotentialMember potentialMember = new PotentialMember();
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Code");
            ViewData["NationId"] = new SelectList(_context.Nations, "Id", "Code");
            ViewData["TrainClassId"] = new SelectList(_context.TrainClasses, "Id", "Name");
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
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Code", potentialMember.DepartmentId);
            ViewData["NationId"] = new SelectList(_context.Nations, "Id", "Code", potentialMember.NationId);
            ViewData["TrainClassId"] = new SelectList(_context.TrainClasses, "Id", "Name", potentialMember.TrainClassId);
            return View(potentialMember);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("PotentialMemberTime,TrainClassId,Name,JobNo,IdNumber,Sex,PartyMemberType,BirthDate,NationId,Phone,DepartmentId,Class,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PotentialMember potentialMember)
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
                    PotentialMember potentialMemberInDb = await _context.PotentialMembers.FindAsync(potentialMember.Id);
                    if (potentialMemberInDb != null)
                    {
                        potentialMemberInDb.PotentialMemberTime = potentialMember.PotentialMemberTime;
                        potentialMemberInDb.TrainClassId = potentialMember.TrainClassId;
                        potentialMemberInDb.TrainClass = potentialMember.TrainClass;
                        potentialMemberInDb.TrainClassDisplay = potentialMember.TrainClassDisplay;
                        potentialMemberInDb.TrainClassTermDisplay = potentialMember.TrainClassTermDisplay;
                        potentialMemberInDb.TrainClassYearDisplay = potentialMember.TrainClassYearDisplay;
                        potentialMemberInDb.Name = potentialMember.Name;
                        potentialMemberInDb.JobNo = potentialMember.JobNo;
                        potentialMemberInDb.IdNumber = potentialMember.IdNumber;
                        potentialMemberInDb.Sex = potentialMember.Sex;
                        potentialMemberInDb.SexDisplay = potentialMember.SexDisplay;
                        potentialMemberInDb.PartyMemberType = potentialMember.PartyMemberType;
                        potentialMemberInDb.PartyMemberTypeDisplay = potentialMember.PartyMemberTypeDisplay;
                        potentialMemberInDb.BirthDate = potentialMember.BirthDate;
                        potentialMemberInDb.NationId = potentialMember.NationId;
                        potentialMemberInDb.Nation = potentialMember.Nation;
                        potentialMemberInDb.NationDisplay = potentialMember.NationDisplay;
                        potentialMemberInDb.Phone = potentialMember.Phone;
                        potentialMemberInDb.DepartmentId = potentialMember.DepartmentId;
                        potentialMemberInDb.Department = potentialMember.Department;
                        potentialMemberInDb.DepartmentDisplay = potentialMember.DepartmentDisplay;
                        potentialMemberInDb.Class = potentialMember.Class;
                        potentialMemberInDb.Id = potentialMember.Id;
                        potentialMemberInDb.CreateTime = potentialMember.CreateTime;
                        potentialMemberInDb.OperatorId = potentialMember.OperatorId;
                        potentialMemberInDb.Ordinal = potentialMember.Ordinal;
                        potentialMemberInDb.IsDeleted = potentialMember.IsDeleted;
                        _context.Update(potentialMemberInDb);
                    }
                    else
                    {
                        //potentialMember.Id = Guid.NewGuid();
                        _context.Add(potentialMember);
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


        private bool PotentialMemberExists(Guid id)
        {
            return _context.PotentialMembers.Any(e => e.Id == id);
        }
    }
}
