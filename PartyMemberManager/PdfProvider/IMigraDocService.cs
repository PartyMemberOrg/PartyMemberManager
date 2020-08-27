using AspNetCorePdf.PdfProvider.DataModel;
using System.Collections.Generic;
using System.IO;

namespace AspNetCorePdf.PdfProvider
{
    public interface IMigraDocService
    {
        Stream CreateMigraDocPdf(IEnumerable<PdfData> pdfDatas);
    }
}
