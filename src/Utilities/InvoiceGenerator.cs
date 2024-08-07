﻿using System.Diagnostics;
using System.IO.Compression;
using System.Text;
using InvoiceGenerator.Models;

namespace InvoiceGenerator.Utilities;

public static class InvoiceGenerator
{
    public static void GenerateInvoice()
    {
        // Create an instance of Invoice
        Invoice invoice = new Invoice();

        if (invoice.Hours == 0)
        {
            Console.WriteLine("Hours cannot be null or empty.");
            return;
        }

        // Define paths for the template and the output file
        string templatePath = Path.GetFullPath("Templates/InvoiceTemplate.odt");
        string outputPath = Path.GetFullPath($"Outputs/Rechnung_{invoice.Recipient.Replace(" ", "_")}_{invoice.Date.Replace(".", "_")}.odt");

        // Ensure the template file exists
        if (!File.Exists(templatePath))
        {
            Console.WriteLine($"Template file '{templatePath}' does not exist.");
            return;
        }

        // Copy the template to the output path
        File.Copy(templatePath, outputPath, true);

        // Modify the content.xml inside the ODT file
        if (!ModifyContentXml(outputPath, invoice))
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

        // Generate the QR Code and add it to the PDF
        QRBillGenerator.GenerateQRBill(pdfOutputPath, invoice);

        Console.WriteLine("The invoice was created successfully.");
    }

    /// <summary>
    /// Modifies the content.xml file within an ODT archive by replacing placeholders with actual values.
    /// This method reads the content.xml file from the provided ODT file, replaces predefined placeholders
    /// with the actual invoice details, and then updates the content.xml file within the ODT archive.
    /// </summary>
    /// <param name="outputPath">The file path of the ODT document to modify.</param>
    /// <param name="invoice">The Invoice object containing invoice information.</param>
    /// <returns>Returns true if the content.xml file was modified successfully; otherwise, false.</returns>
    public static bool ModifyContentXml(string outputPath, Invoice invoice)
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
                using (StreamReader sr = new(entry.Open(), Encoding.UTF8))
                {
                    content = sr.ReadToEnd();
                }

                // Replace placeholders with actual values
                content = content.Replace("[Date]", invoice.Date)
                                 .Replace("[FirstDateMonth]", invoice.FirstDateMonth)
                                 .Replace("[HourlyWage]", invoice.FormatCurrency(invoice.HourlyWage))
                                 .Replace("[Hours]", invoice.Hours.ToString())
                                 .Replace("[LastDateMonth]", invoice.LastDateMonth)
                                 .Replace("[MonthYear]", invoice.MonthYear)
                                 .Replace("[MWSTRate]", invoice.MWSTRate.ToString())
                                 .Replace("[MWSTPrice]", invoice.FormatCurrency(invoice.MWSTPrice))
                                 .Replace("[Place]", $"{invoice.ZIP} {invoice.Place}")
                                 .Replace("[Recipient]", invoice.Recipient)
                                 .Replace("[Street]", invoice.Street)
                                 .Replace("[TotalPrice]", invoice.FormatCurrency(invoice.TotalPrice))
                                 .Replace("[TotalPriceInclMWST]", invoice.FormatCurrency(invoice.TotalPriceInclMWST));

                // Delete the old entry and create a new one with the updated content
                entry.Delete();
                ZipArchiveEntry newEntry = archive.CreateEntry("content.xml");
                using (Stream entryStream = newEntry.Open())
                using (StreamWriter sw = new(entryStream, Encoding.UTF8))
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

    /// <summary>
    /// Converts an ODT file to a PDF file using LibreOffice in headless mode.
    /// This method starts a LibreOffice process to perform the conversion, 
    /// waits for the process to complete, and then checks if the PDF file was created successfully.
    /// </summary>
    /// <param name="outputPath">The file path of the ODT document to convert.</param>
    /// <param name="pdfOutputPath">The file path where the converted PDF document will be saved.</param>
    /// <returns>Returns true if the PDF file was created successfully; otherwise, false.</returns>
    public static bool ConvertOdtToPDF(string outputPath, string pdfOutputPath)
    {
        // Set up the process start information
        ProcessStartInfo startInfo = new()
        {
            FileName = @"C:\Program Files\LibreOffice\program\soffice.exe",
            Arguments = $"--headless --convert-to pdf \"{outputPath}\" --outdir \"{Path.GetDirectoryName(pdfOutputPath)}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            UseShellExecute = false,
        };

        try
        {
            // Start the LibreOffice process
            using (Process? process = Process.Start(startInfo))
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