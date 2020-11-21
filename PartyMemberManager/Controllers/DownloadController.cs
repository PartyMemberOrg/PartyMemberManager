using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PartyMemberManager.Dal;


namespace PartyMemberManager.Controllers
{
    public class DownloadController : Controller
    {
        public IActionResult ExcelTemplates(string fileName)
        {
            string file = System.IO.Path.Combine(AppContext.BaseDirectory, "ExcelTemplates", fileName);
            System.IO.FileStream fileStream = new System.IO.FileStream(file, System.IO.FileMode.Open);
            return File(fileStream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",fileName);
        }
    }
}
