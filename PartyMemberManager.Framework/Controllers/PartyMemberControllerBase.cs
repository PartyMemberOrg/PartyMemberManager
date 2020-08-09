using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using EntityExtension;
using PartyMemberManager.Core.Enums;
using PartyMemberManager.Core.Exceptions;
using PartyMemberManager.Dal;
using PartyMemberManager.Dal.Entities;
using PartyMemberManager.Framework.Models.JsonModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PartyMemberManager.Framework.Controllers
{
    [Authorize]
    public class PartyMemberControllerBase : Controller
    {
        private IHttpContextAccessor _accessor;
        private Operator currentUser = null;
        public Operator CurrentUser
        {

            get
            {
                HttpContext httpContext = HttpContext;
                if (httpContext == null)
                    httpContext = _accessor.HttpContext;
                if (currentUser != null) return currentUser;
                if (httpContext == null) return null;
                if (httpContext.User == null)
                    return null;
                var identity = httpContext.User.Identity;
                var claims = httpContext.User.Claims;
                Operator @operator = new Operator
                {
                    Id = Guid.Parse(claims.First(c => c.Type == ClaimTypes.NameIdentifier).Value),
                    LoginName = claims.First(c => c.Type == ClaimTypes.Name).Value,
                    Name = claims.First(c => c.Type == "FullName").Value,
                    Roles = (Role)Enum.Parse(typeof(Role), claims.First(c => c.Type == ClaimTypes.Role).Value)
                };
                currentUser = @operator;
                return @operator;
            }
        }
        protected readonly PMContext _context;
        protected readonly ILogger<PartyMemberControllerBase> _logger;
        public PartyMemberControllerBase(ILogger<PartyMemberControllerBase> logger, PMContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _logger = logger;
            _accessor = accessor;
        }

        protected void ShowAndLogSystemError(Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            _logger.LogError(ex, ex.Message);
        }

        /// <summary>
        /// 未找到数据
        /// </summary>
        /// <returns></returns>
        public virtual IActionResult NotFoundData()
        {
            return View("NotFoundData");
        }
    }
}
