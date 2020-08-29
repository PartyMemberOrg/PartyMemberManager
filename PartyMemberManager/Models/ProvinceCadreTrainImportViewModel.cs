using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PartyMemberManager.Models
{
    /// <summary>
    /// 省级干部培训班导入模型
    /// </summary>
    public class ProvinceCadreTrainImportViewModel
    {

        /// <summary>
        /// 年份
        /// </summary>
        [DisplayName("年份")]
        public int? Year { get; set; }

        /// <summary>
        /// 所属培训班
        /// </summary>
        [DisplayName("培训班")]
        [Required]
        public Guid ProvinceTrainClassId { get; set; }
        /// <summary>
        /// 文件
        /// </summary>
        [DisplayName("文件")]
        [Required]
        public IFormFile File { get; set; }
    }
}
