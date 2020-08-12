using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using PartyMemberManager.Core.Exceptions;
using PartyMemberManager.Core.Helpers;
using PartyMemberManager.Dal;
using PartyMemberManager.Dal.Entities;
using PartyMemberManager.Framework.Controllers;
using PartyMemberManager.Framework.Models.JsonModels;
using PartyMemberManager.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace PartyMemberManager.Controllers
{
    public class AccountController : PartyMemberControllerBase
    {

        public AccountController(ILogger<AccountController> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
        }
        [AllowAnonymous]
        public IActionResult Login()
        {
            LoginViewModel model = new LoginViewModel();
            return View(model);
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                //await AdminEnabled();
                var user = await _context.Operators.Where(m => m.Enabled).Where(m => m.LoginName == model.LoginName).FirstOrDefaultAsync();
                if (user != null)
                {
                    string password = DecryptPassword(model.Password);
                    if (user.Password.Equals(StringHelper.EncryPassword(password)))
                    {
                        //登陆成功的操作
                        //用户标识
                        var identity = new
                                           ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);//一定要声明AuthenticationScheme
                        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                        identity.AddClaim(new Claim(ClaimTypes.Surname, user.LoginName));
                        identity.AddClaim(new Claim(ClaimTypes.Name, user.Name));
                        identity.AddClaim(new Claim(ClaimTypes.Role, user.Roles.ToString()));

                        await HttpContext.SignInAsync(identity.AuthenticationType,
                                                      new ClaimsPrincipal(identity),
                                                      new AuthenticationProperties
                                                      {
                                                          IsPersistent = true,
                                                      });

                        if (String.IsNullOrEmpty(returnUrl))
                        {
                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            return Redirect(returnUrl);
                        }
                    }
                }
                await HttpContext.ChallengeAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                ModelState.AddModelError("LoginName", "账号或者密码不正确");
            }
            return View();
        }
        /// <summary>
        /// 管理员可用
        /// </summary>
        /// <returns></returns>
        private async Task AdminEnabled()
        {
            if (_context.Operators.Any())
            {
                if (_context.Operators.All(o => !o.Enabled))
                {
                    Operator operatorAdmin = await _context.Operators.FirstOrDefaultAsync(o => o.Roles == Core.Enums.Role.系统管理员);
                    operatorAdmin.Enabled = true;
                    await _context.SaveChangesAsync();
                }
                return;
            }
        }
        [AllowAnonymous]
        public IActionResult Forbidden()
        {
            return View();
        }
        [AllowAnonymous]
        public IActionResult LoginInForm()
        {
            LoginViewModel model = new LoginViewModel();
            return View(model);
        }

        public IActionResult Logout()
        {
            SignOut();
            return RedirectToAction("Login");
            //return new SignOutResult();
            //await HttpContext.SignOutAsync();
            //await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            //return View();
        }
        [HttpPost]
        public async Task<IActionResult> LogoutByAjax()
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "退出成功"
            };
            try
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                SignOut();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, ex.Message);
                jsonResult.Code = -1;
                jsonResult.Message = "退出时发生错误";
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
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LoginByAjax(LoginViewModel model, string returnUrl)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "登录成功"
            };
            try
            {
                if (ModelState.IsValid)
                {
                    //await AdminEnabled();
                    var user = await _context.Operators.Where(m => m.Enabled).Where(m => m.LoginName == model.LoginName).FirstOrDefaultAsync();
                    if (user != null)
                    {
                        string password = DecryptPassword(model.Password);
                        if (user.Password.Equals(StringHelper.EncryPassword(password)))
                        {
                            //登陆成功的操作
                            //用户标识
                            var identity = new
                                               ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme);//一定要声明AuthenticationScheme
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()));
                            identity.AddClaim(new Claim(ClaimTypes.Name, user.LoginName));
                            identity.AddClaim(new Claim("FullName", user.Name));
                            identity.AddClaim(new Claim(ClaimTypes.Role, user.Roles.ToString()));
                            await HttpContext.SignInAsync(identity.AuthenticationType,
                                                          new ClaimsPrincipal(identity),
                                                          new AuthenticationProperties
                                                          {
                                                              IsPersistent = model.IsRemember,
                                                          });

                        }
                        else
                            throw new PartyMemberException("LoginName", "账号或者密码错误");
                    }
                    else
                        throw new PartyMemberException("LoginName", "账号或者密码错误");
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, ex.Message);
                jsonResult.Code = -1;
                jsonResult.Message = "登录时发生错误";
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
        /// 解密密码(如果解密失败，会返回原字符串，所以不加密密码也可以)
        /// </summary>
        /// <param name="password">密码</param>
        /// <returns></returns>
        private static string DecryptPassword(string password)
        {
            //如果解密失败，会返回原字符串，所以不加密密码也可以
            return StringHelper.DESDecrypt(password);
        }

        /// <summary>
        /// 修改个人信息
        /// </summary>
        /// <returns></returns>
        // GET: Users/Create
        public async Task<IActionResult> EditUser()
        {
            Operator @operator = await _context.Operators.FindAsync(CurrentUser.Id);
            return View(@operator);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveUser(Guid id, [Bind("Name,LoginName,Phone,Password,Id")] Operator @operator)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "数据保存成功"
            };
            try
            {

                if (ModelState.IsValid)
                {
                    Operator userInDb = await _context.Operators.FindAsync(id);
                    if (userInDb != null)
                    {
                        userInDb.Name = @operator.Name;
                        userInDb.Phone = @operator.Phone;
                        if (@operator.Password != userInDb.Password)
                            userInDb.Password = StringHelper.EncryPassword(@operator.Password);
                        _context.Update(userInDb);
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        throw new PartyMemberException("", "未找到用户信息");
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

        [AllowAnonymous]
        public IActionResult Test()
        {
            return View();
        }
    }
}