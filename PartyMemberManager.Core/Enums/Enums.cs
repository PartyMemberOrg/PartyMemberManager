﻿using System;
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
        学校党委 = 1,
        学院党委 = 2,
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
        长青学院=3,
        陇桥学院=4
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
}
