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
using System.IO;
using Newtonsoft.Json;
using System.Data;
using ExcelCore;
using PartyMemberManager.Models;
using PartyMemberManager.Models.PrintViewModel;
using NPOI.SS.Formula.Functions;

namespace PartyMemberManager.Controllers
{
    public class CoursesController : PartyMemberDataControllerBase<Course>
    {

        public CoursesController(ILogger<CoursesController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: Courses
        public async Task<IActionResult> Index(int page = 1)
        {
            return View(await _context.Courses.OrderBy(s => s.Ordinal).GetPagedDataAsync(page));
        }
        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        //public async Task<IActionResult> GetDatasWithFilter(Guid? yearTermId, Guid? departmentId, Guid? trainClassId, string partyMemberType,BatchType batch, string keyword, int page = 1, int limit = 10)
        //{
        //    JsonResultDatasModel<PartyActivist> jsonResult = new JsonResultDatasModel<PartyActivist>
        //    {
        //        Code = 0,
        //        Msg = ""
        //    };

        //    try
        //    {
        //        var filter = PredicateBuilder.True<PartyActivist>();
        //        if (yearTermId != null)
        //        {
        //            filter = filter.And(d => d.YearTermId == yearTermId);
        //        }
        //        if (departmentId != null)
        //        {
        //            filter = filter.And(d => d.DepartmentId == departmentId);
        //        }
        //        if (trainClassId != null)
        //        {
        //            filter = filter.And(d => d.TrainClassId == trainClassId);
        //        }
        //        if (keyword != null)
        //        {
        //            filter = filter.And(d => d.Name.Contains(keyword) || d.JobNo.Contains(keyword));
        //        }
        //        if ((int)batch >0)
        //        {
        //            filter = filter.And(d => d.TrainClass.Batch ==batch);
        //        }
        //        if (partyMemberType != null)
        //        {
        //            filter = filter.And(d => d.PartyMemberType == (PartyMemberType)Enum.Parse(typeof(PartyMemberType), partyMemberType));
        //        }
        //        if (CurrentUser.Roles > Role.学院党委)
        //        {
        //            var data = await _context.Set<PartyActivist>().Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.YearTerm)
        //                .Where(filter)
        //                //.Where(d => d.YearTerm.Enabled == true)
        //                .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
        //            if (data == null)
        //                throw new PartyMemberException("未找到数据");
        //            jsonResult.Count = _context.Set<PartyActivist>().Where(filter).Count();
        //            jsonResult.Data = data.Data;
        //        }
        //        else
        //        {
        //            if (CurrentUser.DepartmentId == null)
        //                throw new PartyMemberException("该用户不合法，请设置该用户所属部门");
        //            var data = await _context.Set<PartyActivist>().Where(filter).Include(d => d.Department).Include(d => d.Nation).Include(d => d.TrainClass).Include(t => t.TrainClass).Include(d => d.YearTerm)
        //                .Where(filter).Where(d => d.YearTerm.Enabled == true)
        //                .Where(d => d.DepartmentId == CurrentUser.DepartmentId)
        //                .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
        //            if (data == null)
        //                throw new PartyMemberException("未找到数据");
        //            jsonResult.Count = _context.Set<PartyActivist>().Where(filter).Count();
        //            jsonResult.Data = data.Data;
        //        }
        //    }

        //    catch (PartyMemberException ex)
        //    {
        //        jsonResult.Code = -1;
        //        jsonResult.Msg = ex.Message;
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, ex.Message);
        //        jsonResult.Code = -1;
        //        jsonResult.Msg = "发生系统错误";
        //    }
        //    return Json(jsonResult);
        //}
        // GET: Courses/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var course = await _context.Courses
            .SingleOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFoundData();
            }

            return View(course);
        }

        // GET: Courses/Create
        public IActionResult Create()
        {
            Course course = new Course();
            return View(course);
        }


        // GET: Courses/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var course = await _context.Courses.SingleOrDefaultAsync(m => m.Id == id);
            if (course == null)
            {
                return NotFoundData();
            }
            return View(course);
        }
        /// <summary>
        /// 删除数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        public override async Task<IActionResult> Delete(Guid? id)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };

            try
            {
                if (id == null)
                    throw new PartyMemberException("未传入删除项目的Id");
                var data = await _context.Set<Course>().SingleOrDefaultAsync(m => m.Id == id);
                if (data == null)
                    throw new PartyMemberException("未找到要删除的数据");
                _context.Set<Course>().Remove(data);
                await _context.SaveChangesAsync();
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("YearTermId,TrainClassId,ApplicationTime,ActiveApplicationTime,Duty,Name,JobNo,IdNumber,Sex,PartyMemberType,BirthDate,NationId,Phone,DepartmentId,Class,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] Course course)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };
            try
            {
                if (course.CourseType.ToString() == "0")
                    throw new PartyMemberException("请选择授课类型");
                if (course.NationId == Guid.Empty || course.NationId == null)
                    throw new PartyMemberException("请选择民族");
                if (ModelState.IsValid)
                {
                    Course courseInDb = await _context.Courses.FindAsync(course.Id);
                    if (courseInDb != null)
                    {
                        courseInDb.CourseType = course.CourseType;
                        courseInDb.Name = course.Name;
                        courseInDb.Organization = course.Organization;
                        courseInDb.Rank = course.Rank;
                        courseInDb.NationId = course.NationId;
                        courseInDb.Phone = course.Phone;
                        courseInDb.StartTime = course.StartTime;
                        courseInDb.Place = course.Place;
                        courseInDb.CourseName = course.CourseName;
                        courseInDb.CourseHour = course.CourseHour;
                        _context.Update(courseInDb);
                    }
                    else
                    {
                        course.CreateTime = DateTime.Now;
                        course.OperatorId = CurrentUser.Id;
                        course.Ordinal = _context.Courses.Count() + 1;
                        _context.Add(course);
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

        private bool CourseExists(Guid id)
        {
            return _context.Courses.Any(e => e.Id == id);
        }
    }
}
