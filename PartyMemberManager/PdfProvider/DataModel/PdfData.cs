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
        public string DocumentTitle { get; set; }

        public string CreatedBy { get; set; }

        public string Description { get; set; }

        public List<DisplayItem>  DisplayItems { get; set; }
        public string DocumentName { get; set; }
        public string BackgroundImage { get; set; }
    }
}
