using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PartyMemberManager.Core.Enums;
using PartyMemberManager.Core.Helpers;
using PartyMemberManager.Dal.Entities;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Microsoft.VisualBasic.CompilerServices;
using System.Net.Http.Headers;

namespace PartyMemberManager.Dal
{
    public static class DbInitializer
    {
        public static void Initialize(PMContext context)
        {
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            #region 初始化数据
            //初始化用户
            InitOperators(context);

            //初始化模块和权限
            InitModules(context);
            //初始化部门
            InitDepartments(context);
            //初始化民族
            InitNations(context);
            //初始化培训班类型
            InitTrainClassTypes(context);
            //初始化学期
            InitYearTerms(context);
            #endregion
            context.SaveChanges();
        }

        /// <summary>
        /// 初始化模块和权限
        /// </summary>
        /// <param name="context"></param>
        private static void InitModules(PMContext context)
        {
            if (context.Modules.Any())
                return;

            #region 初始化功能模块和权限
            //说明：如果只指定了controller，没有指定action，则该congtroller下的所有action均授权或者禁止权限
            Module module = new Module
            {
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "系统管理",
                Controller = null,
                Action = null,
                Ordinal = 1,
                Roles = Role.学校党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(module);
            Module childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "账号管理",
                Controller = "Operators",
                Action = null,
                Ordinal = 102,
                Roles = Role.学校党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "角色权限",
                Controller = "ModuleFunctions",
                Action = null,
                Ordinal = 103,
                Roles = Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "账号权限",
                Controller = "OperatorModules",
                Action = null,
                Ordinal = 104,
                Roles = Role.学校党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "部门管理",
                Controller = "Departments",
                Action = null,
                Ordinal = 105,
                Roles = Role.学校党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "培训班类型",
                Controller = "TrainClassTypes",
                Action = null,
                Ordinal = 106,
                Roles = Role.学校党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "民族管理",
                Controller = "Nations",
                Action = null,
                Ordinal = 107,
                Roles = Role.学校党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "学期管理",
                Controller = "YearTerms",
                Action = null,
                Ordinal = 108,
                Roles = Role.学校党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            module = new Module
            {
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "入党积极分子管理",
                Controller = null,
                Action = null,
                Ordinal = 2,
                Roles = Role.学校党委 | Role.学院党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(module);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "入党积极分子摸底",
                Controller = "ActiveApplicationSurveys",
                Action = null,
                Ordinal = 201,
                Roles = Role.学校党委 | Role.学院党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "入党积极分子名单",
                Controller = "PartyActivists",
                Action = null,
                Ordinal = 202,
                Roles = Role.学校党委 | Role.学院党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "入党积极分子培训班",
                Controller = "TrainClasses",
                Action = null,
                Ordinal = 203,
                Roles = Role.学校党委 | Role.学院党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "入党积极分子成绩",
                Controller = "ActivistTrainResults",
                Action = null,
                Ordinal = 204,
                Roles = Role.学校党委 | Role.学院党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);

            module = new Module
            {
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "发展对象管理",
                Controller = null,
                Action = null,
                Ordinal = 3,
                Roles = Role.学校党委 | Role.学院党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(module);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "发展对象名单",
                Controller = "PotentialMembers",
                Action = null,
                Ordinal = 301,
                Roles = Role.学校党委 | Role.学院党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "发展对象培训班",
                Controller = "TrainClasses",
                Action = null,
                Ordinal = 302,
                Roles = Role.学校党委 | Role.学院党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "发展对象培训成绩",
                Controller = "PotentialTrainResults",
                Action = null,
                Ordinal = 303,
                Roles = Role.学校党委 | Role.学院党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);


            module = new Module
            {
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "省级干部培训",
                Controller = null,
                Action = null,
                Ordinal = 4,
                Roles = Role.学校党委 | Role.系统管理员 | Role.超级管理员
            };
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "省级干部培训班",
                Controller = "ProvinceTrainClasses",
                Action = null,
                Ordinal = 403,
                Roles = Role.学校党委 | Role.学院党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            childModule = new Module
            {
                ParentModuleId = module.Id,
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "省级干部培训名单",
                Controller = "ProvinceCadreTrains",
                Action = null,
                Ordinal = 405,
                Roles = Role.学校党委 | Role.学院党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(childModule);
            context.Modules.Add(module);

            module = new Module
            {
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                Name = "领导干部培训",
                Controller = "SchoolCadreTrains",
                Action = null,
                Ordinal = 5,
                Roles = Role.学校党委 | Role.系统管理员 | Role.超级管理员
            };
            context.Modules.Add(module);
            #endregion
        }

        /// <summary>
        /// 初始化用户
        /// </summary>
        /// <param name="context"></param>
        private static void InitOperators(PMContext context)
        {
            if (context.Operators.Any())
                return;
            #region 初始化用户
            Operator @operator = new Operator
            {
                LoginName = "admin",
                Name = "Admin",
                Password = StringHelper.EncryPassword("pm123"),
                Roles = Core.Enums.Role.超级管理员,
                IsDeleted = false,
                CreateTime = DateTime.Now,
                Enabled = true,
                Ordinal = 1
            };
            context.Operators.Add(@operator);
            @operator = new Operator
            {
                LoginName = "sysadmin",
                Name = "SysAdmin",
                Password = StringHelper.EncryPassword("pm123"),
                Roles = Core.Enums.Role.系统管理员,
                IsDeleted = false,
                CreateTime = DateTime.Now,
                Enabled = true,
                Ordinal = 2
            };
            context.Operators.Add(@operator);
            @operator = new Operator
            {
                LoginName = "1074113104",
                Name = "测试学校党委",
                Password = StringHelper.EncryPassword("123"),
                Roles = Core.Enums.Role.学校党委,
                IsDeleted = false,
                CreateTime = DateTime.Now,
                Enabled = false,
                Ordinal = 3
            };
            context.Operators.Add(@operator);
            @operator = new Operator
            {
                LoginName = "1074113105",
                Name = "测试学院党委",
                Password = StringHelper.EncryPassword("123"),
                Roles = Core.Enums.Role.学院党委,
                IsDeleted = false,
                CreateTime = DateTime.Now,
                Enabled = false,
                Ordinal = 4
            };
            context.Operators.Add(@operator);
            #endregion
        }

        private static void InitDepartments(PMContext context)
        {
            if (context.Departments.Any())
                return;

            #region 部门
            string depList = "机关党委 00,后勤处党总支 00,离退休工作党委 00,会计学院党委 09,金融学院党委 02,统计学院党委 06,工商管理学院党委 08,经济学院党委 21,国际经济与贸易学院党委 01,信息工程学院党委 07,财税与公共管理学院党委 10,法学院党委 03,马克思主义学院党委 14,商务传媒学院党委 12,外语学院党委 04,艺术学院党委 05,农林经济管理学院党委 11,国际教育学院（中亚商学院）直属党支部 16,继续教育学院直属党支部 00,体育教学部直属党支部 13,MBA教育中心直属党支部 00,创新创业学院直属党支部 14,图书馆直属党支部 00,信息中心直属党支部 00,档案馆直属党支部 00,长青学院党总支 00,陇桥学院党总支 00 ";
            string[] depListSplit = depList.Split(",");

            foreach (var item in depListSplit)
            {
                Department department = new Department
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.Now,
                    Ordinal = context.Departments.Local.Count() + 1,
                    Name = item.Split(" ")[0],
                    Code = item.Split(" ")[1]
                };
                if (item.Contains("长青"))
                {
                    department.SchoolArea = SchoolArea.长青学院;
                }
                else if (item.Contains("陇桥"))
                {
                    department.SchoolArea = SchoolArea.陇桥学院;
                }
                else if (item.Contains("信息工程") || item.Contains("艺术"))
                {
                    department.SchoolArea = SchoolArea.段家滩;
                }
                else
                {
                    department.SchoolArea = SchoolArea.和平;
                }

                context.Departments.Add(department);
            }
            #endregion
        }
        private static void InitNations(PMContext context)
        {
            if (context.Nations.Any())
                return;

            #region 民族
            string nationList = "01汉族,02蒙古族,03回族,04藏族,05维吾尔族,06苗族,07彝族,08壮族,09布依族,10朝鲜族,11满族,12侗族,13瑶族,14白族,15土家族,16哈尼族,17哈萨克族,18傣族 ,19黎族,20傈僳族,21佤族 ,22畲族,23高山族,24拉祜族,25水族,26东乡族,27纳西族,28景颇族,29柯尔克孜族,30土族,31达斡尔族,32仫佬族,33羌族,34布朗族,35撒拉族,36毛难族,37仡佬族,38锡伯族,39阿昌族,40普米族,41塔吉克族,42怒族,43乌孜别克族,44俄罗斯族,45鄂温克族,46崩龙族,47保安族,48裕固族,49京族,50塔塔尔族,51独龙族,52鄂伦春族,53赫哲族,54门巴族,55珞巴族,56基诺族,97其他,98外国血统";
            string[] nationListSplit = nationList.Split(",");
            foreach (var item in nationListSplit)
            {

                Nation nation = new Nation
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.Now,
                };
                nation.Code = item.Trim().Substring(0, 2);
                nation.Name = item.Trim().Substring(2);
                nation.Ordinal = int.Parse(nation.Code);
                context.Nations.Add(nation);
            }
            #endregion
        }
        private static void InitTrainClassTypes(PMContext context)
        {
            if (context.TrainClassTypes.Any())
                return;

            #region 培训班类型
            string classTypeList = "处级干部培训班 21,科级干部培训班 22,党务干部培训班 31,分党委（党总支）书记培训班 32,党务秘书培训班 33,入党积极分子培训班 41,发展对象培训班 42";
            string[] classTypeListSplit = classTypeList.Split(",");
            foreach (var item in classTypeListSplit)
            {

                TrainClassType trainClassType = new TrainClassType
                {
                    Id = Guid.NewGuid(),
                    CreateTime = DateTime.Now,
                    Name = item.Split(" ")[0].Trim(),
                    Code = item.Split(" ")[1].Trim()
                };
                trainClassType.Ordinal = int.Parse(trainClassType.Code);
                context.TrainClassTypes.Add(trainClassType);
            }
            #endregion
        }

        private static void InitYearTerms(PMContext context)
        {
            if (context.YearTerms.Any())
                return;

            #region 初始化学期
            //计算当前的学年和学期
            int year = DateTime.Today.Year;
            int month = DateTime.Today.Month;
            Term term = Term.第一学期;
            if (month > 2 && month < 8)
            {
                term = Term.第二学期;
                year--;
            }
            else
            {
                term = Term.第一学期;
                if (month <= 2)
                    year--;
            }
            YearTerm yearTerm = new YearTerm
            {
                Id = Guid.NewGuid(),
                CreateTime = DateTime.Now,
                IsDeleted = false,
                Enabled=true,
                Ordinal = 1,
                StartYear = year,
                Term = term
            };
            context.YearTerms.Add(yearTerm);
            #endregion
        }

    }
}
