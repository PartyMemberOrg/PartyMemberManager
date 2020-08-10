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
    public class PartyActivistsController : PartyMemberDataControllerBase<PartyActivist>
    {

        public PartyActivistsController(ILogger<PartyActivistsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: PartyActivists
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _context.PartyActivists.OrderBy(p => p.Ordinal).GetPagedDataAsync(page));
        }

        // GET: PartyActivists/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var partyActivist = await _context.PartyActivists
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
            return View(partyActivist);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("Name,StudentNo,IDNumber,Sex,BirthDate,Nationality,Phone,Class,ApplicationTime,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] PartyActivist partyActivist)
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
                    if (partyActivistInDb != null)
                    {
                        partyActivistInDb.Name = partyActivist.Name;
                        partyActivistInDb.StudentNo = partyActivist.StudentNo;
                        partyActivistInDb.IDNumber = partyActivist.IDNumber;
                        partyActivistInDb.Sex = partyActivist.Sex;
                        partyActivistInDb.BirthDate = partyActivist.BirthDate;
                        partyActivistInDb.Nationality = partyActivist.Nationality;
                        partyActivistInDb.Phone = partyActivist.Phone;
                        partyActivistInDb.Department = partyActivist.Department;
                        partyActivistInDb.Class = partyActivist.Class;
                        partyActivistInDb.ApplicationTime = partyActivist.ApplicationTime;
                        partyActivistInDb.Id = partyActivist.Id;
                        partyActivistInDb.CreateTime = partyActivist.CreateTime;
                        partyActivistInDb.OperatorId = partyActivist.OperatorId;
                        partyActivistInDb.Ordinal = partyActivist.Ordinal;
                        partyActivistInDb.IsDeleted = partyActivist.IsDeleted;
                        _context.Update(partyActivistInDb);
                    }
                    else
                    {
                        //partyActivist.Id = Guid.NewGuid();
                        _context.Add(partyActivist);
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


        private bool PartyActivistExists(Guid id)
        {
            return _context.PartyActivists.Any(e => e.Id == id);
        }
    }
}
