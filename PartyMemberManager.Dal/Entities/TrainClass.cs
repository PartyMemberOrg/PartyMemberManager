using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    /// <summary>
    /// 培训班
    /// </summary>
    public class TrainClass : EntityBase
    {
        /// <summary>
        /// 培训班名称
        /// </summary>
        [DisplayName("培训班名称")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }

        /// <summary>
        /// 培训班代码
        /// </summary>
        [DisplayName("培训班代码")]
        [StringLength(2, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Code { get; set; }

        /// <summary>
        /// 培训班类型
        /// </summary>
        public Guid TrainClassTypeId { get; set; }

        /// <summary>
        /// 培训班类型
        /// </summary>
        public TrainClassType TrainClassType { get; set; }
    }
}
