using PartyMemberManager.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PartyMemberManager.Dal.Entities
{
    public class Department:EntityBase
    {

        /// <summary>
        /// 部门名称
        /// </summary>
        [DisplayName("部门名称")]
        [StringLength(50, MinimumLength = 1, ErrorMessageResourceName = "StringLengthErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        [Required(ErrorMessageResourceName = "RequiredErrorMessage", ErrorMessageResourceType = typeof(Properties.Resources))]
        public string Name { get; set; }

        public string Password { get; set; }
        /// <summary>
        /// 所属校区
        /// </summary>
        [DisplayName("所属校区")]
        public SchoolArea SchoolAreas { get; set; }
        /// <summary>
        /// 所属校区
        /// </summary>
        [DisplayName("所属校区")]
        [NotMapped]
        public string SchoolAreasDisplay { get => SchoolAreas.ToString(); }
    }
}
