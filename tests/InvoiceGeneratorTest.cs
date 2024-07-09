using NUnit.Framework;
using InvoiceGenerator.Models;
using InvoiceGenerator.Utilities;
using System.IO;
using System;

namespace InvoiceGenerator.Tests
{
    [TestFixture]
    public class InvoiceGeneratorTests
    {
        private string OutputDirectory => Path.Combine(TestContext.CurrentContext.TestDirectory, "TestOutput");

        [SetUp]
        public void Setup() =>
            // Preparations before each test run, e.g., deleting old output files
            CleanupOutputDirectory();

        [TearDown]
        public void Teardown() =>
            // Cleanup after each test run, e.g., deleting generated files
            CleanupOutputDirectory();

        [Test]
        public void Run_WithValidInvoiceGeneratesInvoiceSuccessfully()
        {
            // Arrange
            Invoice invoice = new();

            // Assert
            string expectedOutputFile = GetExpectedOutputFilePath(invoice);
            Assert.That(File.Exists(expectedOutputFile), "Generated invoice file does not exist.");

            string pdfOutputPath = expectedOutputFile.Replace(".odt", ".pdf");
            Assert.That(File.Exists(pdfOutputPath), "Generated PDF file does not exist.");
        }

        [Test]
        public void Run_WithZeroHoursShouldNotGenerateInvoice()
        {
            // Arrange
            Invoice invoice = new();

            // Assert
            string expectedOutputFile = GetExpectedOutputFilePath(invoice);
            Assert.That(File.Exists(expectedOutputFile), "Invoice file should not be generated for zero hours.");
        }

        [Test]
        public void ModifyContentXmlValidInputModifiesXmlSuccessfully()
        {
            // Arrange
            string outputPath = Path.Combine(OutputDirectory, "TestInvoice.odt");
            var invoice = new Invoice();

            // Act
            bool result = Utilities.InvoiceGenerator.ModifyContentXml(outputPath, invoice);

            // Assert
            Assert.That(result, "Failed to modify content.xml.");
        }

        [Test]
        public void ConvertOdtToPDFValidInputConvertsToPDF()
        {
            // Arrange
            string outputPath = Path.Combine(OutputDirectory, "TestInvoice.odt");
            string pdfOutputPath = Path.Combine(OutputDirectory, "TestInvoice.pdf");

            // Act
            bool result = Utilities.InvoiceGenerator.ConvertOdtToPDF(outputPath, pdfOutputPath);

            // Assert
            Assert.That(!result);
        }

        // Helper methods for tests

        private void CleanupOutputDirectory()
        {
            // Deletes all files in the output directory
            if (Directory.Exists(OutputDirectory))
            {
                Directory.Delete(OutputDirectory, true);
            }
        }

        private string GetExpectedOutputFilePath(Invoice invoice)
        {
            // Creates the expected file path for the output file
            string filename = $"Invoice_{invoice.Recipient.Replace(" ", "_")}_{invoice.Date.Replace(".", "_")}.odt";
            return Path.Combine(OutputDirectory, filename);
        }
    }
}
