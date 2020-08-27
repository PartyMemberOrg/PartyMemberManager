using PartyMemberManager.Models.PrintViewModel;
using PartyMemberManager.PdfProvider.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspNetCorePdf.PdfProvider.DataModel
{
    public class PdfData
    {
        /// <summary>
        /// 单位使用mm（毫米）
        /// </summary>
        public System.Drawing.Size PageSize { get; set; }
        public string DocumentTitle { get; set; }

        public string CreatedBy { get; set; }

        public string Description { get; set; }

        public List<DisplayItem>  DisplayItems { get; set; }
        public string DocumentName { get; set; }
        public string BackgroundImage { get; set; }
    }
}
