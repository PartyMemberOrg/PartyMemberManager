using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PartyMemberManager.Core.Enums
{
    /// <summary>
    /// 用户角色
    /// </summary>
    //[Flags]
    public enum Role
    {
        学院党委 = 1,
        学校党委 = 2,
        系统管理员 = 4,
        超级管理员 = 8
    }
    /// <summary>
    /// 性别
    /// </summary>
    public enum Sex
    {
        男 = 1,
        女 = 2
    }
    /// <summary>
    /// 学期
    /// </summary>
    public enum Term
    {
        第一学期 = 1,
        第二学期 = 2
    }
    /// <summary>
    /// 校区
    /// </summary>
    public enum SchoolArea
    {
        和平 = 1,
        段家滩 = 2,
        长青学院 = 4,
        陇桥学院 = 8
    }
    /// <summary>
    /// 干部培训类型
    /// </summary>
    public enum CadreTrainType
    {
        省级干部培训 = 1,
        校级干部培训 = 2,
    }

    /// <summary>
    /// 对象类型
    /// </summary>
    public enum PartyMemberType
    {
        本科生 = 1,
        教师 = 2,
        研究生 = 3,
        预科生 = 4
    }
    /// <summary>
    /// 学校干部培训类型
    /// </summary>
    public enum SchoolCadreTrainType
    {
        校领导 = 1,
        正处级 = 2,
        副处级 = 3,
        正科级 = 4,
        副科级 = 5,
        普通教师 = 6
    }
    /// <summary>
    /// 批次
    /// </summary>
    public enum BatchType
    {
        第一期 = 1,
        第二期 = 2,
        第三期 = 3,
        第四期 = 4
    }

    /// <summary>
    /// 授课类型
    /// </summary>
    public enum CourseType
    {
        入党积极分子培训 = 1,
        发展对象培训 = 2,
        省级干部培训 = 3,
    }
    /// <summary>
    /// 授权类型
    /// </summary>
    public enum RightType
    {
        [Display(Name = "授权")]
        Grant = 0,
        [Display(Name = "禁止权限")]
        Deny = 1
    }

    /// <summary>
    /// 入党积分分子状态
    /// </summary>
    public enum ActivistTrainStatus
    {
        成绩不合格 = 0,
        成绩合格 = 1,
        已打印 = 2,
        已列为发展对象 = 4,
        成绩合格并打印 = 成绩合格 + 已打印,
        成绩合格并列为发展对象 = 成绩合格 + 已打印 + 已列为发展对象
    }
}
