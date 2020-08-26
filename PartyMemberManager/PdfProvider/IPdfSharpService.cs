using AspNetCorePdf.PdfProvider.DataModel;
using System.IO;

namespace AspNetCorePdf.PdfProvider
{
    public interface IPdfSharpService
    {
        Stream CreatePdf(PdfData pdfData);
    }
}
