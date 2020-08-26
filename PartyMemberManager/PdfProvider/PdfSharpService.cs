using AspNetCorePdf.PdfProvider.DataModel;
using DocumentFormat.OpenXml.Wordprocessing;
using PartyMemberManager.PdfProvider.DataModel;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System;
using System.IO;

namespace AspNetCorePdf.PdfProvider
{
    public class PdfSharpService : IPdfSharpService
    {
        private string _createdDocsPath = ".\\PdfProvider\\Created";
        private string _imagesPath = ".\\PdfProvider\\Images";
        private string _resourcesPath = ".\\PdfProvider\\Resources";
        private double scale = 2.835016835016835;

        public Stream CreatePdf(PdfData pdfData)
        {
            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new FontResolver(_resourcesPath);
            }

            var document = new PdfDocument();
            var page = document.AddPage();
            //A4 new XSize(595, 842);，从mm转换
            page.Width = (int)(pdfData.PageSize.Width * scale);
            page.Height = (int)(pdfData.PageSize.Height * scale);
            var gfx = XGraphics.FromPdfPage(page);
            if (!string.IsNullOrEmpty(pdfData.BackgroundImage))
                AddBackground(gfx, page, $"{_imagesPath}\\{pdfData.BackgroundImage}", 0, 0);
            //AddTitleAndFooter(page, gfx, pdfData.DocumentTitle, document, pdfData);
            //AddDescription(gfx, pdfData);
            AddText(gfx, pdfData);

            string docName = $"{_createdDocsPath}/{pdfData.DocumentName}-{DateTime.UtcNow.ToOADate()}.pdf";
            MemoryStream memoryStream = new MemoryStream();
            //document.Save(docName);
            document.Save(memoryStream);
            //return docName;
            return memoryStream;
        }

        void AddBackground(XGraphics gfx, PdfPage page, string imagePath, int xPosition, int yPosition)
        {
            if (!File.Exists(imagePath))
            {
                throw new FileNotFoundException(String.Format("Could not find image {0}.", imagePath));
            }

            XImage xImage = XImage.FromFile(imagePath);
            gfx.DrawImage(xImage, xPosition, yPosition, xImage.PixelWidth / 4, xImage.PixelHeight / 4);
        }

        void AddTitleAndFooter(PdfPage page, XGraphics gfx, string title, PdfDocument document, PdfData pdfData)
        {
            XRect rect = new XRect(new XPoint(), gfx.PageSize);
            rect.Inflate(-10, -15);
            XFont font = new XFont("楷体", 14, XFontStyle.Bold);
            gfx.DrawString(title, font, XBrushes.MidnightBlue, rect, XStringFormats.TopCenter);

            rect.Offset(0, 5);
            font = new XFont("楷体", 8, XFontStyle.Italic);
            XStringFormat format = new XStringFormat();
            format.Alignment = XStringAlignment.Near;
            format.LineAlignment = XLineAlignment.Far;
            gfx.DrawString("Created by " + pdfData.CreatedBy, font, XBrushes.DarkOrchid, rect, format);

            font = new XFont("楷体", 8);
            format.Alignment = XStringAlignment.Center;
            gfx.DrawString(document.PageCount.ToString(), font, XBrushes.DarkOrchid, rect, format);

            document.Outlines.Add(title, page, true);
        }

        void AddDescription(XGraphics gfx, PdfData pdfData)
        {
            var font = new XFont("黑体", 14, XFontStyle.Regular);
            XTextFormatter tf = new XTextFormatter(gfx);
            XRect rect = new XRect(40, 100, 520, 100);
            gfx.DrawRectangle(XBrushes.White, rect);
            tf.DrawString(pdfData.Description, font, XBrushes.Black, rect, XStringFormats.TopLeft);
        }

        void AddText(XGraphics gfx, PdfData pdfData)
        {
            int listItemHeight = 30;

            for (int i = 0; i < pdfData.DisplayItems.Count; i++)
            {
                DisplayItem displayItem = pdfData.DisplayItems[i];
                string fontName = displayItem.Font;
                if (string.IsNullOrEmpty(fontName))
                    fontName = "宋体";
                var font = new XFont(displayItem.Font, displayItem.FontSize, XFontStyle.Regular);
                XTextFormatter tf = new XTextFormatter(gfx);
                XRect rect = new XRect(displayItem.Location.X * scale, displayItem.Location.Y * scale, 500, listItemHeight);
                //gfx.DrawRectangle(XBrushes.White, rect);
                var data = $"{displayItem.Text}";
                tf.DrawString(data, font, XBrushes.Black, rect, XStringFormats.TopLeft);
            }
        }

    }
}
