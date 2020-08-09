using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class Log : EntityBase
    {
        /// <summary>
        /// 日志级别
        /// </summary>
        [DisplayName("日志级别")]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceName = "StringLengthErrorMessage")]
        public string LogLevel { get; set; }
        /// <summary>
        /// 分类
        /// </summary>
        [DisplayName("分类")]
        [StringLength(500, MinimumLength = 2, ErrorMessageResourceName = "StringLengthErrorMessage")]
        public string CategoryName { get; set; }
        /// <summary>
        /// 信息
        /// </summary>
        [DisplayName("信息")]
        [StringLength(5000, MinimumLength = 2, ErrorMessageResourceName = "StringLengthErrorMessage")]
        public string Message { get; set; }
        /// <summary>
        /// 用户
        /// </summary>
        [DisplayName("用户")]
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceName = "StringLengthErrorMessage")]
        public string User { get; set; }
    }
}
