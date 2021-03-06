﻿using AspNetCorePdf.PdfProvider.DataModel;
using DocumentFormat.OpenXml.Wordprocessing;
using MigraDoc.DocumentObjectModel;
using PartyMemberManager.PdfProvider.DataModel;
using PdfSharp.Drawing;
using PdfSharp.Drawing.Layout;
using PdfSharp.Fonts;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;

namespace AspNetCorePdf.PdfProvider
{
    public class PdfSharpService : IPdfSharpService
    {
        private string _createdDocsPath = $".{Path.DirectorySeparatorChar}PdfProvider{Path.DirectorySeparatorChar}Created";
        private string _imagesPath = $".{Path.DirectorySeparatorChar}PdfProvider{Path.DirectorySeparatorChar}Images";
        private string _resourcesPath = $".{Path.DirectorySeparatorChar}PdfProvider{Path.DirectorySeparatorChar}Resources";
        public PdfSharpService()
        {
            string basePath = AppContext.BaseDirectory;
            _createdDocsPath = System.IO.Path.Combine(basePath, $"PdfProvider{Path.DirectorySeparatorChar}Created");
            _imagesPath = System.IO.Path.Combine(basePath, $"PdfProvider{Path.DirectorySeparatorChar}Images");
            _resourcesPath = System.IO.Path.Combine(basePath, $"PdfProvider{Path.DirectorySeparatorChar}Resources");
        }

        public Stream CreatePdf(IEnumerable<PdfData> pdfDatas, bool printBackground = false)
        {
            if (GlobalFontSettings.FontResolver == null)
            {
                GlobalFontSettings.FontResolver = new FontResolver(_resourcesPath);
            }

            var document = new PdfDocument();
            document.Info.Author = "预备党员管理系统";
            document.PageMode = PdfPageMode.FullScreen;
            document.Settings.TrimMargins = new TrimMargins { All = new XUnit(0, XGraphicsUnit.Millimeter) };
            foreach (PdfData pdfData in pdfDatas)
            {
                var page = document.AddPage();
                page.Rotate = pdfData.Rotate;
                //page.Orientation = PdfSharp.PageOrientation.Landscape;
                page.TrimMargins = new TrimMargins { All = new XUnit(0, XGraphicsUnit.Millimeter) };
                //A4 new XSize(595, 842);，从mm转换
                page.Width = new XUnit(pdfData.PageSize.Width, XGraphicsUnit.Millimeter);// (int)(pdfData.PageSize.Width * scale);
                page.Height = new XUnit(pdfData.PageSize.Height, XGraphicsUnit.Millimeter);// (int)(pdfData.PageSize.Height * scale);
                var gfx = XGraphics.FromPdfPage(page, XGraphicsUnit.Millimeter);
                //暂时不打印背景
                if (printBackground)
                {
                    if (!string.IsNullOrEmpty(pdfData.BackgroundImage))
                        AddBackground(gfx, page, $"{_imagesPath}{Path.DirectorySeparatorChar}{pdfData.BackgroundImage}", 0, 0);
                }
                //AddTitleAndFooter(page, gfx, pdfData.DocumentTitle, document, pdfData);
                //AddDescription(gfx, pdfData);
                AddText(gfx, pdfData);
            }

            //string docName = $"{_createdDocsPath}/{pdfData.DocumentName}-{DateTime.UtcNow.ToOADate()}.pdf";
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
            Unit imageWidth = Unit.FromPoint(xImage.PointWidth);
            Unit imageHeight = Unit.FromPoint(xImage.PointHeight);
            double imageHeightScale = imageHeight / page.Height;
            double imageWightScale = imageWidth / page.Width;
            double imageScale = imageHeightScale;
            if (imageWightScale > imageHeightScale)
                imageScale = imageWightScale;
            //gfx.DrawImage(xImage, xPosition, yPosition, xImage.PixelWidth / imageScale, xImage.PixelHeight / imageScale);
            XRect xrectSource = new XRect { X = xPosition, Y = yPosition, Width = imageWidth.Millimeter, Height = imageHeight.Millimeter };
            XRect xrect = new XRect { X = xPosition, Y = yPosition, Width = imageWidth.Millimeter / imageScale, Height = imageHeight.Millimeter / imageScale };

            gfx.DrawImage(xImage, xrect, xrect, XGraphicsUnit.Millimeter);
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
            int listItemHeight = 15;

            for (int i = 0; i < pdfData.DisplayItems.Count; i++)
            {
                DisplayItem displayItem = pdfData.DisplayItems[i];
                string fontName = displayItem.Font;
                if (string.IsNullOrEmpty(fontName))
                    fontName = "宋体";
                //将字体的大小由点转换为毫米（为方便，统一使用毫米作为单位）
                Unit fontSize = new Unit(displayItem.FontSize, UnitType.Point);
                var font = new XFont(displayItem.Font, fontSize.Millimeter, XFontStyle.Regular);
                XTextFormatter tf = new XTextFormatter(gfx);
                XRect rect = new XRect(displayItem.Location.X, displayItem.Location.Y, 400, listItemHeight);
                //gfx.DrawRectangle(XBrushes.White, rect);
                var data = $"{displayItem.Text}";
                tf.DrawString(data, font, XBrushes.Black, rect, XStringFormats.TopLeft);
            }
        }

    }
}
