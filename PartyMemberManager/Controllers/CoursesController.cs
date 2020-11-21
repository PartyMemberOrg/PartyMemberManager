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
            return View(await _context.Courses.Include(d => d.Nation).OrderBy(s => s.Ordinal).GetPagedDataAsync(page));
        }
        /// <summary>
        /// 重载获取数据函数，主要史需要Include
        /// </summary>
        /// <param name="page"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public override async Task<IActionResult> GetDatas(int page = 1, int limit = 10)
        {
            JsonResultDatasModel<Course> jsonResult = new JsonResultDatasModel<Course>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var data = await _context.Set<Course>().Include(d => d.Nation).Where(DataFilter).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Count = _context.Set<Course>().Where(DataFilter).Count();
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
        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(CourseType courseType, string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<Course> jsonResult = new JsonResultDatasModel<Course>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<Course>();
                if (keyword != null)
                {
                    filter = filter.And(d => d.Name.Contains(keyword));
                }
                if ((int)courseType > 0)
                {
                    filter = filter.And(d => d.CourseType == courseType);
                }
                var data = await _context.Set<Course>()
                    .Where(filter)
                    .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Count = _context.Set<Course>().Where(filter).Count();
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
        //GET: Courses/Details/5
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
            ViewBag.NationId = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
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
            ViewBag.NationId = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
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
        public async Task<IActionResult> Save([Bind("CourseType,Name,Organization,Rank,NationId,Phone,StartTime,Place,CourseName,CourseHour,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] Course course,IFormFile filePPT,IFormFile fileWord)
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
                if (filePPT != null)
                {
                    Stream stream = filePPT.OpenReadStream();
                    var filePath = Path.GetTempFileName();
                    var fileStream = System.IO.File.Create(filePath);
                    await filePPT.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                    fileStream.Close();
                }
                if (fileWord!= null)
                {
                    Stream stream = fileWord.OpenReadStream();
                    var filePath = Path.GetTempFileName();
                    var fileStream = System.IO.File.Create(filePath);
                    await fileWord.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                    fileStream.Close();
                }
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
