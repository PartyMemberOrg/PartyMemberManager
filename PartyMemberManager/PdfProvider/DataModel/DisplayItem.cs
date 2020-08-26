using DocumentFormat.OpenXml.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyMemberManager.PdfProvider.DataModel
{
    /// <summary>
    /// 显示项
    /// </summary>
    public class DisplayItem
    {
        public System.Drawing.Point Location { get; set; }
        public string Font { get; set; }
        public float FontSize { get; set; }
        public string Text { get; set; }
    }
}
