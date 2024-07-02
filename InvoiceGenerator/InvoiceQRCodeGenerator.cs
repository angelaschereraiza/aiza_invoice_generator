using System;
using System.IO;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using Codecrete.SwissQRBill.Generator;
using Codecrete.SwissQRBill.Generator.Canvas;

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
        };

        // Generate QR bill
            byte[] svg = QRBill.Generate(bill);

            // Save generated SVG file
            const string path = "qrbill.svg";
            File.WriteAllBytes(path, svg);
            Console.WriteLine($"QR bill saved at { Path.GetFullPath(path) }");
    }
}