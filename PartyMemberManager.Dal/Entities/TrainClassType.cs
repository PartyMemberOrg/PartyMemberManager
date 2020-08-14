using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 培训班类型
    /// </summary>
    public class TrainClassType : EntityBase
    {

        /// <summary>
        /// 名称
        /// </summary>
        [DisplayName("名称")]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }
        /// <summary>
        /// Code
        /// </summary>
        [DisplayName("代码")]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Code { get; set; }

    }
}
