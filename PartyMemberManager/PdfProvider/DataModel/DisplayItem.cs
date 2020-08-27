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
        /// <summary>
        /// 显示位置（单位毫米)
        /// </summary>
        public System.Drawing.Point Location { get; set; }
        public string Font { get; set; }
        /// <summary>
        /// 字体大小（单位用point)
        /// </summary>
        public float FontSize { get; set; }
        public string Text { get; set; }
    }
}
