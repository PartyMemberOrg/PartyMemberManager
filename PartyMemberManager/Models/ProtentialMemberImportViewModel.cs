using Microsoft.AspNetCore.Http;
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
    /// 发展对象数据导入视图模型
    /// </summary>
    public class ProtentialMemberImportViewModel
    {

        /// <summary>
        /// 学期
        /// </summary>
        [DisplayName("学期")]
        public Guid? YearTermId { get; set; }

        ///// <summary>
        ///// 培训班类型
        ///// </summary>
        //[DisplayName("培训班类型")]
        //public Guid? TrainClassTypeId { get; set; }

        /// <summary>
        /// 部门
        /// </summary>
        [DisplayName("部门")]
        public Guid? DepartmentId { get; set; }

        /// <summary>
        /// 批次
        /// </summary>
        [DisplayName("批次")]
        [Required]
        public BatchType Batch { get; set; }

        /// <summary>
        /// 所属培训班
        /// </summary>
        [DisplayName("培训班")]
        [Required]
        public Guid TrainClassId { get; set; }
        /// <summary>
        /// 类型学生或者教师
        /// </summary>
        [DisplayName("类型")]
        [Required]
        public PartyMemberType PartyMemberType { get; set; }
        /// <summary>
        /// 文件
        /// </summary>
        [DisplayName("文件")]
        [Required]
        public IFormFile File { get; set; }
    }
}
