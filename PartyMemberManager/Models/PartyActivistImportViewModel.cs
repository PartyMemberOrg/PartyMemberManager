using NPOI.Util.Collections;
using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PartyMemberManager.Models
{
    /// <summary>
    /// 入党积极分子数据导入视图模型
    /// </summary>
    public class PartyActivistImportViewModel
    {

        /// <summary>
        /// 年度
        /// </summary>
        [DisplayName("年度")]
        [Range(1900,2999)]
        public int YearBegin { get; set; }
        /// <summary>
        /// 年度
        /// </summary>
        [DisplayName("年度")]
        public int YearEnd { get => YearBegin + 1; }
        /// <summary>
        /// 学期
        /// </summary>
        [DisplayName("学期")]
        public Term? Term { get; set; }

        /// <summary>
        /// 培训班类型
        /// </summary>
        [DisplayName("培训班类型")]
        public Guid? TrainClassTypeId { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        [DisplayName("部门")]
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// 所属培训班
        /// </summary>
        [DisplayName("培训班")]
        public Guid TrainClassId { get; set; }
    }
}
