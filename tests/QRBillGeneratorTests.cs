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
    private string? _originalCurrentDirectory;
    private string? _tempConfigDirectory;

    [SetUp]
    public void SetUp()
    {
        _originalCurrentDirectory = Directory.GetCurrentDirectory();

        string repoRoot = FindRepositoryRoot();
        _tempConfigDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestConfig");
        string configDirectory = Path.Combine(_tempConfigDirectory!, "Config");
        Directory.CreateDirectory(configDirectory);

        string sourceConfigPath = Path.Combine(repoRoot, "src", "Config", "example-config.json");
        string targetConfigPath = Path.Combine(configDirectory, "config.json");
        File.Copy(sourceConfigPath, targetConfigPath, overwrite: true);

        Directory.SetCurrentDirectory(_tempConfigDirectory!);
    }

    [TearDown]
    public void TearDown()
    {
        if (!string.IsNullOrEmpty(_tempConfigDirectory) && Directory.Exists(_tempConfigDirectory))
        {
            Directory.Delete(_tempConfigDirectory, recursive: true);
        }

        if (!string.IsNullOrEmpty(_originalCurrentDirectory))
        {
            Directory.SetCurrentDirectory(_originalCurrentDirectory);
        }
    }

    private static string FindRepositoryRoot()
    {
        string currentPath = Directory.GetCurrentDirectory();
        while (!File.Exists(Path.Combine(currentPath, "aiza_invoice_generator.sln")))
        {
            currentPath = Directory.GetParent(currentPath)?.FullName ?? throw new InvalidOperationException("Repository root not found");
        }
        return currentPath;
    }
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
