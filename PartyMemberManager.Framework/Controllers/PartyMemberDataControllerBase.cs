using EntityExtension;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PartyMemberManager.Core.Enums;
using PartyMemberManager.Core.Exceptions;
using PartyMemberManager.Dal;
using PartyMemberManager.Dal.Entities;
using PartyMemberManager.Framework.Models.JsonModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PartyMemberManager.Framework.Controllers
{
    /// <summary>
    /// 和数据库操作有关的控制器基类
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class PartyMemberDataControllerBase<TEntity> : PartyMemberControllerBase
            where TEntity : EntityBase
    {
        /// <summary>
        /// 当前用户角色
        /// </summary>
        protected Role Roles = Role.学校党委;
        /// <summary>
        /// 过滤器
        /// </summary>
        protected virtual Expression<Func<TEntity, bool>> Filter { get; set; }
        public PartyMemberDataControllerBase(ILogger<PartyMemberDataControllerBase<TEntity>> logger, PMContext context, IHttpContextAccessor accessor) : base(logger, context, accessor)
        {
            if (CurrentUser != null)
                Roles = CurrentUser.Roles;
            //打了删除标记的记录不显示
            if (CurrentUser == null)
                Filter = p => !p.IsDeleted;
            else
            {
                Guid userId = CurrentUser.Id;
            }
        }

        /// <summary>
        /// 数据过滤器
        /// </summary>
        public virtual Expression<Func<TEntity, bool>> DataFilter
        {
            get
            {
                return e => true;
            }
        }

        /// <summary>
        /// 返回数据列表
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        protected Task<PagedDataViewModel<TEntity>> List(int page)
        {
            return _context.Set<TEntity>().Where(DataFilter).OrderBy(o => o.Ordinal).GetPagedDataAsync(page);
        }

        /// <summary>
        /// 校验新创建的对象
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        protected virtual bool ValidateCreateObject(TEntity entity)
        {
            return ModelState.IsValid;
        }
        /// <summary>
        /// 通用数据创建控制器
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateObject(TEntity entity)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ValidateCreateObject(entity);
                    entity.Id = Guid.NewGuid();
                    entity.CreateTime = DateTime.Now;
                    entity.Ordinal = _context.Set<TEntity>().Count() + 1;
                    _context.Add(entity);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EntityExists(entity.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (PartyMemberException ex)
                {
                    ModelState.AddModelError(ex.Key, ex.Message);
                }
                catch (Exception ex)
                {
                    ShowAndLogSystemError(ex);
                }
            }
            return View(entity);
        }

        /// <summary>
        /// 检查实体是否存在
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        private bool EntityExists(Guid id)
        {
            return _context.Set<TEntity>().Any(e => e.Id == id);
        }

        /// <summary>
        /// 获取数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        public virtual async Task<IActionResult> GetDatas(int page = 1, int limit = 10)
        {
            JsonResultDatasModel<TEntity> jsonResult = new JsonResultDatasModel<TEntity>
            {
                Code = 0,
                Msg = ""
            };

            try
            {
                var data = await _context.Set<TEntity>().Where(DataFilter).OrderBy(o => o.Ordinal).GetPagedDataAsync(page, limit);
                if (data == null)
                    throw new PartyMemberException("未找到数据");
                jsonResult.Count = _context.Set<TEntity>().Where(DataFilter).Count();
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


        /// <summary>
        /// 删除数据（通过ajax调用)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost, ActionName("Delete")]
        public virtual async Task<IActionResult> Delete(Guid? id)
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
                var data = await _context.Set<TEntity>().Where(DataFilter).SingleOrDefaultAsync(m => m.Id == id);
                if (data == null)
                    throw new PartyMemberException("未找到要删除的数据");
                ValidateDeleteObject(data);
                _context.Set<TEntity>().Remove(data);
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
        /// 事项上移或下移
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isUp"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> ItemUpDown(Guid id, bool isUp = true)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "上下移动成功"
            };
            try
            {
                if (ModelState.IsValid)
                {
                    TEntity entityInDb = await _context.Set<TEntity>().FindAsync(id);
                    int ordinal = entityInDb.Ordinal;
                    if (isUp)
                    {
                        TEntity entityPrev = await _context.Set<TEntity>().Where(d => d.Ordinal < ordinal).OrderByDescending(d => d.Ordinal).FirstOrDefaultAsync();
                        if (entityPrev == null)
                            throw new PartyMemberException("已经是第一条数据了");
                        entityInDb.Ordinal = entityPrev.Ordinal;
                        entityPrev.Ordinal = ordinal;
                    }
                    else
                    {
                        TEntity entityNext = await _context.Set<TEntity>().Where(d => d.Ordinal > ordinal).OrderBy(d => d.Ordinal).FirstOrDefaultAsync();
                        if (entityNext == null)
                            throw new PartyMemberException("已经是最后一条数据了");
                        entityInDb.Ordinal = entityNext.Ordinal;
                        entityNext.Ordinal = ordinal;
                    }
                    await _context.SaveChangesAsync();
                }
                else
                {
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
        /// 事项上移或下移
        /// </summary>
        /// <param name="id"></param>
        /// <param name="isUp"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual async Task<IActionResult> ItemChangePosition(Guid id, int oldOrdinal, int newOrdinal)
        {
            JsonResultNoData jsonResult = new JsonResultNoData
            {
                Code = 0,
                Message = "移动成功"
            };
            try
            {
                if (ModelState.IsValid)
                {
                    //数据库中索引从1开始，因此做位置转换
                    int newOrdinalInDb = newOrdinal + 1;
                    TEntity entityInDb = await _context.Set<TEntity>().FindAsync(id);
                    int ordinal = entityInDb.Ordinal;
                    List<TEntity> entities = null;
                    if (newOrdinal < oldOrdinal)
                        entities = await _context.Set<TEntity>().Where(d => d.Ordinal >= newOrdinalInDb && d.Ordinal < ordinal && d.Id != id).OrderByDescending(d => d.Ordinal).ToListAsync();
                    else
                        entities = await _context.Set<TEntity>().Where(d => d.Ordinal > ordinal && d.Ordinal <= newOrdinalInDb && d.Id != id).OrderByDescending(d => d.Ordinal).ToListAsync();

                    foreach (TEntity entity in entities)
                    {
                        if (newOrdinal < oldOrdinal)
                            entity.Ordinal++;
                        else
                            entity.Ordinal--;
                    }
                    entityInDb.Ordinal = newOrdinalInDb;
                    await _context.SaveChangesAsync();
                }
                else
                {
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
        /// 检查要删除的对象是否可以被删除，如果不可以删除则抛出异常
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        protected virtual void ValidateDeleteObject(TEntity data)
        {

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<IActionResult> Save(TEntity entity)
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
                    TEntity entityDb = await _context.Set<TEntity>().FindAsync(entity.Id);
                    if (entityDb != null)
                    {
                        _context.Update(entity);
                    }
                    else
                    {
                        //department.Id = Guid.NewGuid();
                        _context.Add(entity);
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
    }
}
