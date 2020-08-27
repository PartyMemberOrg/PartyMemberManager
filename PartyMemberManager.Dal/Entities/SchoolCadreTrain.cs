using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class SchoolCadreTrain : EntityBase
    {
        /// <summary>
        /// 年份
        /// </summary>
        [DisplayName("年份")]
        [StringLength(4, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Year { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [DisplayName("姓名")]
        [StringLength(20, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }
        /// <summary>
        /// 培训班次
        /// </summary>
        [DisplayName("培训班次")]
        [StringLength(100, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string TrainClassName { get; set; }
       
        /// <summary>
        /// 组织单位
        /// </summary>
        [DisplayName("组织单位")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Organizer { get; set; }

        /// <summary>
        /// 培训单位
        /// </summary>
        [DisplayName("培训单位")]
        [StringLength(100, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string TrainOrganizational { get; set; }

        /// <summary>
        /// 培训时间
        /// </summary>
        [DisplayName("培训时间")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public DateTime? TrainTime { get; set; }

        /// <summary>
        /// 地点
        /// </summary>
        [DisplayName("地点")]
        [StringLength(200, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string TrainAddress { get; set; }

        /// <summary>
        /// 培训时长
        /// </summary>
        [DisplayName("培训时长(天)")]
        [Range(1,1000, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]

        public int TrainDuration { get; set; }

        /// <summary>
        ///学时
        /// </summary>
        [DisplayName("学时")]
        public int ClassHour { get; set; }


    }
}
