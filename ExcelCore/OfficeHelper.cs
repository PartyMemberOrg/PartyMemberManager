using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Data;
using System.IO;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using System.Data.Common;

namespace ExcelCore
{
    public class OfficeHelper
    {
        /// <summary>
        /// 将excel文件内容读取到DataTable数据表中
        /// </summary>
        /// <param name="fileName">文件完整路径名</param>
        /// <param name="sheetName">指定读取excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名：true=是，false=否</param>
        /// <returns>DataTable数据表</returns>
        public static DataTable ReadExcelToDataTable(string fileName, string sheetName = null, bool isFirstRowColumn = true)
        {
            //定义要返回的datatable对象
            DataTable data = new DataTable();
            //excel工作表
            ISheet sheet = null;
            //数据开始行(排除标题行)
            int startRow = 0;
            try
            {
                if (!File.Exists(fileName))
                {
                    return null;
                }
                //根据指定路径读取文件
                FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                //根据文件流创建excel数据结构
                IWorkbook workbook = WorkbookFactory.Create(fs);
                //IWorkbook workbook = new HSSFWorkbook(fs);
                //如果有指定工作表名称
                if (!string.IsNullOrEmpty(sheetName))
                {
                    sheet = workbook.GetSheet(sheetName);
                    //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    if (sheet == null)
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    //如果没有指定的sheetName，则尝试获取第一个sheet
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    //一行最后一个cell的编号 即总的列数
                    int cellCount = firstRow.LastCellNum;
                    //如果第一行是标题列名
                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }
                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            if (row.Cells.Count == 0) continue;
                            if (row.Cells.Count <= j) break;
                            if (row.GetCell(j) != null) //同理，没有数据的单元格都默认是null
                            {
                                switch (row.GetCell(j).CellType)
                                {
                                    case NPOI.SS.UserModel.CellType.Numeric:
                                        switch (row.GetCell(j).CellStyle.DataFormat)
                                        {
                                            case 0:
                                                double doubleValue = row.GetCell(j).NumericCellValue;
                                                dataRow[j] = row.GetCell(j).ToString();
                                                break;
                                            case 14://日期格式
                                            case 31:
                                            case 57:
                                            case 58:
                                            case 20://时间格式
                                            case 32:
                                                DateTime timeValue = row.GetCell(j).DateCellValue;
                                                dataRow[j] = timeValue.ToString("yyyy-MM-dd HH:mm:ss");
                                                break;
                                            default:
                                                dataRow[j] = row.GetCell(j).ToString();
                                                break;
                                        }
                                        break;
                                    default:
                                        dataRow[j] = row.GetCell(j).ToString();
                                        break;
                                }
                            }
                        }
                        data.Rows.Add(dataRow);
                    }
                }
                fs.Close();
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 将文件流读取到DataTable数据表中
        /// </summary>
        /// <param name="fileStream">文件流</param>
        /// <param name="sheetName">指定读取excel工作薄sheet的名称</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名：true=是，false=否</param>
        /// <returns>DataTable数据表</returns>
        public static DataTable ReadStreamToDataTable(Stream fileStream, string sheetName = null, bool isFirstRowColumn = true)
        {
            //定义要返回的datatable对象
            DataTable data = new DataTable();
            //excel工作表
            ISheet sheet = null;
            //数据开始行(排除标题行)
            int startRow = 0;
            try
            {
                //根据文件流创建excel数据结构,NPOI的工厂类WorkbookFactory会自动识别excel版本，创建出不同的excel数据结构
                IWorkbook workbook = WorkbookFactory.Create(fileStream);
                //如果有指定工作表名称
                if (!string.IsNullOrEmpty(sheetName))
                {
                    sheet = workbook.GetSheet(sheetName);
                    //如果没有找到指定的sheetName对应的sheet，则尝试获取第一个sheet
                    if (sheet == null)
                    {
                        sheet = workbook.GetSheetAt(0);
                    }
                }
                else
                {
                    //如果没有指定的sheetName，则尝试获取第一个sheet
                    sheet = workbook.GetSheetAt(0);
                }
                if (sheet != null)
                {
                    IRow firstRow = sheet.GetRow(0);
                    //一行最后一个cell的编号 即总的列数
                    int cellCount = firstRow.LastCellNum;
                    //如果第一行是标题列名
                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            ICell cell = firstRow.GetCell(i);
                            if (cell != null)
                            {
                                string cellValue = cell.StringCellValue;
                                if (cellValue != null)
                                {
                                    DataColumn column = new DataColumn(cellValue);
                                    data.Columns.Add(column);
                                }
                            }
                        }
                        startRow = sheet.FirstRowNum + 1;
                    }
                    else
                    {
                        startRow = sheet.FirstRowNum;
                    }
                    //最后一列的标号
                    int rowCount = sheet.LastRowNum;
                    for (int i = startRow; i <= rowCount; ++i)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row == null || row.FirstCellNum < 0) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = data.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            //同理，没有数据的单元格都默认是null
                            ICell cell = row.GetCell(j);
                            if (cell != null)
                            {
                                if (cell.CellType == NPOI.SS.UserModel.CellType.Numeric)
                                {
                                    //判断是否日期类型
                                    if (DateUtil.IsCellDateFormatted(cell))
                                    {
                                        dataRow[j] = row.GetCell(j).DateCellValue;
                                    }
                                    else
                                    {
                                        dataRow[j] = row.GetCell(j).ToString().Trim();
                                    }
                                }
                                else
                                {
                                    dataRow[j] = row.GetCell(j).ToString().Trim();
                                }
                            }
                        }
                        data.Rows.Add(dataRow);
                    }
                }
                return data;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //创建不同版本的文件， excel2003版 或2007+版

        public static IWorkbook BuildWorkbook(DataTable dt, string file)
        {
            IWorkbook book;
            string fileExt = Path.GetExtension(file).ToLower();
            if (fileExt == ".xlsx")
            { book = new XSSFWorkbook(); }
            else if (fileExt == ".xls")
            { book = new HSSFWorkbook(); }
            else { book = null; }
            //var book = new HSSFWorkbook();
            ISheet sheet1 = book.CreateSheet("Sheet1");
            //ISheet sheet2 = book.CreateSheet("Sheet2");
            IRow drowHead1 = sheet1.CreateRow(0);
            //IRow drowHead2 = sheet2.CreateRow(0);
            ICellStyle style = book.CreateCellStyle();
            style.BorderBottom = BorderStyle.Thin;
            style.BorderLeft = BorderStyle.Thin;
            style.BorderRight = BorderStyle.Thin;
            style.BorderTop = BorderStyle.Thin;
            for (int j = 0; j < dt.Columns.Count; j++)
            {
                ICell cell1 = drowHead1.CreateCell(j, NPOI.SS.UserModel.CellType.String);
                //ICell cell2 = drowHead2.CreateCell(j, CellType.String);
                cell1.SetCellValue(dt.Columns[j].ColumnName);
                //cell2.SetCellValue(dt.Columns[j].ColumnName);
                cell1.CellStyle = style;
                //cell2.CellStyle = style;
            }

            //填充数据
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                if (i < 65535)
                {
                    IRow drow = sheet1.CreateRow(i + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        ICell cell = drow.CreateCell(j, NPOI.SS.UserModel.CellType.String);
                        cell.SetCellValue(dt.Rows[i][j].ToString());
                        cell.CellStyle = style;
                    }
                }
                //if (i >= 65535) //再创建一个sheet
                //{
                //    IRow drow = sheet2.CreateRow(i - 65535 + 1);
                //    for (int j = 0; j < dt.Columns.Count; j++)
                //    {
                //        ICell cell = drow.CreateCell(j, CellType.String);
                //        cell.SetCellValue(dt.Rows[i][j].ToString());
                //        cell.CellStyle = style;
                //    }
                //}
            }
            //自动列宽
            for (int i = 0; i <= dt.Columns.Count; i++)
            {
                sheet1.SetColumnWidth(i, 120);
                //sheet1.AutoSizeColumn(i);
                //sheet1.AutoSizeColumn(i, true);
                //sheet2.AutoSizeColumn(i, true);
            }
            return book;
        }

        //导出至excel文件
        public static byte[] ExportExcel(DataTable dt, string fileName = "")
        {
            ////////生成Excel
            NPOIMemoryStream ms = new NPOIMemoryStream(false);
            IWorkbook book = BuildWorkbook(dt, fileName);
            book.Write(ms);
            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            ms.AllowColse = true;
            byte[] datas = ms.GetBuffer();
            ms.Close();
            ms = null;
            return datas;
        }

        public static Stream ExportExcelByOpenXml(DataTable datatable)
        {
            MemoryStream ms = new MemoryStream();
            var spreadsheetDocument = SpreadsheetDocument.Create(ms, SpreadsheetDocumentType.Workbook);
            var workbookpart = spreadsheetDocument.AddWorkbookPart();
            workbookpart.Workbook = new Workbook();
            WorksheetPart worksheetPart = workbookpart.AddNewPart<WorksheetPart>();
            worksheetPart.Worksheet = new Worksheet(new SheetData());
            Sheets sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());
            Sheet sheet = new Sheet() { Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };
            sheets.Append(sheet);
            var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();
            Row rowHead = new Row();
            foreach (DataColumn column in datatable.Columns)
            {
                Cell dataCell = new Cell();
                dataCell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue($"{column.ColumnName}");
                dataCell.DataType = new EnumValue<CellValues>(CellValues.String);
                rowHead.AppendChild(dataCell);
            }
            sheetData.Append(rowHead);
            foreach (DataRow dataRow in datatable.Rows)
            {
                Row row = new Row();
                foreach (DataColumn column in datatable.Columns)
                {
                    Cell dataCell = new Cell();
                    dataCell.CellValue = new DocumentFormat.OpenXml.Spreadsheet.CellValue($"{dataRow[column]}");
                    dataCell.DataType = new EnumValue<CellValues>(CellValues.String);
                    row.AppendChild(dataCell);
                }
                sheetData.Append(row);
            }


            workbookpart.Workbook.Save();
            spreadsheetDocument.Close();
            ms.Flush();
            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }

        private class NPOIMemoryStream : MemoryStream
        {
            /// <summary>
            /// 获取流是否关闭
            /// </summary>
            public bool AllowColse
            {
                get;
                set;
            }

            public NPOIMemoryStream(bool allowClose = false)
            {
                AllowColse = allowClose;
            }

            public override void Close()
            {
                if (AllowColse)
                {
                    base.Close();
                }

            }
        }
    }
}
