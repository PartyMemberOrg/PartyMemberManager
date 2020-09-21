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
using System.IO;
using System.Data;
using ExcelCore;
using Newtonsoft.Json;
using PartyMemberManager.Models;

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
            return View(await _context.SchoolCadreTrains.OrderBy(s => s.Ordinal).GetPagedDataAsync(page));
        }

        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<IActionResult> GetDatasWithFilter(string year, string keyword, int page = 1, int limit = 10)
        {
            JsonResultDatasModel<SchoolCadreTrain> jsonResult = new JsonResultDatasModel<SchoolCadreTrain>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var filter = PredicateBuilder.True<SchoolCadreTrain>();
                if (year != null)
                {
                    filter = filter.And(d => d.Year == year);
                }
                if (keyword != null)
                {
                    filter = filter.And(d => d.Name.Contains(keyword) || d.Organizer.Contains(keyword) || d.TrainClassName.Contains(keyword) || d.TrainOrganizational.Contains(keyword));
                }
                var data = await _context.Set<SchoolCadreTrain>()
                    .Where(filter)
                    .OrderByDescending(d => d.Ordinal).GetPagedDataAsync(page, limit);
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Count = _context.Set<SchoolCadreTrain>().Where(filter).Count();
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

        // GET: SchoolCadreTrains/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFoundData();
            }

            var schoolCadreTrain = await _context.SchoolCadreTrains
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
            return View(schoolCadreTrain);
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
                    var data = await _context.Set<SchoolCadreTrain>().SingleOrDefaultAsync(m => m.Id == id);
                    if (data == null)
                        throw new PartyMemberException("未找到要删除的数据");
                    ValidateDeleteObject(data);
                    _context.Set<SchoolCadreTrain>().Remove(data);
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
        public override async Task<IActionResult> Save([Bind("Year,Name,TrainClassName,Organizer,TrainOrganizational,TrainTime,EndTrainTime,TrainAddress,ClassHour,Id,CreateTime,OperatorId,Ordinal,IsDeleted")] SchoolCadreTrain schoolCadreTrain)
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
                        schoolCadreTrainInDb.Year = schoolCadreTrain.Year;
                        schoolCadreTrainInDb.Name = schoolCadreTrain.Name;
                        schoolCadreTrainInDb.TrainClassName = schoolCadreTrain.TrainClassName;
                        schoolCadreTrainInDb.Organizer = schoolCadreTrain.Organizer;
                        schoolCadreTrainInDb.TrainOrganizational = schoolCadreTrain.TrainOrganizational;
                        schoolCadreTrainInDb.TrainTime = schoolCadreTrain.TrainTime;
                        schoolCadreTrainInDb.EndTrainTime = schoolCadreTrain.EndTrainTime;
                        schoolCadreTrainInDb.ClassHour = schoolCadreTrain.ClassHour;

                        schoolCadreTrainInDb.TrainAddress = schoolCadreTrain.TrainAddress;
                        //schoolCadreTrainInDb.TrainDuration = schoolCadreTrain.TrainDuration;
                        //schoolCadreTrainInDb.ClassHour = schoolCadreTrain.ClassHour;
                        //schoolCadreTrainInDb.Id = schoolCadreTrain.Id;
                        //schoolCadreTrainInDb.CreateTime = DateTime.Now;
                        //schoolCadreTrainInDb.OperatorId = CurrentUser.Id;
                        //schoolCadreTrainInDb.Ordinal = _context.SchoolCadreTrains.Count() + 1;
                        //schoolCadreTrainInDb.IsDeleted = schoolCadreTrain.IsDeleted;
                        _context.Update(schoolCadreTrainInDb);
                    }
                    else
                    {
                        //schoolCadreTrain.Id = Guid.NewGuid();
                        schoolCadreTrain.CreateTime = DateTime.Now;
                        schoolCadreTrain.OperatorId = CurrentUser.Id;
                        schoolCadreTrain.Ordinal = _context.SchoolCadreTrains.Count() + 1;
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

        /// <summary>
        /// 导入省级干部培训名单
        /// </summary>
        /// <returns></returns>
        public IActionResult Import()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Import(SchoolCadreTranImportViewModel model)
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
                        Stream stream = file.OpenReadStream();
                        var filePath = Path.GetTempFileName();
                        var fileStream = System.IO.File.Create(filePath);
                        await file.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                        fileStream.Close();
                        #region 领导干部培训名单
                        DataTable table = OfficeHelper.ReadExcelToDataTable(filePath);
                        DataTable tableErrorData = table.Clone();
                        DataColumn columnErrorMessage = tableErrorData.Columns.Add("错误提示", typeof(string));
                        int rowIndex = 0;
                        int successCount = 0;
                        string fieldsStudent = "序号,姓名,培训班名称,组织单位,培训单位,年度,培训时间,结束时间,培训地点,备注";
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
                                SchoolCadreTrain schoolCadreTrain = new SchoolCadreTrain
                                {
                                    Id = Guid.NewGuid(),
                                    CreateTime = DateTime.Now,
                                    Ordinal = rowIndex,
                                    OperatorId = CurrentUser.Id
                                };
                                string nameField = "姓名";
                                string trainClassNameField = "培训班名称";
                                string organizerField = "组织单位";
                                string trainOrganizationalField = "培训单位";
                                string yearField = "年度";
                                string trainTimeField = "培训时间";
                                string trainEndTimeField = "结束时间";
                                string trainAddressField = "培训地点";
                                string remarkField = "备注";

                                string name = row[nameField].ToString();
                                string trainClassName = row[trainClassNameField].ToString();
                                string organizer = row[organizerField].ToString();
                                string trainOrganizational = row[trainOrganizationalField].ToString();
                                string year = row[yearField].ToString();
                                string trainTime = row[trainTimeField].ToString();
                                string trainEndTime = row[trainEndTimeField].ToString();
                                string trainAddress = row[trainAddressField].ToString();
                                string remark = row[remarkField].ToString();
                                year = year.Replace("年", "").Trim();
                                //跳过姓名为空的记录
                                if (string.IsNullOrEmpty(name)) continue;
                                schoolCadreTrain.Name = name;
                                //schoolCadreTrain.ClassHour = 0;
                                schoolCadreTrain.Organizer = organizer;
                                schoolCadreTrain.TrainAddress = trainAddress;
                                schoolCadreTrain.TrainClassName = trainClassName;
                                //int trainDurationValue = 0;
                                //if (int.TryParse(trainDuration, out trainDurationValue))
                                //schoolCadreTrain.TrainDuration = trainDurationValue;
                                //else
                                //schoolCadreTrain.TrainDuration = 0;
                                trainTime = trainTime.Replace(".", "-").Replace("/", "-");
                                DateTime trainTimeValue = DateTime.Now;
                                if (!TryParseDate(trainTime, out trainTimeValue))
                                {
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{trainTimeField}】日期格式不合法");
                                }
                                trainEndTime = trainEndTime.Replace(".", "-").Replace("/", "-");
                                DateTime trainEndTimeValue = DateTime.Now;
                                if (!TryParseDate(trainEndTime, out trainEndTimeValue))
                                {
                                    throw new ImportDataErrorException($"第{rowIndex}行数据中的【{trainEndTimeField}】日期格式不合法");
                                }
                                schoolCadreTrain.TrainOrganizational = trainOrganizational;
                                schoolCadreTrain.TrainTime = trainTimeValue;
                                schoolCadreTrain.EndTrainTime = trainEndTimeValue;
                                schoolCadreTrain.Year = year;

                                _context.SchoolCadreTrains.Add(schoolCadreTrain);
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
                            string fileName = $"学校干部培训名单错误数据_{CurrentUser.LoginName}.xlsx";
                            string fileWithPath = $"{basePath}{Path.DirectorySeparatorChar}{fileName}";
                            Stream streamOutExcel = OfficeHelper.ExportExcelByOpenXml(tableErrorData);
                            FileStream outExcelFile = new FileStream(fileWithPath, FileMode.Create, System.IO.FileAccess.Write);
                            byte[] bytes = new byte[streamOutExcel.Length];
                            streamOutExcel.Read(bytes, 0, (int)streamOutExcel.Length);
                            outExcelFile.Write(bytes, 0, bytes.Length);
                            outExcelFile.Close();
                            streamOutExcel.Close();
                            jsonResult.ErrorDataFile = $"SchoolCareTrains/GetErrorImportData?fileName={fileName}";
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
                jsonResult.Message = "学校干部培训名单导入错误";
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
            return File(outExcelFile, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "学校干部培训名单导入失败数据.xlsx");
        }
        /// <summary>
        /// 导出所有学生数据
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Export(string year, string keyword)
        {
            try
            {
                string fileName = "学校干部培训导出名单.xlsx";
                List<SchoolCadreTrain> schoolCadreTrains = null;
                var filter = PredicateBuilder.True<SchoolCadreTrain>();
                if (year != null)
                {
                    filter = filter.And(d => d.Year == year);
                }
                if (keyword != null)
                {
                    filter = filter.And(d => d.Name.Contains(keyword) || d.Organizer.Contains(keyword) || d.TrainClassName.Contains(keyword) || d.TrainOrganizational.Contains(keyword));
                }
                schoolCadreTrains = await _context.Set<SchoolCadreTrain>()
                    .Where(filter)
                    .OrderByDescending(d => d.Ordinal).ToListAsync();

                DataTable table = new DataTable();
                //string fieldsStudent = "序号,姓名,,组织单位,培训单位,年度,培训时间,结束时间,培训地点,备注";
                table.Columns.Add("年度", typeof(string));
                table.Columns.Add("姓名", typeof(string));
                table.Columns.Add("培训班名称", typeof(string));
                table.Columns.Add("组织单位", typeof(string));
                table.Columns.Add("培训单位", typeof(string));
                table.Columns.Add("培训时间", typeof(string));
                table.Columns.Add("结束时间", typeof(string));
                table.Columns.Add("培训时长", typeof(int));
                table.Columns.Add("培训学时", typeof(int));
                table.Columns.Add("培训地点", typeof(string));
                table.Columns.Add("备注", typeof(string));
                foreach (SchoolCadreTrain schoolCadreTrain in schoolCadreTrains)
                {
                    DataRow row = table.NewRow();
                    row["年度"] = schoolCadreTrain.Year;
                    row["姓名"] = schoolCadreTrain.Name;
                    row["培训班名称"] = schoolCadreTrain.TrainClassName;
                    row["组织单位"] = schoolCadreTrain.Organizer;
                    row["培训单位"] = schoolCadreTrain.TrainOrganizational;
                    row["培训时间"] = string.Format("{0:yyyy-MM-dd}", schoolCadreTrain.TrainTime);
                    row["结束时间"] = string.Format("{0:yyyy-MM-dd}", schoolCadreTrain.EndTrainTime);
                    row["培训时长"] = schoolCadreTrain.TrainDuration;
                    row["培训学时"] = schoolCadreTrain.ClassHour;
                    row["培训地点"] = schoolCadreTrain.TrainAddress;
                    row["备注"] = "";
                    table.Rows.Add(row);
                }
                Stream datas = OfficeHelper.ExportExcelByOpenXml(table);
                return File(datas, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                return View("ShowErrorMessage", ex);
            }
        }

        private bool SchoolCadreTrainExists(Guid id)
        {
            return _context.SchoolCadreTrains.Any(e => e.Id == id);
        }
    }
}
