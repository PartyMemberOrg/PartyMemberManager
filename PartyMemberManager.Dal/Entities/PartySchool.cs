using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 党校
    /// </summary>
    public class PartySchool : EntityBase
    {
        /// <summary>
        /// 党校名称
        /// </summary>
        [DisplayName("党校名称")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }

        /// <summary>
        /// 党校代码
        /// </summary>
        [DisplayName("党校代码")]
        public string Code { get; set; }
    }
}
