using QRCoder;
using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;

public class InvoiceQRCodeGenerator
{
    /// <summary>
    /// Generates a QR code containing invoice information and embeds it into a PDF file.
    /// The method reads invoice details, generates a QR code, converts it into an image,
    /// and embeds this image at a specified position in the provided PDF document.
    /// </summary>
    /// <param name="pdfOutputPath">The file path of the PDF document to modify.</param>
    /// <param name="invoiceDetails">The InvoiceDetails object containing invoice information.</param>
    public void GenerateInvoiceQRCode(string pdfOutputPath, InvoiceDetails invoiceDetails)
    {
        // Generate the QR code data string
        string qrCodeData = $"SPC\n0200\n1\nCH5800791123000889012\nS\nRaiffeisen\nBahnhofstrasse\n8021\nZürich\nCH\n\n\n\n{invoiceDetails.FormatCurrency(invoiceDetails.TotalPriceInclMWST)}\nCHF\n\nInvoice for services\n\n\n\n{invoiceDetails.Recipient}\n\n{invoiceDetails.Street}\n{invoiceDetails.Place}\nCH\n";

        // Generate QR Code using QRCoder
        QRCodeGenerator qrGenerator = new QRCodeGenerator();
        QRCodeData qrCodeDataObject = qrGenerator.CreateQrCode(qrCodeData, QRCodeGenerator.ECCLevel.Q);
        PngByteQRCode qrCode = new PngByteQRCode(qrCodeDataObject);
        byte[] qrCodeAsBitmapByteArr = qrCode.GetGraphic(20);

        // Save the QR code as a PNG file
        string tempPngPath = Path.Combine(Path.GetTempPath(), "qrcode.png");
        File.WriteAllBytes(tempPngPath, qrCodeAsBitmapByteArr);

        // Load the PDF document
        PdfDocument document = PdfReader.Open(pdfOutputPath, PdfDocumentOpenMode.Modify);
        PdfPage page = document.Pages[0];
        XGraphics gfx = XGraphics.FromPdfPage(page);

        // Load the QR code image from the PNG file
        using (XImage xImage = XImage.FromFile(tempPngPath))
        {
            gfx.DrawImage(xImage, 50, 700, 100, 100); // Adjust position and size as needed
        }

        // Save the modified PDF
        document.Save(pdfOutputPath);

        // Clean up temporary files
        File.Delete(tempPngPath);
    }
}