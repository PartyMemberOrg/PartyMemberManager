using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PartyMemberManager.Dal;
using PartyMemberManager.Framework.Controllers;
using PartyMemberManager.Models;

namespace PartyMemberManager.Controllers
{
    //[TypeFilter(typeof(Filters.AuthorizeFilter))]
    public class HomeController : PartyMemberControllerBase
    {
        public HomeController(ILogger<AccountController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }
        public IActionResult Index()
        {
            var modules = _context.Modules.ToList();
            return View(modules);
        }

        public IActionResult RightIndex()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
