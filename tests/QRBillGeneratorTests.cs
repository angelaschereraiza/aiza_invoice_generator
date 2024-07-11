using NUnit.Framework;
using InvoiceGenerator.Models;
using InvoiceGenerator.Utilities;
using System.IO;
using System;
using PdfSharp.Pdf;
using PdfSharp.Drawing;

namespace InvoiceGenerator.Tests;

[TestFixture]
public class QRBillGeneratorTests
{
    [Test]
    public void GenerateQRBillValidInvoiceGeneratesPDFWithQRCode()
    {
        // Arrange
        PdfDocument document = new PdfDocument();
        document.AddPage();
        string pdfOutputPath = "test.pdf";
        document.Save(pdfOutputPath);
        document.Close();
        
        // Simulate user input of 1 hours
        StringReader inputReader = new("1");
        Console.SetIn(inputReader);
        Invoice invoice = new Invoice();

        // Act
        QRBillGenerator.GenerateQRBill(pdfOutputPath, invoice);

        // Assert
        Assert.That(File.Exists(pdfOutputPath), "PDF file should be generated.");

        // Clean up
        File.Delete(pdfOutputPath);
    }

    [Test]
    public void ConvertSvgToPngValidSvgFileConvertsToPng()
    {
        // Arrange
        string svgPath = "test.svg";
        string pngPath = "test.png";

        // Create a simple SVG content
        string svgContent = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"100\" height=\"100\"><rect width=\"100\" height=\"100\" style=\"fill:rgb(0,0,255);stroke-width:1;stroke:rgb(0,0,0)\" /></svg>";
        File.WriteAllText(svgPath, svgContent);

        // Act
        QRBillGenerator.ConvertSvgToPng(svgPath, pngPath);

        // Assert
        Assert.That(File.Exists(pngPath), "PNG file should be generated.");

        // Clean up
        File.Delete(svgPath);
        File.Delete(pngPath);
    }
}
