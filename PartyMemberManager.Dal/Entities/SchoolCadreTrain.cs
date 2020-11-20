using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
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
        [StringLength(50, MinimumLength = 2, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }

        /// <summary>
        /// 职级
        /// </summary>
        [DisplayName("职级")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public SchoolCadreTrainType SchoolCadreTrainType { get; set; }
        /// <summary>
        /// 职级
        /// </summary>
        [DisplayName("职级")]
        [NotMapped]
        public string SchoolCadreTrainTypeDisplay { get => SchoolCadreTrainType.ToString(); }
        /// <summary>
        /// 培训班次
        /// </summary>
        [DisplayName("培训班名称")]
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
        /// 培训开始时间
        /// </summary>
        [DisplayName("培训开始日期")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? TrainTime { get; set; }

        /// <summary>
        /// 培训结束时间
        /// </summary>
        [DisplayName("培训结束日期")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        public DateTime? EndTrainTime { get; set; }

        /// <summary>
        /// 地点
        /// </summary>
        [DisplayName("培训地点")]
        [StringLength(200, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string TrainAddress { get; set; }

        /// <summary>
        /// 培训时长
        /// </summary>
        [DisplayName("培训时长(天)")]
        public int TrainDuration { get { return (TrainTime.HasValue && EndTrainTime.HasValue) ? (Convert.ToDateTime(EndTrainTime.Value.ToShortDateString()) - Convert.ToDateTime(TrainTime.Value.ToShortDateString())).Days+1 : 0; } }

        /// <summary>
        ///学时b
        /// </summary>
        [DisplayName("学时")]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Range(0, 500, ErrorMessageResourceName = "RangeErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public int ClassHour { get; set; }


    }
}
