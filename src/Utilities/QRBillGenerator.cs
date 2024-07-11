using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Codecrete.SwissQRBill.Generator;
using SkiaSharp;
using Svg.Skia;
using InvoiceGenerator.Models;

namespace InvoiceGenerator.Utilities;

public static class QRBillGenerator
{
    /// <summary>
    /// Generates a QR code containing invoice information and embeds it into a PDF file.
    /// The method reads invoice details, generates a QR code, converts it into an image,
    /// and embeds this image at a specified position in the provided PDF document.
    /// </summary>
    /// <param name="pdfOutputPath">The file path of the PDF document to modify.</param>
    /// <param name="invoice">The Invoice object containing invoice information.</param>
    public static void GenerateQRBill(string pdfOutputPath, Invoice invoice)
    {
        // Create the QR bill data
        Bill bill = new()
        {
            Account = "CH23 8080 8007 6888 9345 2",
            Creditor = new Address
            {
                Name = "Aiza GmbH",
                Street = "Bernstrasse 159",
                PostalCode = "3052",
                Town = "Zollikofen",
                CountryCode = "CH"
            },
            Amount = (decimal?)invoice.TotalPriceInclMWST,
            Currency = "CHF",
            Debtor = new Address
            {
                Name = invoice.Recipient,
                Street = invoice.Street,
                PostalCode = invoice.ZIP,
                Town = invoice.Place,
                CountryCode = "CH"
            },
            Format = new BillFormat
            {
                Language = Language.DE,
            }
        };

        // Generate QR bill
        byte[] svgBytes = QRBill.Generate(bill);

        const string svgPath = "qrbill.svg";
        const string pngPath = "qrbill.png";

        // Save generated SVG file
        File.WriteAllBytes(svgPath, svgBytes);

        // Convert the SVG to a high-quality PNG file
        ConvertSvgToPng(svgPath, pngPath);

        // Embed the QR code PNG into the PDF
        EmbedQRInPDF(pdfOutputPath, pngPath);

        // Delete temporary files
        File.Delete(pngPath);
        File.Delete(svgPath);
    }

    /// <summary>
    /// Converts an SVG file to a high-resolution PNG file.
    /// This method loads an SVG file, renders it onto a high-resolution canvas to ensure quality,
    /// and then saves the rendered image as a PNG file.
    /// </summary>
    /// <param name="svgPath">The file path of the input SVG file.</param>
    /// <param name="pngPath">The file path where the output PNG file will be saved.</param>
    public static void ConvertSvgToPng(string svgPath, string pngPath)
    {
        // Load the SVG file
        SKSvg svg = new();
        using (FileStream stream = new(svgPath, FileMode.Open, FileAccess.Read))
        {
            svg.Load(stream);
        }

        if (svg.Picture == null)
        {
            throw new InvalidOperationException("Failed to load the SVG picture.");
        }

        // Render the SVG to a high-resolution PNG file
        int width = (int)svg.Picture.CullRect.Width * 4;  // Increase resolution by 4 times
        int height = (int)svg.Picture.CullRect.Height * 4;
        using (SKSurface surface = SKSurface.Create(new SKImageInfo(width, height)))
        {
            SKCanvas canvas = surface.Canvas;
            canvas.Clear(SKColors.Transparent);
            canvas.Scale(4);  // Scale canvas to improve resolution
            canvas.DrawPicture(svg.Picture);

            SKImage qrImage = surface.Snapshot();

            using (SKData data = qrImage.Encode(SKEncodedImageFormat.Png, 100))
            {
                File.WriteAllBytes(pngPath, data.ToArray());
            }
        }
    }

    /// <summary>
    /// Embeds a QR code image into a PDF file.
    /// This method opens an existing PDF file, loads a QR code image,
    /// and embeds the QR code image into the last page of the PDF at a specified position and size.
    /// </summary>
    /// <param name="pdfOutputPath">The file path of the existing PDF document to modify.</param>
    /// <param name="qrImagePath">The file path of the QR code image to embed.</param>
    public static void EmbedQRInPDF(string pdfOutputPath, string qrImagePath)
    {
        using (PdfDocument document = PdfSharp.Pdf.IO.PdfReader.Open(pdfOutputPath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Modify))
        {
            PdfPage page = document.Pages[0];
            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Load the QR code image
            XImage qrImage = XImage.FromFile(qrImagePath);

            // Define the position and size for the QR code image 
            double y = page.Height.Point - (qrImage.PixelHeight * (page.Width.Point / qrImage.PixelWidth));
            double width = page.Width.Point;
            double height = qrImage.PixelHeight * (page.Width.Point / qrImage.PixelWidth);

            gfx.DrawImage(qrImage, 0, y, width, height);

            // Save the updated PDF
            document.Save(pdfOutputPath);
        }
    }
}
