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
    public class ProvinceCadreTrainsController : PartyMemberDataControllerBase<ProvinceCadreTrain>
    {

        public ProvinceCadreTrainsController(ILogger<ProvinceCadreTrainsController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }

        // GET: ProvinceCadreTrains
        public async Task<IActionResult> Index(int page = 1)
        {
            var pMContext = _context.ProvinceCadreTrains.Include(p => p.Department).Include(p => p.TrainClass).Include(p => p.YearTerm);
            return View(await pMContext.ToListAsync());
        }

        // GET: ProvinceCadreTrains/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var provinceCadreTrain = await _context.ProvinceCadreTrains
                    .Include(p => p.Department)
                    .Include(p => p.TrainClass)
                    .Include(p => p.YearTerm)
            .SingleOrDefaultAsync(m => m.Id == id);
            if (provinceCadreTrain == null)
            {
                return NotFoundData();
            }

            return View(provinceCadreTrain);
        }

        // GET: ProvinceCadreTrains/Create
        public IActionResult Create()
        {
            ProvinceCadreTrain provinceCadreTrain = new ProvinceCadreTrain();
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Code");
            ViewData["TrainClassId"] = new SelectList(_context.TrainClasses, "Id", "Name");
            ViewData["YearTermId"] = new SelectList(_context.YearTerms, "Id", "Id");
            return View(provinceCadreTrain);
        }


        // GET: ProvinceCadreTrains/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var provinceCadreTrain = await _context.ProvinceCadreTrains.SingleOrDefaultAsync(m => m.Id == id);
            if (provinceCadreTrain == null)
            {
                return NotFoundData();
            }
            ViewData["DepartmentId"] = new SelectList(_context.Departments, "Id", "Code", provinceCadreTrain.DepartmentId);
            ViewData["TrainClassId"] = new SelectList(_context.TrainClasses, "Id", "Name", provinceCadreTrain.TrainClassId);
            ViewData["YearTermId"] = new SelectList(_context.YearTerms, "Id", "Id", provinceCadreTrain.YearTermId);
            return View(provinceCadreTrain);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public override async Task<IActionResult> Save([Bind("TrainClassId,YearTermId,Organizer,TrainOrganizational,TrainTime,TrainDuration,Name,Phone,IDNumber,Sex,DepartmentId,Post,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] ProvinceCadreTrain provinceCadreTrain)
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
                    ProvinceCadreTrain provinceCadreTrainInDb = await _context.ProvinceCadreTrains.FindAsync(provinceCadreTrain.Id);
                    if (provinceCadreTrainInDb != null)
                    {
                        provinceCadreTrainInDb.TrainClassId = provinceCadreTrain.TrainClassId;
                        provinceCadreTrainInDb.TrainClass = provinceCadreTrain.TrainClass;
                        provinceCadreTrainInDb.YearTermId = provinceCadreTrain.YearTermId;
                        provinceCadreTrainInDb.YearTerm = provinceCadreTrain.YearTerm;
                        provinceCadreTrainInDb.Organizer = provinceCadreTrain.Organizer;
                        provinceCadreTrainInDb.TrainOrganizational = provinceCadreTrain.TrainOrganizational;
                        provinceCadreTrainInDb.TrainTime = provinceCadreTrain.TrainTime;
                        provinceCadreTrainInDb.TrainDuration = provinceCadreTrain.TrainDuration;
                        provinceCadreTrainInDb.Name = provinceCadreTrain.Name;
                        provinceCadreTrainInDb.Phone = provinceCadreTrain.Phone;
                        provinceCadreTrainInDb.IDNumber = provinceCadreTrain.IDNumber;
                        provinceCadreTrainInDb.Sex = provinceCadreTrain.Sex;
                        provinceCadreTrainInDb.DepartmentId = provinceCadreTrain.DepartmentId;
                        provinceCadreTrainInDb.Department = provinceCadreTrain.Department;
                        provinceCadreTrainInDb.Post = provinceCadreTrain.Post;
                        provinceCadreTrainInDb.Id = provinceCadreTrain.Id;
                        provinceCadreTrainInDb.CreateTime = provinceCadreTrain.CreateTime;
                        provinceCadreTrainInDb.OperatorId = provinceCadreTrain.OperatorId;
                        provinceCadreTrainInDb.Ordinal = provinceCadreTrain.Ordinal;
                        provinceCadreTrainInDb.IsDeleted = provinceCadreTrain.IsDeleted;
                        _context.Update(provinceCadreTrainInDb);
                    }
                    else
                    {
                        //provinceCadreTrain.Id = Guid.NewGuid();
                        _context.Add(provinceCadreTrain);
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


        private bool ProvinceCadreTrainExists(Guid id)
        {
            return _context.ProvinceCadreTrains.Any(e => e.Id == id);
        }
    }
}
