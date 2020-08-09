﻿using System;
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
using System.Linq.Expressions;
using PartyMemberManager.Core.Enums;
using PartyMemberManager.Framework.Models.JsonModels;
using Microsoft.AspNetCore.Http;
using System.Xml;

namespace PartyMemberManager.Controllers
{
    [TypeFilter(typeof(Filters.AuthorizeFilter))]
    public class OperatorsController : PartyMemberControllerBase
    {
        public OperatorsController(ILogger<OperatorsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }
        /// <summary>
        /// 数据过滤器，不显示比自己权限高的用户
        /// </summary>
        //public override Expression<Func<Operator, bool>> DataFilter => o => o.Roles <= Roles;
        // GET: Operators
        public async Task<IActionResult> Index(int page = 1)
        {
            if (CurrentUser.Roles == Role.超级管理员)
                return View(await _context.Operators.OrderBy(o => o.Ordinal).GetPagedDataAsync(page));
            else
                return View(await _context.Operators.Where(o => o.Roles != Role.超级管理员).OrderBy(o => o.Ordinal).GetPagedDataAsync(page));
        }

        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatas(int page = 1, int limit = 10)
        {
            JsonResultDatasModel<Operator> jsonResult = new JsonResultDatasModel<Operator>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                if (CurrentUser.Roles == Role.超级管理员)
                {
                    var data = await _context.Set<Operator>().OrderByDescending(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<Operator>().Count();
                    jsonResult.Data = data.Data;
                }
                else
                {
                    var data = await _context.Set<Operator>().Where(o => o.Roles != Role.超级管理员).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                    if (data == null)
                        throw new PartyMemberException("未找到数据");
                    jsonResult.Count = _context.Set<Operator>().Count();
                    jsonResult.Data = data.Data;
                }
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
        // GET: Operators/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var Operator = await _context.Operators
            .SingleOrDefaultAsync(m => m.Id == id);
            if (Operator == null)
            {
                return NotFoundData();
            }

            return View(Operator);
        }

        // GET: Operators/Create
        public IActionResult Create()
        {
            Operator Operator = new Operator();
            Operator.Roles = Role.学院党委;
            return View(Operator);
        }

        // POST: Operators/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="Operator">操作员信息</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("LoginName,Name,Phone,Password,Roles,Enabled,Id,CreateTime,Ordinal")] Operator Operator)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    Operator.Id = Guid.NewGuid();
                    Operator.CreateTime =DateTime.Now;
                    Operator.Ordinal = _context.Operators.Count() + 1;
                    Operator.Password = StringHelper.EncryPassword(Operator.Password);
                    _context.Add(Operator);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OperatorExists(Operator.Id))
                    {
                        return NotFoundData();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (PartyMemberException ex)
                {
                    ModelState.AddModelError(ex.Key, ex.Message);
                }
                catch (Exception ex)
                {
                    ShowAndLogSystemError(ex);
                }
            }
            return View(Operator);
        }

        // GET: Operators/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var Operator = await _context.Operators.SingleOrDefaultAsync(m => m.Id == id);
            if (Operator == null)
            {
                return NotFoundData();
            }
            return View(Operator);
        }

        // POST: Operators/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="Operator">操作员信息</param>
        /// <param name="inpatientAreas">负责病区</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("LoginName,Name,Phone,Password,Roles,Enabled,DoctorType,GroupDoctorId,Id,CreateTime,Ordinal")] Operator Operator)
        {
            if (id != Operator.Id)
            {
                return NotFoundData();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    Operator OperatorInDb = await _context.Operators.FindAsync(id);
                    OperatorInDb.LoginName = Operator.LoginName;
                    OperatorInDb.Name = Operator.Name;
                    OperatorInDb.Phone = Operator.Phone;
                    if (Operator.Password != OperatorInDb.Password)
                        OperatorInDb.Password = StringHelper.EncryPassword(Operator.Password);
                    OperatorInDb.Roles = Operator.Roles;
                    OperatorInDb.Enabled = Operator.Enabled;
                    _context.Update(OperatorInDb);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OperatorExists(Operator.Id))
                    {
                        return NotFoundData();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (PartyMemberException ex)
                {
                    ModelState.AddModelError(ex.Key, ex.Message);
                }
                catch (Exception ex)
                {
                    ShowAndLogSystemError(ex);
                }
            }
            return View(Operator);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Guid id, [Bind("LoginName,Name,Phone,Password,Roles,Enabled,Id,CreateTime,Ordinal")] Operator Operator, Guid[] inpatientAreas)
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
                    Operator OperatorInDb = await _context.Operators.FindAsync(id);
                    if (OperatorInDb.LoginName.ToLower() == "eadmin" || OperatorInDb.LoginName.ToLower() == "erasadmin")
                        if (OperatorInDb.LoginName != Operator.LoginName)
                            throw new PartyMemberException($"{OperatorInDb.LoginName}为内置系统管理员账号，不允许修改该账号的“账号”名");
                    switch (Operator.Roles)
                    {
                        case Role.学校党委:
                            break;
                        case Role.学院党委:
                            break;
                        case Role.系统管理员:
                        case Role.超级管理员:
                            inpatientAreas = new Guid[] { };
                            break;
                    }
                    if (OperatorInDb != null)
                    {
                        if (_context.Operators.Any(o => o.LoginName == Operator.LoginName && o.Id != Operator.Id))
                            throw new PartyMemberException($"登录名[{Operator.LoginName}]已经存在");
                        OperatorInDb.LoginName = Operator.LoginName;
                        OperatorInDb.Name = Operator.Name;
                        OperatorInDb.Phone = Operator.Phone;
                        if (Operator.Password != OperatorInDb.Password)
                            OperatorInDb.Password = StringHelper.EncryPassword(Operator.Password);
                        OperatorInDb.Roles = Operator.Roles;
                        OperatorInDb.Enabled = Operator.Enabled;
                        _context.Update(OperatorInDb);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        if (_context.Operators.Any(o => o.LoginName == Operator.LoginName))
                            throw new PartyMemberException($"登录名[{Operator.LoginName}]已经存在");
                        Operator.Id = Guid.NewGuid();
                        Operator.CreateTime = DateTime.Now;
                        Operator.Password = StringHelper.EncryPassword(Operator.Password);
                        Operator.Ordinal = _context.Operators.Count() + 1;
                        _context.Add(Operator);
                        await _context.SaveChangesAsync();
                    }
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

        /// <summary>
        /// 校验要删除的数据，如果不合法则抛出异常
        /// </summary>
        /// <param name="data"></param>
        //protected override void ValidateDeleteObject(Operator data)
        //{
        //    if (data.Id == CurrentOperator.Id)
        //        throw new PartyMemberException("不能删除自己");
        //    if (data.Roles >= CurrentOperator.Roles)
        //        throw new PartyMemberException($"您无权删除{data.RolesDisplay}");
        //    if (_context.PatientArchives.Any(p => p.DoctorId == data.Id))
        //        throw new PartyMemberException($"该医生有患者，不能删除");
        //    if (_context.NutritionScreens.Any(p => p.OperatorId == data.Id))
        //        throw new PartyMemberException($"该医生与风险筛查表，不能删除");
        //}

        private bool OperatorExists(Guid id)
        {
            return _context.Operators.Any(e => e.Id == id);
        }
    }
}
