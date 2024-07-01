using System;
using System.Diagnostics;
using System.IO;

public static class InvoiceGenerator
{
    public static void Run()
    {
        string recipient = "Customer AG";
        string street = "Test Street";
        string place = "0000 Test";
        string date = DateTime.Now.ToString("dd.MM.yyyy");

        Console.Write("Amount: ");
        string? amount = Console.ReadLine();

        if (amount == null)
        {
            Console.WriteLine("Amount cannot be null.");
            return;
        }

        // Paths to the template and output document
        string templatePath = "invoice_template.odt";
        string outputPath = $"Rechnung {recipient} {date}.pdf";

        // Read the template content and replace placeholders with actual values
        string content = File.ReadAllText(templatePath);
        content = content.Replace("[InvoiceRecipient]", recipient);
        content = content.Replace("[Street]", street);
        content = content.Replace("[Place]", place);
        content = content.Replace("[Date]", date);
        content = content.Replace("[Amount]", amount);

        // Save the modified content to the output path
        File.WriteAllText(outputPath, content);

        // Start LibreOffice in headless mode to convert the ODG to PDF
        ProcessStartInfo startInfo = new ProcessStartInfo
        {
            FileName = "soffice",
            Arguments = $"--headless --convert-to pdf \"{outputPath}\" --outdir \"path/to/output/directory\"",
            CreateNoWindow = true,
            UseShellExecute = false
        };

        Process? process = Process.Start(startInfo);

        if (process == null)
        {
            Console.WriteLine("Failed to start LibreOffice.");
            return;
        }

        process.WaitForExit();

        Console.WriteLine("Invoice has been created and saved as PDF.");
    }
}