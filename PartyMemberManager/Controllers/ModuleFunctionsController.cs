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
    public class ModuleFunctionsController : PartyMemberControllerBase
    {

        public ModuleFunctionsController(ILogger<ModuleFunctionsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context,accessor)
        {
        }

        // GET: Modules
        public async Task<IActionResult> Index(int page = 1)
        {
            var PMContext = _context.Modules.Where(m => m.ParentModuleId == null).Include(m => m.ParentModule).OrderBy(m => m.Ordinal);
            return View(await PMContext.ToListAsync());
        }

        // GET: Modules/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var @module = await _context.Modules
                    .Include(m => m.ParentModule)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (@module == null)
            {
                return NotFoundData();
            }

            return View(@module);
        }

        // GET: Modules/Create
        public IActionResult Create()
        {
            Module @module = new Module();
            ViewData["ParentModuleId"] = new SelectList(_context.Modules, "Id", "Id");
            return View(@module);
        }

        // POST: Modules/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Area,Controller,Action,Roles,ParentModuleId,Id,CreateTime,OperatorId,Ordinal")] Module @module)
        {
            if (ModelState.IsValid)
            {
                //@module.Id = Guid.NewGuid();
                _context.Add(@module);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["ParentModuleId"] = new SelectList(_context.Modules, "Id", "Id", @module.ParentModuleId);
            return View(@module);
        }

        // GET: Modules/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var @module = await _context.Modules.SingleOrDefaultAsync(m => m.Id == id);
            if (@module == null)
            {
                return NotFoundData();
            }
            ViewData["ParentModuleId"] = new SelectList(_context.Modules, "Id", "Id", @module.ParentModuleId);
            return View(@module);
        }

        // POST: Modules/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Name,Area,Controller,Action,Roles,ParentModuleId,Id,CreateTime,OperatorId,Ordinal")] Module @module)
        {
            if (id != @module.Id)
            {
                return NotFoundData();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Module @moduleInDb = await _context.Modules.FindAsync(id);
                    @moduleInDb.Name = @module.Name;
                    @moduleInDb.Area = @module.Area;
                    @moduleInDb.Controller = @module.Controller;
                    @moduleInDb.Action = @module.Action;
                    @moduleInDb.Roles = @module.Roles;
                    @moduleInDb.ParentModuleId = @module.ParentModuleId;
                    @moduleInDb.ParentModule = @module.ParentModule;
                    @moduleInDb.ChildModules = @module.ChildModules;
                    @moduleInDb.Id = @module.Id;
                    @moduleInDb.CreateTime = @module.CreateTime;
                    @moduleInDb.OperatorId = @module.OperatorId;
                    @moduleInDb.Ordinal = @module.Ordinal;
                    _context.Update(@moduleInDb);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ModuleExists(@module.Id))
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
            ViewData["ParentModuleId"] = new SelectList(_context.Modules, "Id", "Id", @module.ParentModuleId);
            return View(@module);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save([Bind("Name,Area,Controller,Action,Roles,ParentModuleId,Id,CreateTime,OperatorId,Ordinal")] Module @module)
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
                    Module @moduleInDb = await _context.Modules.FindAsync(@module.Id);
                    if (@moduleInDb != null)
                    {
                        @moduleInDb.Name = @module.Name;
                        @moduleInDb.Area = @module.Area;
                        @moduleInDb.Controller = @module.Controller;
                        @moduleInDb.Action = @module.Action;
                        @moduleInDb.Roles = @module.Roles;
                        @moduleInDb.ParentModuleId = @module.ParentModuleId;
                        @moduleInDb.ParentModule = @module.ParentModule;
                        @moduleInDb.ChildModules = @module.ChildModules;
                        @moduleInDb.Id = @module.Id;
                        @moduleInDb.CreateTime = @module.CreateTime;
                        @moduleInDb.OperatorId = @module.OperatorId;
                        @moduleInDb.Ordinal = @module.Ordinal;
                        _context.Update(@moduleInDb);
                    }
                    else
                    {
                        //@module.Id = Guid.NewGuid();
                        _context.Add(@module);
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


        private bool ModuleExists(Guid id)
        {
            return _context.Modules.Any(e => e.Id == id);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRights(string[] moduleRoles)
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
                    Dictionary<Guid, List<Role>> moduleRoleList = new Dictionary<Guid, List<Role>>();
                    foreach (string moduleRole in moduleRoles)
                    {
                        string[] moduleRoleStrings = moduleRole.Split(',');
                        Guid moduleId = Guid.Parse(moduleRoleStrings[0]);
                        Role role = Enum.Parse<Role>(moduleRoleStrings[1]);
                        if (moduleRoleList.ContainsKey(moduleId))
                            moduleRoleList[moduleId].Add(role);
                        else
                        {
                            moduleRoleList.Add(moduleId, new List<Role>());
                            moduleRoleList[moduleId].Add(role);
                        }
                    }
                    foreach (Guid key in moduleRoleList.Keys)
                    {
                        Module module = await _context.Modules.FindAsync(key);
                        module.Roles = (Role)moduleRoleList[key].Sum(r => (int)r);
                        List<Module> childModules = await _context.Modules.Include(m => m.ChildModules).Where(m => m.ParentModuleId == module.Id).ToListAsync();
                        //需要同时更新二级、三级子模块
                        foreach(Module childModule in childModules)
                        {
                            childModule.Roles = module.Roles;
                            if(childModule.ChildModules!=null)
                            {
                                foreach(Module childSubModule in childModule.ChildModules)
                                {
                                    childSubModule.Roles = module.Roles;
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
    }
}
