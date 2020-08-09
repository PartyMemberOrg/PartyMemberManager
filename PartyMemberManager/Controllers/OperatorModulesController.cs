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
using PartyMemberManager.Core.Enums;
using Microsoft.AspNetCore.Http;

namespace PartyMemberManager.Controllers
{
    //[TypeFilter(typeof(Filters.AuthorizeFilter))]
    public class OperatorModulesController : PartyMemberControllerBase
    {

        public OperatorModulesController(ILogger<OperatorModulesController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context,accessor)
        {
        }

        // GET: OperatorModules
        public async Task<IActionResult> Index(int page = 1)
        {
            var pmContext = _context.OperatorModules.Include(o => o.Module).Include(o => o.User);
            ViewData["Modules"] = await _context.Modules.Where(m => m.ParentModuleId == null).OrderBy(m => m.Ordinal).ToListAsync();
            ViewData["Operators"] = await _context.Operators.OrderBy(m => m.Ordinal).ToListAsync();
            return View(await pmContext.ToListAsync());
        }

        // GET: OperatorModules/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var opeartorModule = await _context.OperatorModules
                    .Include(o => o.Module)
                    .Include(o => o.User)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (opeartorModule == null)
            {
                return NotFoundData();
            }

            return View(opeartorModule);
        }

        // GET: OperatorModules/Create
        public IActionResult Create()
        {
            OperatorModule UserModule = new OperatorModule();
            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Operators, "Id", "LoginName");
            return View(UserModule);
        }

        // POST: OperatorModules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,ModuleId,RightType,Id,CreateTime,UserId,Ordinal")] OperatorModule UserModule)
        {
            if (ModelState.IsValid)
            {
                //UserModule.Id = Guid.NewGuid();
                _context.Add(UserModule);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "Id", UserModule.ModuleId);
            ViewData["UserId"] = new SelectList(_context.Operators, "Id", "LoginName", UserModule.UserId);
            return View(UserModule);
        }

        // GET: OperatorModules/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var operatorModule = await _context.OperatorModules.SingleOrDefaultAsync(m => m.Id == id);
            if (operatorModule == null)
            {
                return NotFoundData();
            }
            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "Id", operatorModule.ModuleId);
            ViewData["UserId"] = new SelectList(_context.Operators, "Id", "LoginName", operatorModule.UserId);
            return View(operatorModule);
        }

        // POST: OperatorModules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("UserId,ModuleId,RightType,Id,CreateTime,UserId,Ordinal")] OperatorModule operatorModule)
        {
            if (id != operatorModule.Id)
            {
                return NotFoundData();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    OperatorModule UserModuleInDb = await _context.OperatorModules.FindAsync(id);
                    UserModuleInDb.UserId = operatorModule.UserId;
                    UserModuleInDb.ModuleId = operatorModule.ModuleId;
                    UserModuleInDb.RightType = operatorModule.RightType;
                    UserModuleInDb.User = operatorModule.User;
                    UserModuleInDb.Module = operatorModule.Module;
                    UserModuleInDb.Id = operatorModule.Id;
                    UserModuleInDb.CreateTime = operatorModule.CreateTime;
                    UserModuleInDb.UserId = operatorModule.UserId;
                    UserModuleInDb.Ordinal = operatorModule.Ordinal;
                    _context.Update(UserModuleInDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OperatorModuleExists(operatorModule.Id))
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
            ViewData["ModuleId"] = new SelectList(_context.Modules, "Id", "Id", operatorModule.ModuleId);
            ViewData["UserId"] = new SelectList(_context.Operators, "Id", "LoginName", operatorModule.UserId);
            return View(operatorModule);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([Bind("UserId,ModuleId,RightType,Id,CreateTime,UserId,Ordinal")] OperatorModule UserModule)
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
                    OperatorModule operatorModuleInDb = await _context.OperatorModules.FindAsync(UserModule.Id);
                    if (operatorModuleInDb != null)
                    {
                        operatorModuleInDb.UserId = UserModule.UserId;
                        operatorModuleInDb.ModuleId = UserModule.ModuleId;
                        operatorModuleInDb.RightType = UserModule.RightType;
                        operatorModuleInDb.User = UserModule.User;
                        operatorModuleInDb.Module = UserModule.Module;
                        operatorModuleInDb.Id = UserModule.Id;
                        operatorModuleInDb.CreateTime = UserModule.CreateTime;
                        operatorModuleInDb.UserId = UserModule.UserId;
                        operatorModuleInDb.Ordinal = UserModule.Ordinal;
                        _context.Update(operatorModuleInDb);
                    }
                    else
                    {
                        //UserModule.Id = Guid.NewGuid();
                        _context.Add(UserModule);
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
                if (!string.IsNullOrEmpty(ex.Key))
                    jsonResult.Errors.Add(new ModelError
                    {
                        Key = ex.Key,
                        Message = ex.Message
                    });
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


        private bool OperatorModuleExists(Guid id)
        {
            return _context.OperatorModules.Any(e => e.Id == id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRights(string[] moduleRights)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据保存成功"
            };
            try
            {
                if (ModelState.IsValid)
                {
                    //首先移除之前的全部权限
                    _context.OperatorModules.RemoveRange(await _context.OperatorModules.Where(om => om.RightType == RightType.Grant).ToListAsync());
                    foreach (string moduleRole in moduleRights)
                    {
                        string[] moduleRightStrings = moduleRole.Split(',');
                        Guid UserId = Guid.Parse(moduleRightStrings[0]);
                        Guid moduleId = Guid.Parse(moduleRightStrings[1]);
                        OperatorModule UserModule = new OperatorModule
                        {
                            Id = Guid.NewGuid(),
                            CreateTime =DateTime.Now,
                            UserId = CurrentUser.Id,
                            ModuleId = moduleId,
                            Ordinal = 0,
                            RightType = RightType.Grant
                        };
                        _context.OperatorModules.Add(UserModule);
                        //需要同时更新二级、三级子模块
                        List<Module> childModules = await _context.Modules.Include(m => m.ChildModules).Where(m => m.ParentModuleId == moduleId).ToListAsync();
                        foreach (Module childModule in childModules)
                        {
                            OperatorModule childUserModule = new OperatorModule
                            {
                                Id = Guid.NewGuid(),
                                CreateTime =DateTime.Now,
                                UserId = CurrentUser.Id,
                                ModuleId = childModule.Id,
                                Ordinal = 0,
                                RightType = RightType.Grant,
                            };
                            _context.OperatorModules.Add(childUserModule);
                            if (childModule.ChildModules != null)
                            {
                                foreach (Module childSubModule in childModule.ChildModules)
                                {
                                    OperatorModule childSubUserModule = new OperatorModule
                                    {
                                        Id = Guid.NewGuid(),
                                        CreateTime =DateTime.Now,
                                        OperatorId = CurrentUser.Id,
                                        ModuleId = childSubModule.Id,
                                        Ordinal = 0,
                                        RightType = RightType.Grant,
                                    };
                                    _context.OperatorModules.Add(childSubUserModule);
                                }
                            }
                        }

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
    }
}
