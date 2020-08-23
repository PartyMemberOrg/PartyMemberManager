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
    public class SchoolCadreTrainsController : PartyMemberDataControllerBase<SchoolCadreTrain>
    {

        public SchoolCadreTrainsController(ILogger<SchoolCadreTrainsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: SchoolCadreTrains
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.SchoolCadreTrains.Include(s => s.Department).Include(s => s.TrainClass).Include(s => s.YearTerm);
            return View(await pMContext.ToListAsync());
        }

        // GET: SchoolCadreTrains/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var schoolCadreTrain = await _context.SchoolCadreTrains
                    .Include(s => s.Department)
                    .Include(s => s.TrainClass)
                    .Include(s => s.YearTerm)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (schoolCadreTrain == null)
            {
                return NotFoundData();
            }

            return View(schoolCadreTrain);
        }

        // GET: SchoolCadreTrains/Create
        public IActionResult Create()
        {
            SchoolCadreTrain schoolCadreTrain = new SchoolCadreTrain();
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Code");
            ViewData["TrainClassId"] = new SelectList(_context.TrainClasses, "Id", "Name");
            ViewData["YearTermId"] = new SelectList(_context.YearTerms, "Id", "Id");
            return View(schoolCadreTrain);
        }


        // GET: SchoolCadreTrains/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var schoolCadreTrain = await _context.SchoolCadreTrains.SingleOrDefaultAsync(m => m.Id == id);
            if (schoolCadreTrain == null)
            {
                return NotFoundData();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Code", schoolCadreTrain.DepartmentId);
            ViewData["TrainClassId"] = new SelectList(_context.TrainClasses, "Id", "Name", schoolCadreTrain.TrainClassId);
            ViewData["YearTermId"] = new SelectList(_context.YearTerms, "Id", "Id", schoolCadreTrain.YearTermId);
            return View(schoolCadreTrain);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("TrainClassId,YearTermId,Organizer,TrainOrganizational,TrainTime,TrainDuration,Name,Phone,IDNumber,Sex,DepartmentId,Post,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] SchoolCadreTrain schoolCadreTrain)
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
                    SchoolCadreTrain schoolCadreTrainInDb = await _context.SchoolCadreTrains.FindAsync(schoolCadreTrain.Id);
                    if (schoolCadreTrainInDb != null)
                    {
                        schoolCadreTrainInDb.TrainClassId = schoolCadreTrain.TrainClassId;
                        schoolCadreTrainInDb.TrainClass = schoolCadreTrain.TrainClass;
                        schoolCadreTrainInDb.YearTermId = schoolCadreTrain.YearTermId;
                        schoolCadreTrainInDb.YearTerm = schoolCadreTrain.YearTerm;
                        schoolCadreTrainInDb.Organizer = schoolCadreTrain.Organizer;
                        schoolCadreTrainInDb.TrainOrganizational = schoolCadreTrain.TrainOrganizational;
                        schoolCadreTrainInDb.TrainTime = schoolCadreTrain.TrainTime;
                        schoolCadreTrainInDb.TrainDuration = schoolCadreTrain.TrainDuration;
                        schoolCadreTrainInDb.Name = schoolCadreTrain.Name;
                        schoolCadreTrainInDb.Phone = schoolCadreTrain.Phone;
                        schoolCadreTrainInDb.IDNumber = schoolCadreTrain.IDNumber;
                        schoolCadreTrainInDb.Sex = schoolCadreTrain.Sex;
                        schoolCadreTrainInDb.DepartmentId = schoolCadreTrain.DepartmentId;
                        schoolCadreTrainInDb.Department = schoolCadreTrain.Department;
                        schoolCadreTrainInDb.Post = schoolCadreTrain.Post;
                        schoolCadreTrainInDb.Id = schoolCadreTrain.Id;
                        schoolCadreTrainInDb.CreateTime = schoolCadreTrain.CreateTime;
                        schoolCadreTrainInDb.OperatorId = schoolCadreTrain.OperatorId;
                        schoolCadreTrainInDb.Ordinal = schoolCadreTrain.Ordinal;
                        schoolCadreTrainInDb.IsDeleted = schoolCadreTrain.IsDeleted;
                        _context.Update(schoolCadreTrainInDb);
                    }
                    else
                    {
                        //schoolCadreTrain.Id = Guid.NewGuid();
                        _context.Add(schoolCadreTrain);
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


        private bool SchoolCadreTrainExists(Guid id)
        {
            return _context.SchoolCadreTrains.Any(e => e.Id == id);
        }
    }
}
