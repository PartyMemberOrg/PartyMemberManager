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
    public class PartySchoolsController : PartyMemberDataControllerBase<PartySchool>
    {

        public PartySchoolsController(ILogger<PartySchoolsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: PartySchools
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _context.PartySchools.OrderBy(p => p.Ordinal).GetPagedDataAsync(page));
        }

        // GET: PartySchools/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var partySchool = await _context.PartySchools
            .SingleOrDefaultAsync(m => m.Id == id);
            if (partySchool == null)
            {
                return NotFoundData();
            }

            return View(partySchool);
        }

        // GET: PartySchools/Create
        public IActionResult Create()
        {
            PartySchool partySchool = new PartySchool();
            return View(partySchool);
        }

        // POST: PartySchools/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Code,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PartySchool partySchool)
        {
            if (ModelState.IsValid)
            {
                //partySchool.Id = Guid.NewGuid();
                _context.Add(partySchool);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(partySchool);
        }

        // GET: PartySchools/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var partySchool = await _context.PartySchools.SingleOrDefaultAsync(m => m.Id == id);
            if (partySchool == null)
            {
                return NotFoundData();
            }
            return View(partySchool);
        }

        // POST: PartySchools/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Name,Code,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PartySchool partySchool)
        {
            if (id != partySchool.Id)
            {
                return NotFoundData();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    PartySchool partySchoolInDb = await _context.PartySchools.FindAsync(id);
                    partySchoolInDb.Name = partySchool.Name;
                    partySchoolInDb.Code = partySchool.Code;
                    partySchoolInDb.Id = partySchool.Id;
                    partySchoolInDb.CreateTime = partySchool.CreateTime;
                    partySchoolInDb.OperatorId = partySchool.OperatorId;
                    partySchoolInDb.Ordinal = partySchool.Ordinal;
                    partySchoolInDb.IsDeleted = partySchool.IsDeleted;
                    _context.Update(partySchoolInDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PartySchoolExists(partySchool.Id))
                    {
                        return NotFoundData();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(partySchool);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("Name,Code,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PartySchool partySchool)
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
                    PartySchool partySchoolInDb = await _context.PartySchools.FindAsync(partySchool.Id);
                    if (partySchoolInDb != null)
                    {
                        partySchoolInDb.Name = partySchool.Name;
                        partySchoolInDb.Code = partySchool.Code;
                        partySchoolInDb.Id = partySchool.Id;
                        partySchoolInDb.CreateTime = partySchool.CreateTime;
                        partySchoolInDb.OperatorId = partySchool.OperatorId;
                        partySchoolInDb.Ordinal = partySchool.Ordinal;
                        partySchoolInDb.IsDeleted = partySchool.IsDeleted;
                        _context.Update(partySchoolInDb);
                    }
                    else
                    {
                        //partySchool.Id = Guid.NewGuid();
                        _context.Add(partySchool);
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


        private bool PartySchoolExists(Guid id)
        {
            return _context.PartySchools.Any(e => e.Id == id);
        }
    }
}
