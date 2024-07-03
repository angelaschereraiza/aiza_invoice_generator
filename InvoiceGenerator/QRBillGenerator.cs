using PdfSharp.Pdf;
using PdfSharp.Drawing;
using Codecrete.SwissQRBill.Generator;
using SkiaSharp;
using System;
using System.IO;
using Svg.Skia;
using Aspose.Html;
using Aspose.Html.Dom.Svg;
using Aspose.Html.Converters;
using Aspose.Html.Saving;
using Aspose.Html.Rendering.Image;

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

        const string svgPath = "qrbill.svg";
        const string pngPath = "qrbill.png";

        // Save generated SVG file
        File.WriteAllBytes(svgPath, svgBytes);

        // Convert SVG to PNG and saves PNG
        Converter.ConvertSVG(new SVGDocument(svgPath), new ImageSaveOptions(ImageFormat.Png), pngPath);

        // Load the SVG into an SKPicture
        // SKSvg svg = new SKSvg();
        // using (var stream = new MemoryStream(svgBytes))
        // {
        //     svg.Load(stream);
        // }

        // if (svg.Picture == null)
        // {
        //     throw new InvalidOperationException("Failed to load the SVG picture.");
        // }

        // // Convert SKPicture to SKImage with proper font handling
        // SKImageInfo info = new SKImageInfo((int)svg.Picture.CullRect.Width, (int)svg.Picture.CullRect.Height);
        // using (SKSurface surface = SKSurface.Create(info))
        // {
        //     SKCanvas canvas = surface.Canvas;
        //     canvas.Clear(SKColors.Transparent);
        //     canvas.DrawPicture(svg.Picture);

        //     // Load and set the desired fonts
        //     using (var fontStream = File.OpenRead("Helvetica.ttf"))
        //     {
        //         var typeface = SKTypeface.FromStream(fontStream);
        //         canvas.DrawPicture(svg.Picture, new SKPaint { Typeface = typeface });
        //     }

        //     SKImage qrImage = surface.Snapshot();

        //     // Save SKImage to a temporary PNG file
        //     using (SKData pngData = qrImage.Encode(SKEncodedImageFormat.Png, 200))
        //     {
        //         using (FileStream fs = File.OpenWrite(pngPath))
        //         {
        //             pngData.SaveTo(fs);
        //         }
        //     }
        //}

        // Embed the QR code image into the PDF
        EmbedQRInPDF(pdfOutputPath, pngPath);

        // Delete temporary files
        File.Delete(pngPath);
        File.Delete(svgPath);
    }

    private void EmbedQRInPDF(string pdfOutputPath, string qrImagePath)
    {
        using (PdfDocument document = PdfSharp.Pdf.IO.PdfReader.Open(pdfOutputPath, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Modify))
        {
            // Assume that the QR code should be added to the last page
            PdfPage page = document.Pages[document.Pages.Count - 1];

            XGraphics gfx = XGraphics.FromPdfPage(page);

            // Load the QR code image
            XImage qrImage = XImage.FromFile(qrImagePath);

            // Define the position and size for the QR code image
            double x = 0; // Adjust the X position as needed
            double y = page.Height.Point - (qrImage.PixelHeight * (page.Width.Point / qrImage.PixelWidth)); // Adjust the Y position as needed
            double width = page.Width.Point;
            double height = qrImage.PixelHeight * (page.Width.Point / qrImage.PixelWidth); // Maintain aspect ratio

            gfx.DrawImage(qrImage, x, y, width, height);

            // Save the updated PDF
            document.Save(pdfOutputPath);
        }
    }
}
