using PartyMemberManager.Dal;
using PartyMemberManager.Framework.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyMemberManager.Controllers
{
    public class ErrorController : PartyMemberControllerBase
    {
        public ErrorController(ILogger<ErrorController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        { 
        }
        [Route("error/404")]
        public IActionResult Error404()
        {
            return View();
        }

        [Route("error/{code:int}")]
        public IActionResult Error(int code)
        {
            // handle different codes or just return the default error view
            return View();
        }
    }
}
