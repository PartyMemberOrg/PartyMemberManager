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
    public class DepartmentsController : PartyMemberDataControllerBase<Department>
    {

        public DepartmentsController(ILogger<DepartmentsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: Departments
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _context.Departments.OrderBy(d => d.Ordinal).GetPagedDataAsync(page));
        }

        // GET: Departments/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var department = await _context.Departments
            .SingleOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFoundData();
            }

            return View(department);
        }

        // GET: Departments/Create
        public IActionResult Create()
        {
            Department department = new Department();
            return View(department);
        }

        // GET: Departments/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var department = await _context.Departments.SingleOrDefaultAsync(m => m.Id == id);
            if (department == null)
            {
                return NotFoundData();
            }
            return View(department);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("Name,Password,SchoolAreas,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] Department department)
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
                    Department departmentInDb = await _context.Departments.FindAsync(department.Id);
                    if (departmentInDb != null)
                    {
                        departmentInDb.Name = department.Name;
                        departmentInDb.SchoolArea = department.SchoolArea;
                        departmentInDb.Id = department.Id;
                        departmentInDb.CreateTime = department.CreateTime;
                        departmentInDb.OperatorId = department.OperatorId;
                        departmentInDb.Ordinal = department.Ordinal;
                        departmentInDb.IsDeleted = department.IsDeleted;
                        _context.Update(departmentInDb);
                    }
                    else
                    {
                        //department.Id = Guid.NewGuid();
                        _context.Add(department);
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


        private bool DepartmentExists(Guid id)
        {
            return _context.Departments.Any(e => e.Id == id);
        }
    }
}
