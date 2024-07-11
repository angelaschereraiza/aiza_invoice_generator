using NUnit.Framework;
using InvoiceGenerator.Models;
using InvoiceGenerator.Utilities;
using System.IO;
using System;
using System.IO.Compression;
using System.Text;

namespace InvoiceGenerator.Tests
{
    [TestFixture]
    public class InvoiceGeneratorTests
    {
        private string OutputDirectory => Path.Combine(TestContext.CurrentContext.TestDirectory, "TestOutput");

        [Test]
        public void RunWithZeroHoursShouldNotGenerateInvoice()
        {
            // Arrange
            // Simulate user input of 0 hours
            var inputReader = new StringReader("0");
            Console.SetIn(inputReader);

            // Act & Assert
            Assert.Throws<ArgumentException>(() => new Invoice());
        }

        [Test]
        public void ModifyContentXmlValidInputModifiesXmlSuccessfully()
        {
            // Arrange
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            string outputPath = Path.Combine(OutputDirectory, "XMLTestInvoice.odt");

            // Path inside the ODT file
            string contentXmlPath = "content.xml"; 
            
            // Simulate user input of 1 hours
           StringReader inputReader = new("1");
            Console.SetIn(inputReader);
            Invoice invoice = new Invoice();

            // Create an empty ODT file and add content.xml
            using (ZipArchive archive = ZipFile.Open(outputPath, ZipArchiveMode.Create))
            {
                archive.CreateEntry(contentXmlPath);
            }

            // Act: Modify content.xml in the ODT file
            bool result = Utilities.InvoiceGenerator.ModifyContentXml(outputPath, invoice);

            // Assert: Check if modifying content.xml succeeded
            Assert.IsTrue(result, "Modifying content.xml should succeed.");

            // Assert: Check if content.xml exists in the ODT file after modification
            using (ZipArchive archive = ZipFile.Open(outputPath, ZipArchiveMode.Update))
            {
                ZipArchiveEntry entry = archive.GetEntry(contentXmlPath);
                Assert.IsNotNull(entry, "content.xml should exist in the ODT file.");
            }

            Directory.Delete(OutputDirectory, true);
        }

        [Test]
        public void ConvertOdtToPDFValidInputConvertsToPDF()
        {
            // Arrange
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }

            string outputPath = Path.Combine(OutputDirectory, "TestInvoice");
            string odtFilePath = $"{outputPath}.odt";
            string pdfFilePath = $"{outputPath}.pdf";

            // Create an empty ODT file with proper file sharing options
            using (FileStream stream = new FileStream(odtFilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                // Act
                bool result = Utilities.InvoiceGenerator.ConvertOdtToPDF(odtFilePath, pdfFilePath);

                // Assert
                Assert.IsTrue(result, "Conversion from ODT to PDF should succeed.");
                Assert.IsTrue(File.Exists(pdfFilePath), "PDF file should exist after conversion.");
            }

            Directory.Delete(OutputDirectory, true);
        }

        private string GetExpectedOutputFilePath(Invoice invoice)
        {
            // Creates the expected file path for the output file
            string filename = $"Invoice_{invoice.Recipient.Replace(" ", "_")}_{invoice.Date.Replace(".", "_")}.odt";
            return Path.Combine(OutputDirectory, filename);
        }
    }
}
