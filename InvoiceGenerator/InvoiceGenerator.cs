using System.Diagnostics;
using System.IO.Compression;
using System.Text;

public static class InvoiceGenerator
{
    public static void Run()
    {
        var invoiceDetails = new InvoiceDetails();

        if (string.IsNullOrWhiteSpace(invoiceDetails.Hours))
        {
            Console.WriteLine("Hours cannot be null or empty.");
            return;
        }

        // Define paths for the template and the output file
        string templatePath = Path.GetFullPath("invoice_template.odt");
        string outputPath = GetOutputPath(invoiceDetails);

        // Ensure the template file exists
        if (!File.Exists(templatePath))
        {
            Console.WriteLine($"Template file '{templatePath}' does not exist.");
            return;
        }

        // Copy the template to the output path
        File.Copy(templatePath, outputPath, true);

        // Modify the content.xml inside the ODT file
        if (!ModifyContentXml(outputPath, invoiceDetails))
        {
            Console.WriteLine("Failed to modify ODT file.");
            return;
        }

        // Verify that the ODT file was created successfully
        if (!File.Exists(outputPath))
        {
            Console.WriteLine($"Failed to create ODT file: {outputPath}");
            return;
        }

        // Define the output path for the PDF file
        string pdfOutputPath = outputPath.Replace(".odt", ".pdf");

        // Convert the ODT file to PDF
        if (!ConvertOdtToPDF(outputPath, pdfOutputPath))
        {
            Console.WriteLine("Failed to create PDF.");
            return;
        }

        // Delete the ODT file after successful PDF creation
        File.Delete(outputPath);

        Console.WriteLine("The invoice was created successfully.");
    }

    // Function to get the output path for the ODT file
    private static string GetOutputPath(InvoiceDetails details)
    {
        return Path.GetFullPath($"Rechnung_{details.Recipient.Replace(" ", "_")}_{details.Date.Replace(".", "_")}.odt");
    }

    // Function to modify the content.xml file inside the ODT archive
    private static bool ModifyContentXml(string outputPath, InvoiceDetails details)
    {
        try
        {
            using (ZipArchive archive = ZipFile.Open(outputPath, ZipArchiveMode.Update))
            {
                ZipArchiveEntry? entry = archive.GetEntry("content.xml");
                if (entry == null)
                {
                    Console.WriteLine("content.xml not found in the ODT archive.");
                    return false;
                }

                // Read the content of content.xml
                string content;
                using (StreamReader sr = new StreamReader(entry.Open(), Encoding.UTF8))
                {
                    content = sr.ReadToEnd();
                }

                // Replace placeholders with actual values
                content = content.Replace("[InvoiceRecipient]", details.Recipient)
                                 .Replace("[Street]", details.Street)
                                 .Replace("[Place]", details.Place)
                                 .Replace("[Hours]", details.Hours)
                                 .Replace("[HourlyWage]", details.HourlyWage.ToString())
                                 .Replace("[MWST]", details.Mwst.ToString())
                                 .Replace("[Date]", details.Date);

                // Delete the old entry and create a new one with the updated content
                entry.Delete();
                ZipArchiveEntry newEntry = archive.CreateEntry("content.xml");
                using (Stream entryStream = newEntry.Open())
                using (StreamWriter sw = new StreamWriter(entryStream, Encoding.UTF8))
                {
                    sw.Write(content);
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to modify ODT file: " + ex.Message);
            return false;
        }
    }

    // Function to convert the ODT file to PDF using LibreOffice
    private static bool ConvertOdtToPDF(string outputPath, string pdfOutputPath)
    {
        // Set up the process start information
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "soffice",
            Arguments = $"--headless --convert-to pdf \"{outputPath}\" --outdir \"{Path.GetDirectoryName(pdfOutputPath)}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        try
        {
            // Start the LibreOffice process
            using (Process process = Process.Start(startInfo))
            {
                if (process == null)
                {
                    throw new InvalidOperationException("Failed to start LibreOffice process.");
                }

                process.WaitForExit();

                // Check if the PDF file was created successfully
                return File.Exists(pdfOutputPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return false;
        }
    }
}