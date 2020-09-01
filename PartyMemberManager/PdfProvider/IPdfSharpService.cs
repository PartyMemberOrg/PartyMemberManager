using AspNetCorePdf.PdfProvider.DataModel;
using System.Collections.Generic;
using System.IO;

namespace AspNetCorePdf.PdfProvider
{
    public interface IPdfSharpService
    {
        Stream CreatePdf(IEnumerable<PdfData> pdfDatas, bool printBackground = false);
    }
}
