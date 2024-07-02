using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text;

public static class InvoiceGenerator
{
    public static void Run()
    {
        // Define invoice details
        string recipient = "Customer AG";
        string street = "Test Street";
        string place = "0000 Test";
        string date = DateTime.Now.ToString("dd.MM.yyyy");
        
        Console.Write("Amount: ");
        string? amountInput = Console.ReadLine();
        string amount = amountInput ?? "0"; // Default to "0" if amountInput is null

        // Check if the amount is valid
        if (string.IsNullOrWhiteSpace(amount))
        {
            Console.WriteLine("Amount cannot be null or empty.");
            return;
        }

        // Define paths for the template and the output file
        string templatePath = Path.GetFullPath("invoice_template.odt");
        string outputPath = Path.GetFullPath($"Rechnung_{recipient.Replace(" ", "_")}_{date.Replace(".", "_")}.odt");

        // Ensure the template file exists
        if (!File.Exists(templatePath))
        {
            Console.WriteLine($"Template file '{templatePath}' does not exist.");
            return;
        }

        // Copy the template to the output path
        File.Copy(templatePath, outputPath, true);

        // Modify the content.xml inside the ODT file
        if (!ModifyContentXml(outputPath, recipient, street, place, date, amount))
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
        if (!ConvertOdtToPdf(outputPath, pdfOutputPath))
        {
            Console.WriteLine("Failed to create PDF.");
            return;
        }

        // Delete the ODT file after successful PDF creation
        File.Delete(outputPath);

        Console.WriteLine("The invoice was created successfully.");
    }

    // Function to modify the content.xml file inside the ODT archive
    private static bool ModifyContentXml(string outputPath, string recipient, string street, string place, string date, string amount)
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
                content = content.Replace("[InvoiceRecipient]", recipient)
                                 .Replace("[Street]", street)
                                 .Replace("[Place]", place)
                                 .Replace("[Date]", date)
                                 .Replace("[Amount]", amount);

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
    private static bool ConvertOdtToPdf(string outputPath, string pdfOutputPath)
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
