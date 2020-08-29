using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PartyMemberManager.Models
{
    public class SchoolCadreTranImportViewModel
    {
        /// <summary>
        /// 文件
        /// </summary>
        [DisplayName("文件")]
        [Required]
        public IFormFile File { get; set; }
    }
}
