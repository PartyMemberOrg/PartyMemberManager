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
    public class YearTermsController : PartyMemberDataControllerBase<YearTerm>
    {

        public YearTermsController(ILogger<YearTermsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: YearTerms
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _context.YearTerms.OrderByDescending(y => y.StartYear).ThenByDescending(y=>y.Term).GetPagedDataAsync(page));
        }

        // GET: YearTerms/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var yearTerm = await _context.YearTerms
            .SingleOrDefaultAsync(m => m.Id == id);
            if (yearTerm == null)
            {
                return NotFoundData();
            }

            return View(yearTerm);
        }

        // GET: YearTerms/Create
        public IActionResult Create()
        {
            YearTerm yearTerm = new YearTerm
            {
                CreateTime = DateTime.Now,
                Id = Guid.NewGuid(),
                IsDeleted = false,
                StartYear = DateTime.Today.Year,
                Term = Term.第一学期,
                Ordinal = 0
            };
            return View(yearTerm);
        }


        // GET: YearTerms/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var yearTerm = await _context.YearTerms.SingleOrDefaultAsync(m => m.Id == id);
            if (yearTerm == null)
            {
                return NotFoundData();
            }
            return View(yearTerm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("StartYear,Term,Enabled,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] YearTerm yearTerm)
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
                    YearTerm yearTermInDb = await _context.YearTerms.FindAsync(yearTerm.Id);
                    if (yearTermInDb != null)
                    {
                        yearTermInDb.StartYear = yearTerm.StartYear;
                        yearTermInDb.Term = yearTerm.Term;
                        yearTermInDb.Enabled = yearTerm.Enabled;
                        _context.Update(yearTermInDb);
                    }
                    else
                    {
                        if (await _context.YearTerms.AnyAsync(y => y.StartYear == yearTerm.StartYear && y.Term == yearTerm.Term))
                            throw new PartyMemberException($"【{yearTerm.Name}】已经存在");
                        //yearTerm.Id = Guid.NewGuid();
                        yearTerm.CreateTime = DateTime.Now;
                        yearTerm.IsDeleted = false;
                        yearTerm.OperatorId = CurrentUser.Id;
                        yearTerm.Ordinal = _context.YearTerms.Count() + 1;
                        _context.Add(yearTerm);
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
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public override async Task<IActionResult> GetDatas(int page = 1, int limit = 10)
        {
            JsonResultDatasModel<YearTerm> jsonResult = new JsonResultDatasModel<YearTerm>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var data = await _context.Set<YearTerm>().Where(DataFilter).OrderByDescending(y => y.StartYear).ThenByDescending(y => y.Term).GetPagedDataAsync(page, limit);
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Count = _context.Set<YearTerm>().Where(DataFilter).Count();
                jsonResult.Data = data.Data;
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
        private bool YearTermExists(Guid id)
        {
            return _context.YearTerms.Any(e => e.Id == id);
        }
    }
}
