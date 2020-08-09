using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PartyMemberManager.Framework.Controllers;
using EntityExtension;
using PartyMemberManager.Dal;
using PartyMemberManager.Dal.Entities;
using PartyMemberManager.Core.Helpers;
using PartyMemberManager.Core.Exceptions;
using Microsoft.Extensions.Logging;
using PartyMemberManager.Framework.Models.JsonModels;
using PartyMemberManager.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace PartyMemberManager.Controllers
{
    public class LogsController : PartyMemberControllerBase
    {

        public LogsController(ILogger<LogsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context,accessor)
        {
        }

        // GET: Logs
        public IActionResult Index(int page = 1)
        {
            string sessionKey = this.GetType().Name + "_GetDatasWithFilter";
            DateTime today = DateTime.Today;
            DateTime start = today;
            start = start.AddDays(-30);
            LogIndexViewModel model = new LogIndexViewModel
            {
                DateStart = start,
                DateEnd = DateTime.Today,
                LogLevel = null,
                CategoryName = null,
                Message = null
            };
            //从缓存中提取
            if (HttpContext.Session.Keys.Contains(sessionKey))
            {
                string sessionValue = HttpContext.Session.GetString(sessionKey);
                if (!string.IsNullOrEmpty(sessionValue))
                {
                    QueryParameterSessionModel<LogIndexViewModel> sessionModel = JsonConvert.DeserializeObject<QueryParameterSessionModel<LogIndexViewModel>>(sessionValue);
                    model.DateStart = sessionModel.QueryParameter.DateStart;
                    model.DateEnd = sessionModel.QueryParameter.DateEnd;
                    model.LogLevel = sessionModel.QueryParameter.LogLevel;
                    model.CategoryName = sessionModel.QueryParameter.CategoryName;
                    model.Message = sessionModel.QueryParameter.Message;
                    page = sessionModel.Page;
                }
            }
            return View(model);
        }

        // GET: Logs/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var log = await _context.Logs
            .SingleOrDefaultAsync(m => m.Id == id);
            if (log == null)
            {
                return NotFoundData();
            }

            return View(log);
        }

        // GET: Logs/Create
        public IActionResult Create()
        {
            Log log = new Log();
            return View(log);
        }

        // POST: Logs/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LogLevel,CategoryName,Message,User,Id,CreateTime,OperatorId,Ordinal")] Log log)
        {
            if (ModelState.IsValid)
            {
                //log.Id = Guid.NewGuid();
                _context.Add(log);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(log);
        }

        // GET: Logs/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var log = await _context.Logs.SingleOrDefaultAsync(m => m.Id == id);
            if (log == null)
            {
                return NotFoundData();
            }
            return View(log);
        }

        // POST: Logs/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("LogLevel,CategoryName,Message,User,Id,CreateTime,OperatorId,Ordinal")] Log log)
        {
            if (id != log.Id)
            {
                return NotFoundData();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Log logInDb = await _context.Logs.FindAsync(id);
                    logInDb.LogLevel = log.LogLevel;
                    logInDb.CategoryName = log.CategoryName;
                    logInDb.Message = log.Message;
                    logInDb.User = log.User;
                    logInDb.Id = log.Id;
                    logInDb.CreateTime = log.CreateTime;
                    logInDb.OperatorId = log.OperatorId;
                    logInDb.Ordinal = log.Ordinal;
                    _context.Update(logInDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LogExists(log.Id))
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
            return View(log);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([Bind("LogLevel,CategoryName,Message,User,Id,CreateTime,OperatorId,Ordinal")] Log log)
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
                    Log logInDb = await _context.Logs.FindAsync(log.Id);
                    if (logInDb != null)
                    {
                        logInDb.LogLevel = log.LogLevel;
                        logInDb.CategoryName = log.CategoryName;
                        logInDb.Message = log.Message;
                        logInDb.User = log.User;
                        logInDb.Id = log.Id;
                        logInDb.CreateTime = log.CreateTime;
                        logInDb.OperatorId = log.OperatorId;
                        logInDb.Ordinal = log.Ordinal;
                        _context.Update(logInDb);
                    }
                    else
                    {
                        //log.Id = Guid.NewGuid();
                        _context.Add(log);
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


        private bool LogExists(Guid id)
        {
            return _context.Logs.Any(e => e.Id == id);
        }

        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(LogIndexViewModel model, int page = 1, int limit = 10)
        {
            string sessionKey = this.GetType().Name + "_GetDatasWithFilter";
            JsonResultDatasModel<Log> jsonResult = new JsonResultDatasModel<Log>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                if (model.IsPost)
                {
                    //缓存查询条件和页码
                    QueryParameterSessionModel<LogIndexViewModel> sessionModel = new QueryParameterSessionModel<LogIndexViewModel>();
                    sessionModel.QueryParameter = model;
                    sessionModel.Page = page;
                    sessionModel.Limit = limit;
                    HttpContext.Session.SetString(sessionKey, JsonConvert.SerializeObject(sessionModel));
                }
                else
                {
                    //从缓存中提取
                    if (HttpContext.Session.Keys.Contains(sessionKey))
                    {
                        string sessionValue = HttpContext.Session.GetString(sessionKey);
                        if (!string.IsNullOrEmpty(sessionValue))
                        {
                            QueryParameterSessionModel<LogIndexViewModel> sessionModel = JsonConvert.DeserializeObject<QueryParameterSessionModel<LogIndexViewModel>>(sessionValue);
                            model.DateStart = sessionModel.QueryParameter.DateStart;
                            model.DateEnd = sessionModel.QueryParameter.DateEnd;
                            model.LogLevel = sessionModel.QueryParameter.LogLevel;
                            model.CategoryName = sessionModel.QueryParameter.CategoryName;
                            model.Message = sessionModel.QueryParameter.Message;
                            page = sessionModel.Page;
                            limit = sessionModel.Limit;
                        }
                    }
                }

                var where = PredicateBuilder.True<Log>();
                if (model.DateRange != null)
                {
                    DateTime endDate = model.DateEnd.Value.AddDays(1);
                    where = where.And(d => d.CreateTime >= model.DateStart && d.CreateTime < endDate);
                }
                if (model.LogLevel != null)
                    where = where.And(d => d.LogLevel == model.LogLevel);
                if (model.CategoryName != null)
                    where = where.And(d => d.CategoryName.Contains(model.CategoryName));
                if (model.Message != null)
                    where = where.And(d => d.Message.Contains(model.Message));
                var data = await _context.Logs
                    .Where(where)
                    .OrderByDescending(o => o.CreateTime).GetPagedDataAsync(page, limit);
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Count = _context.Logs.Where(where).Count();
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
    }
}
