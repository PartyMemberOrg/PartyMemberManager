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
    public class NationsController : PartyMemberDataControllerBase<Nation>
    {

        public NationsController(ILogger<NationsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: Nations
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _context.Nations.OrderBy(n => n.Ordinal).GetPagedDataAsync(page));
        }

        // GET: Nations/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var nation = await _context.Nations
            .SingleOrDefaultAsync(m => m.Id == id);
            if (nation == null)
            {
                return NotFoundData();
            }

            return View(nation);
        }

        // GET: Nations/Create
        public IActionResult Create()
        {
            Nation nation = new Nation();
            return View(nation);
        }

        // POST: Nations/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Code,Name,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] Nation nation)
        {
            if (ModelState.IsValid)
            {
                //nation.Id = Guid.NewGuid();
                _context.Add(nation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(nation);
        }

        // GET: Nations/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var nation = await _context.Nations.SingleOrDefaultAsync(m => m.Id == id);
            if (nation == null)
            {
                return NotFoundData();
            }
            return View(nation);
        }

        // POST: Nations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Code,Name,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] Nation nation)
        {
            if (id != nation.Id)
            {
                return NotFoundData();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Nation nationInDb = await _context.Nations.FindAsync(id);
                    nationInDb.Code = nation.Code;
                    nationInDb.Name = nation.Name;
                    nationInDb.Id = nation.Id;
                    nationInDb.CreateTime = DateTime.Now;
                    nationInDb.OperatorId = CurrentUser.Id;
                    nationInDb.Ordinal = _context.Nations.Count()+1;
                    nationInDb.IsDeleted = nation.IsDeleted;
                    _context.Update(nationInDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NationExists(nation.Id))
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
            return View(nation);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("Code,Name,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] Nation nation)
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
                    Nation nationInDb = await _context.Nations.FindAsync(nation.Id);
                    if (nationInDb != null)
                    {
                        nationInDb.Code = nation.Code;
                        nationInDb.Name = nation.Name;
                        nationInDb.Id = nation.Id;
                        nationInDb.CreateTime = DateTime.Now;
                        nationInDb.OperatorId = CurrentUser.Id;
                        nationInDb.Ordinal = _context.Nations.Count() + 1;
                        nationInDb.IsDeleted = nation.IsDeleted;
                        _context.Update(nationInDb);
                    }
                    else
                    {
                        //nation.Id = Guid.NewGuid();
                        nation.CreateTime = DateTime.Now;
                        nation.OperatorId = CurrentUser.Id;
                        nation.Ordinal = _context.Nations.Count() + 1;
                        _context.Add(nation);
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


        private bool NationExists(Guid id)
        {
            return _context.Nations.Any(e => e.Id == id);
        }
    }
}
