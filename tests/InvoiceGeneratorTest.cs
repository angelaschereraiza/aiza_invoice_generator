using System.IO.Compression;

namespace InvoiceGenerator.Tests
{
    [TestFixture]
    public class InvoiceGeneratorTests
    {
        private string OutputDirectory => Path.Combine(TestContext.CurrentContext.TestDirectory, "TestOutput");
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

        [Test]
        public void RunWithZeroHoursShouldNotGenerateInvoice()
        {
            // Arrange
            // Simulate recipient selection and zero hours / zero minutes
            var inputReader = new StringReader("1\n0\n0\n");
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
            
            // Simulate recipient selection and valid hours/minutes
            StringReader inputReader = new("1\n1\n0\n");
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
        public void MultiServiceInvoiceWithIncludedVatUsesNetPricesAndVatAmount()
        {
            // Arrange
            var inputReader = new StringReader("3\n");
            Console.SetIn(inputReader);

            // Act
            Invoice invoice = new Invoice();

            // Assert
            Assert.That(invoice.SelectedRecipient.Multiple, Is.True);
            Assert.That(invoice.SelectedRecipient.InclMWST, Is.True);
            Assert.That(invoice.TotalPrice, Is.EqualTo(5.55m).Within(0.001m));
            Assert.That(invoice.MWSTPrice, Is.EqualTo(0.45m).Within(0.001m));
            Assert.That(invoice.TotalPriceInclMWST, Is.EqualTo(6.00m).Within(0.001m));
            Assert.That(invoice.SelectedRecipient.Price, Is.EqualTo(0.93m).Within(0.001m));
            Assert.That(invoice.SelectedRecipient.Price1, Is.EqualTo(1.85m).Within(0.001m));
            Assert.That(invoice.SelectedRecipient.Price2, Is.EqualTo(2.78m).Within(0.001m));
        }

        [Test]
        public void ResolveTemplatePathUsesMultipleServiceTemplateForMultiServiceRecipient()
        {
            // Arrange
            var recipient = new Recipient { Name = "Test Customer AG 3", Multiple = true };

            // Act
            string templatePath = InvoiceGenerator.ResolveTemplatePath(recipient);

            // Assert
            Assert.That(templatePath, Does.EndWith("InvoiceMultipleServiceTemplate.odt"));
        }

        [Test]
        public void ResolveTemplatePathUsesStandardTemplateForRegularRecipient()
        {
            // Arrange
            var recipient = new Recipient { Name = "Test Customer AG" };

            // Act
            string templatePath = InvoiceGenerator.ResolveTemplatePath(recipient);

            // Assert
            Assert.That(templatePath, Does.EndWith("InvoiceTemplate.odt"));
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

        private static string FindRepositoryRoot()
        {
            DirectoryInfo? current = new(TestContext.CurrentContext.TestDirectory);
            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, "src", "Config", "example-config.json")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            throw new DirectoryNotFoundException("Could not find repository root containing src/Config/example-config.json.");
        }

        private string GetExpectedOutputFilePath(Invoice invoice)
        {
            // Creates the expected file path for the output file
            string filename = $"Invoice_{invoice.Recipient.Replace(" ", "_")}_{invoice.Date.Replace(".", "_")}.odt";
            return Path.Combine(OutputDirectory, filename);
        }
    }
}
