using NUnit.Framework;
using InvoiceGenerator.Models;
using InvoiceGenerator.Utilities;

namespace InvoiceGenerator.Tests;

[TestFixture]
public class QRBillGeneratorTests
{
    private QRBillGenerator _qrBillGenerator = new();

    [SetUp]
    public void Setup()
    {
        _qrBillGenerator = new QRBillGenerator();
    }

    [Test]
    public void GenerateQRBillValidInvoiceGeneratesPDFWithQRCode()
    {
        // Arrange
        string pdfOutputPath = "test.pdf";
        var invoice = new Invoice();

        // Act
        _qrBillGenerator.GenerateQRBill(pdfOutputPath, invoice);

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
        _qrBillGenerator.ConvertSvgToPng(svgPath, pngPath);

        // Assert
        Assert.That(File.Exists(pngPath), "PNG file should be generated.");

        // Clean up
        File.Delete(svgPath);
        File.Delete(pngPath);
    }
}
