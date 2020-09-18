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
using PartyMemberManager.Models;
using System.IO;
using System.Data;
using ExcelCore;
using PartyMemberManager.Core.Enums;
using Newtonsoft.Json;

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
            var pMContext = _context.ProvinceCadreTrains.Include(p => p.Nation).Include(p => p.ProvinceTrainClass);
            ViewBag.ProvinceTrainClassId = new SelectList(_context.ProvinceTrainClasses.Where(d => d.Enabled == true).OrderByDescending(d => d.Ordinal), "Id", "Name");
            return View(await pMContext.ToListAsync());
        }
        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(string year, Guid? provinceTrainClassId, string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<ProvinceCadreTrain> jsonResult = new JsonResultDatasModel<ProvinceCadreTrain>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<ProvinceCadreTrain>();
                if (year != null)
                {
                    filter = filter.And(d => d.ProvinceTrainClass.Year == year);
                }
                if (provinceTrainClassId != null)
                {
                    filter = filter.And(d => d.ProvinceTrainClassId == provinceTrainClassId);
                }
                if (keyword != null)
                {
                    filter = filter.And(d => d.Name.Contains(keyword) || d.Department.Contains(keyword) || d.Post.Contains(keyword));
                }
                var data = await _context.Set<ProvinceCadreTrain>().Include(d => d.ProvinceTrainClass).Include(d => d.Nation)
                    .Where(filter)
                    .Where(d => d.ProvinceTrainClass.Enabled == true)
                    .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Count = _context.Set<ProvinceCadreTrain>().Include(d => d.ProvinceTrainClass).Where(filter).Where(d => d.ProvinceTrainClass.Enabled == true).Count();
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

        // GET: ProvinceCadreTrains/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var provinceCadreTrain = await _context.ProvinceCadreTrains
                    .Include(p => p.Nation)
                    .Include(p => p.ProvinceTrainClass)
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
            ViewData["NationId"] = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.ProvinceTrainClassId = new SelectList(_context.ProvinceTrainClasses.Where(d => d.Enabled == true).OrderByDescending(d => d.Ordinal), "Id", "Name");
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
            ViewData["NationId"] = new SelectList(_context.Nations.OrderBy(d => d.Ordinal), "Id", "Name");
            ViewBag.ProvinceTrainClassId = new SelectList(_context.ProvinceTrainClasses.Where(d => d.Enabled == true).OrderByDescending(d => d.Ordinal), "Id", "Name");
            return View(provinceCadreTrain);
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
                var data = await _context.Set<ProvinceCadreTrain>().Where(DataFilter).SingleOrDefaultAsync(m => m.Id == id);
                if (data == null)
                    throw new PartyMemberException("未找到要删除的数据");
                ValidateDeleteObject(data);
                var provinceTrainClass = await _context.ProvinceTrainClasses.FindAsync(data.ProvinceTrainClassId);
                provinceTrainClass.Total -= 1;
                _context.Update(provinceTrainClass);
                _context.Set<ProvinceCadreTrain>().Remove(data);
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
        /// <summary>
        /// 删除数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("BatchDelete")]
        public async Task<IActionResult> BatchDelete(string idList)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };

            try
            {
                if (string.IsNullOrEmpty(idList))
                    throw new PartyMemberException("未选择删除的人员");
                string[] idListSplit = idList.Split(",");
                foreach (var item in idListSplit)
                {
                    var id = Guid.Parse(item);
                    var data = await _context.Set<ProvinceCadreTrain>().SingleOrDefaultAsync(m => m.Id == id);
                    if (data == null)
                        throw new PartyMemberException("未找到要删除的数据");
                    ValidateDeleteObject(data);
                    var provinceTrainClass = await _context.ProvinceTrainClasses.FindAsync(data.ProvinceTrainClassId);
                    provinceTrainClass.Total -= 1;
                    _context.Update(provinceTrainClass);
                    _context.Set<ProvinceCadreTrain>().Remove(data);
                }

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
        public override async Task<IActionResult> Save([Bind("ProvinceTrainClassId,Name,IdNumber,Sex,NationId,Department,Post,Phone,Demand1,Demand2,OtherDemand,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] ProvinceCadreTrain provinceCadreTrain)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据删除成功"
            };
            try
            {
                if (provinceCadreTrain.ProvinceTrainClassId == Guid.Empty)
                    throw new PartyMemberException("请选择培训班");
                if (provinceCadreTrain.NationId == Guid.Empty)
                    throw new PartyMemberException("请选择民族");
                if (!StringHelper.ValidateIdNumber(provinceCadreTrain.IdNumber))
                    throw new PartyMemberException($"身份证不合法");
                if (ModelState.IsValid)
                {
                    ProvinceCadreTrain provinceCadreTrainInDb = await _context.ProvinceCadreTrains.FindAsync(provinceCadreTrain.Id);
                    if (provinceCadreTrainInDb != null)
                    {
                        provinceCadreTrainInDb.ProvinceTrainClassId = provinceCadreTrain.ProvinceTrainClassId;
                        provinceCadreTrainInDb.ProvinceTrainClass = provinceCadreTrain.ProvinceTrainClass;
                        provinceCadreTrainInDb.Name = provinceCadreTrain.Name;
                        provinceCadreTrainInDb.IdNumber = provinceCadreTrain.IdNumber;
                        provinceCadreTrainInDb.Sex = provinceCadreTrain.Sex;
                        provinceCadreTrainInDb.NationId = provinceCadreTrain.NationId;
                        provinceCadreTrainInDb.Nation = provinceCadreTrain.Nation;
                        provinceCadreTrainInDb.Department = provinceCadreTrain.Department;
                        provinceCadreTrainInDb.Post = provinceCadreTrain.Post;
                        provinceCadreTrainInDb.Phone = provinceCadreTrain.Phone;
                        provinceCadreTrainInDb.Demand1 = provinceCadreTrain.Demand1;
                        provinceCadreTrainInDb.Demand2 = provinceCadreTrain.Demand2;
                        provinceCadreTrainInDb.OtherDemand = provinceCadreTrain.OtherDemand;
                        //provinceCadreTrainInDb.CreateTime = DateTime.Now;
                        //provinceCadreTrainInDb.OperatorId = CurrentUser.Id;
                        //provinceCadreTrainInDb.Ordinal = _context.ProvinceCadreTrains.Count() + 1;
                        //provinceCadreTrainInDb.IsDeleted = provinceCadreTrain.IsDeleted;
                        _context.Update(provinceCadreTrainInDb);
                    }
                    else
                    {
                        //provinceCadreTrain.Id = Guid.NewGuid();
                        provinceCadreTrain.CreateTime = DateTime.Now;
                        provinceCadreTrain.OperatorId = CurrentUser.Id;
                        provinceCadreTrain.Ordinal = _context.ProvinceCadreTrains.Count() + 1;
                        var ProvinceCadreTrainOld = _context.ProvinceCadreTrains.Where(d => d.IdNumber == provinceCadreTrain.IdNumber && d.ProvinceTrainClassId == provinceCadreTrain.ProvinceTrainClassId).FirstOrDefault();
                        if (ProvinceCadreTrainOld != null)
                        {
                            var noName = "【" + provinceCadreTrain.Name + "-" + provinceCadreTrain.IdNumber + "】";
                            throw new PartyMemberException(noName + "已在该培训班，请核对");
                        }
                        _context.Add(provinceCadreTrain);
                        var provinceTrainClass = await _context.ProvinceTrainClasses.FindAsync(provinceCadreTrain.ProvinceTrainClassId);
                        provinceTrainClass.Total += 1;
                        _context.Update(provinceTrainClass);
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
        /// 根据年份查询省级干部培训班
        /// </summary>

        /// <param name="year"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetProvinceTrainClasses(string year)
        {
            JsonResultModel<ProvinceTrainClass> jsonResult = new JsonResultModel<ProvinceTrainClass>
            {
                Code = 0,
                Message = "",
                Datas = new List<ProvinceTrainClass>()
            };

            try
            {
                var filter = PredicateBuilder.True<ProvinceTrainClass>();
                if (year != null)
                {
                    filter = filter.And(d => d.Year == year);
                }
                var data = await _context.Set<ProvinceTrainClass>()
                    .Where(filter).Where(d => d.Enabled == true)
                    .OrderByDescending(d => d.Ordinal).ToListAsync();
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Datas = data;
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
        /// 导入省级干部培训名单
        /// </summary>
        /// <returns></returns>
        public IActionResult Import()
        {
            ProvinceCadreTrainImportViewModel model = new ProvinceCadreTrainImportViewModel
            {
                Year = DateTime.Today.Year,
            };
            ViewBag.ProvinceTrainClasses = new SelectList(_context.ProvinceTrainClasses.OrderBy(d => d.Ordinal), "Id", "Name");
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Import(ProvinceCadreTrainImportViewModel model)
        {
            JsonResultImport jsonResult = new JsonResultImport
            {
                Code = 0,
                Message = "数据导入成功"
            };
            try
            {
                if (ModelState.IsValid)
                {
                    IFormFile file = model.File;
                    if (file != null)
                    {
                        ProvinceTrainClass provinceTrainClass = await _context.ProvinceTrainClasses.FindAsync(model.ProvinceTrainClassId);
                        Stream stream = file.OpenReadStream();
                        var filePath = Path.GetTempFileName();
                        var fileStream = System.IO.File.Create(filePath);
                        await file.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                        fileStream.Close();
                        #region 省级干部培训
                        DataTable table = OfficeHelper.ReadExcelToDataTable(filePath);
                        DataTable tableErrorData = table.Clone();
                        DataColumn columnErrorMessage = tableErrorData.Columns.Add("错误提示", typeof(string));
                        int rowIndex = 0;
                        int successCount = 0;
                        string fieldsStudent = "序号,姓名,身份证,性别,民族,所在单位,职务,联系电话,备注,学员需求1,学员需求2,其他需求";
                        string[] fieldList = fieldsStudent.Split(',');
                        foreach (string field in fieldList)
                        {
                            if (!table.Columns.Contains(field))
                                throw new PartyMemberException($"缺少【{field}】列");
                        }
                        foreach (DataRow row in table.Rows)
                        {
                            rowIndex++;
                            try
                            {
                                ProvinceCadreTrain provinceCadreTrain = new ProvinceCadreTrain
                                {
                                    Id = Guid.NewGuid(),
                                    CreateTime = DateTime.Now,
                                    Ordinal = rowIndex,
                                    OperatorId = CurrentUser.Id,
                                    ProvinceTrainClassId = provinceTrainClass.Id,
                                };
                                string nameField = "姓名";
                                string sexField = "性别";
                                string nationField = "民族";
                                string idField = "身份证";
                                string phoneField = "联系电话";
                                string remarkField = "备注";
                                string departmentField = "所在单位";
                                string titleField = "职务";
                                string demand1Field = "学员需求1";
                                string demand2Field = "学员需求2";
                                string otherDemandField = "其他需求";
                                string name = row[nameField].ToString();
                                string sex = row[sexField].ToString();
                                string nation = row[nationField].ToString();
                                string id = row[idField].ToString();
                                string phone = row[phoneField].ToString();
                                string remark = row[remarkField].ToString();
                                string title = row[titleField].ToString();
                                string department = row[departmentField].ToString();
                                string demand1 = row[demand1Field].ToString();
                                string demand2 = row[demand2Field].ToString();
                                string otherDemand = row[otherDemandField].ToString();
                                //跳过姓名为空的记录
                                if (string.IsNullOrEmpty(name)) continue;
                                if (!StringHelper.ValidateIdNumber(id))
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{idField}】不合法");
                                nation = nation.Trim();
                                Nation nationData = _context.Nations.Where(n => n.Name == nation).FirstOrDefault();
                                if (nationData == null)
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{nationField}】不合法");
                                Guid nationId = nationData.Id;
                                provinceCadreTrain.Name = name;
                                provinceCadreTrain.Sex = Sex.Parse<Sex>(sex);
                                provinceCadreTrain.NationId = nationId;
                                provinceCadreTrain.IdNumber = id;
                                provinceCadreTrain.Phone = phone;
                                provinceCadreTrain.Post = title;
                                provinceCadreTrain.Department = department;
                                provinceCadreTrain.Demand1 = demand1;
                                provinceCadreTrain.Demand2 = demand2;
                                provinceCadreTrain.OtherDemand = otherDemand;

                                _context.ProvinceCadreTrains.Add(provinceCadreTrain);

                                provinceTrainClass.Total += 1;
                                var ProvinceCadreTrainOld = _context.ProvinceCadreTrains.Where(d => d.IdNumber == provinceCadreTrain.IdNumber && d.ProvinceTrainClassId == provinceCadreTrain.ProvinceTrainClassId).FirstOrDefault();
                                if (ProvinceCadreTrainOld != null)
                                {
                                    var noName = "【" + ProvinceCadreTrainOld.Name + "-" + ProvinceCadreTrainOld.IdNumber + "】";
                                    throw new ImportDataErrorException(noName + "已在该培训班，请核对");
                                }
                                _context.Update(provinceTrainClass);
                                successCount++;
                                await _context.SaveChangesAsync();
                            }
                            catch (ImportDataErrorException ex)
                            {
                                //捕获到数据错误时，继续导入，将错误信息反馈给用户
                                DataRow rowErrorData = tableErrorData.NewRow();
                                foreach (DataColumn column in table.Columns)
                                    rowErrorData[column.ColumnName] = row[column.ColumnName];
                                rowErrorData[columnErrorMessage] = ex.Message;
                                tableErrorData.Rows.Add(rowErrorData);
                            }

                        }
                        #endregion
                        if (tableErrorData.Rows.Count > 0)
                        {
                            string basePath = GetErrorImportDataFilePath();
                            string fileName = $"省级干部培训名单错误数据_{CurrentUser.LoginName}.xlsx";
                            string fileWithPath = $"{basePath}{Path.DirectorySeparatorChar}{fileName}";
                            Stream streamOutExcel = OfficeHelper.ExportExcelByOpenXml(tableErrorData);
                            FileStream outExcelFile = new FileStream(fileWithPath, FileMode.Create, System.IO.FileAccess.Write);
                            byte[] bytes = new byte[streamOutExcel.Length];
                            streamOutExcel.Read(bytes, 0, (int)streamOutExcel.Length);
                            outExcelFile.Write(bytes, 0, bytes.Length);
                            outExcelFile.Close();
                            streamOutExcel.Close();
                            jsonResult.ErrorDataFile = $"ProvinceCadreTrains/GetErrorImportData?fileName={fileName}";
                            jsonResult.FailCount = tableErrorData.Rows.Count;
                            jsonResult.SuccessCount = successCount;
                            jsonResult.Code = -2;
                            jsonResult.Message = "部分数据导入错误";
                        }
                    }
                    else
                    {
                        jsonResult.Code = -1;
                        jsonResult.Message = "请选择文件";
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
                jsonResult.Message = "省级干部培训导入错误";
            }
            catch (PartyMemberException ex)
            {
                jsonResult.Code = -1;
                jsonResult.Message = ex.Message;
            }
            catch (JsonReaderException ex)
            {
                _logger.LogError(ex, ex.Message);
                jsonResult.Code = -1;
                jsonResult.Message = "JSON文件内容格式错误";
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
        /// 返回导入错误文件的存放路径
        /// </summary>
        /// <returns></returns>
        private static string GetErrorImportDataFilePath()
        {
            string basePath = AppContext.BaseDirectory;
            if (!basePath.EndsWith(Path.DirectorySeparatorChar))
                basePath += Path.DirectorySeparatorChar;
            basePath += $"ErrorImportData";
            if (!Directory.Exists(basePath))
                Directory.CreateDirectory(basePath);
            return basePath;
        }
        /// <summary>
        /// 返回错误导入数据
        /// </summary>
        /// <returns></returns>
        public IActionResult GetErrorImportData(string fileName)
        {
            string basePath = GetErrorImportDataFilePath();
            string fileWithPath = $"{basePath}{Path.DirectorySeparatorChar}{fileName}";
            FileStream outExcelFile = new FileStream(fileWithPath, FileMode.Open);
            return File(outExcelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "省级干部培训名单导入失败数据.xlsx");
        }

        private bool ProvinceCadreTrainExists(Guid id)
        {
            return _context.ProvinceCadreTrains.Any(e => e.Id == id);
        }
    }
}
