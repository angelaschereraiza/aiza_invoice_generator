using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;

public static class InvoiceGenerator
{
    public static void Run()
    {
        string recipient = "Customer AG";
        string street = "Test Street";
        string place = "0000 Test";
        string date = DateTime.Now.ToString("dd.MM.yyyy");
        string? amount = "100";

        // Console.Write("Amount: ");
        // string? amount = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(amount))
        {
            Console.WriteLine("Amount cannot be null or empty.");
            return;
        }

        // Paths to the template and output document
        string templatePath = Path.GetFullPath("invoice_template.odt"); // Use absolute path
        string outputPath = Path.GetFullPath($"Rechnung_{recipient.Replace(" ", "_")}_{date.Replace(".", "_")}.odt"); // Avoid spaces in filenames

        // Ensure the template file exists
        if (!File.Exists(templatePath))
        {
            Console.WriteLine($"Template file '{templatePath}' does not exist.");
            return;
        }

        // Create a copy of the template to work on
        File.Copy(templatePath, outputPath, true);

        // Manipulate the content.xml file inside the ODT (which is a ZIP archive)
        try
        {
            using (ZipArchive archive = ZipFile.Open(outputPath, ZipArchiveMode.Update))
            {
                ZipArchiveEntry entry = archive.GetEntry("content.xml");
                if (entry != null)
                {
                    string content;
                    using (StreamReader sr = new StreamReader(entry.Open()))
                    {
                        content = sr.ReadToEnd();
                    }

                    content = content.Replace("[InvoiceRecipient]", recipient);
                    content = content.Replace("[Street]", street);
                    content = content.Replace("[Place]", place);
                    content = content.Replace("[Date]", date);
                    content = content.Replace("[Amount]", amount);

                    using (StreamWriter sw = new StreamWriter(entry.Open()))
                    {
                        sw.Write(content);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Failed to modify ODT file: " + ex.Message);
            return;
        }

        // Verify if the output ODT file is created
        if (!File.Exists(outputPath))
        {
            Console.WriteLine($"Failed to create ODT file: {outputPath}");
            return;
        }

        Console.WriteLine($"ODT file created successfully: {outputPath}");

        // Start LibreOffice in headless mode to convert the ODT to PDF
        string pdfOutputPath = outputPath.Replace(".odt", ".pdf");
        string arguments = $"--headless --convert-to pdf \"{outputPath}\" --outdir \"{Path.GetDirectoryName(pdfOutputPath)}\"";

        Console.WriteLine("Executing command: soffice " + arguments);

        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "soffice",
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false
        };

        Process process = new Process { StartInfo = startInfo };

        try
        {
            process.Start();

            // Read the output and error streams
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            Console.WriteLine("LibreOffice output: " + output);
            Console.WriteLine("LibreOffice error: " + error);

            if (File.Exists(pdfOutputPath))
            {
                Console.WriteLine($"Invoice has been created and saved as PDF: {pdfOutputPath}");
            }
            else
            {
                Console.WriteLine("Failed to create PDF.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
    }
}
