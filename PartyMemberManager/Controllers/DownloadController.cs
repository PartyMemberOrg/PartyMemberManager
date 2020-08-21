using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PartyMemberManager.Controllers
{
    public class DownloadController : Controller
    {
        public IActionResult ExcelTemplates(string fileName)
        {
            return File(System.IO.Path.Combine(AppContext.BaseDirectory, "ExcelTemplates", fileName), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        }
    }
}
