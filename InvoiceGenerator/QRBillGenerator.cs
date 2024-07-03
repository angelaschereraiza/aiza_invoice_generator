using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Codecrete.SwissQRBill.Generator;
using SkiaSharp;
using Svg.Skia;

public class QRBillGenerator
{
    /// <summary>
    /// Generates a QR code containing invoice information and embeds it into a PDF file.
    /// The method reads invoice details, generates a QR code, converts it into an image,
    /// and embeds this image at a specified position in the provided PDF document.
    /// </summary>
    /// <param name="pdfOutputPath">The file path of the PDF document to modify.</param>
    /// <param name="invoiceDetails">The InvoiceDetails object containing invoice information.</param>
    public void GenerateQRBill(string pdfOutputPath, InvoiceDetails invoiceDetails)
    {
        // Create the QR bill data
        Bill bill = new Bill
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
            Amount = (decimal?)invoiceDetails.TotalPriceInclMWST,
            Currency = "CHF",
            Debtor = new Address
            {
                Name = invoiceDetails.Recipient,
                Street = invoiceDetails.Street,
                PostalCode = invoiceDetails.ZIP,
                Town = invoiceDetails.Place,
                CountryCode = "CH"
            },
            Format = new BillFormat
            {
                Language = Language.DE,
            }
        };

        // Generate QR bill
        byte[] svgBytes = QRBill.Generate(bill);

        // Save generated SVG file (optional, for debugging purposes)
        const string svgPath = "qrbill.svg";
        File.WriteAllBytes(svgPath, svgBytes);
        Console.WriteLine($"QR bill saved at {Path.GetFullPath(svgPath)}");

        // Convert SVG to PDF and embed in PDF
        const string pdfPath = "qrbill.pdf";
        using (PdfDocument document = new PdfDocument())
        {
            PdfPage page = document.AddPage();
            XGraphics gfx = XGraphics.FromPdfPage(page);
            using (var svgStream = new MemoryStream(svgBytes))
            {
                var skSvg = new SKSvg();
                skSvg.Load(svgStream);

                // Create a bitmap and draw the SVG onto it
                int width = (int)page.Width;
                int height = (int)page.Height;
                using (var bitmap = new SKBitmap(width, height))
                using (var canvas = new SKCanvas(bitmap))
                {
                    canvas.Clear(SKColors.White);
                    canvas.DrawPicture(skSvg.Picture);
                    canvas.Flush();

                    // Encode the bitmap to a PNG format
                    using (var imageStream = new MemoryStream())
                    {
                        bitmap.Encode(imageStream, SKEncodedImageFormat.Png, 100);
                        imageStream.Position = 0;

                        // Load the image from the memory stream
                        XImage image = XImage.FromStream(imageStream);
                        gfx.DrawImage(image, 0, 0, page.Width.Point, page.Height.Point);
                    }
                }
            }
            document.Save(pdfOutputPath);
            Console.WriteLine($"PDF saved at {Path.GetFullPath(pdfOutputPath)}");
        }
    }
}