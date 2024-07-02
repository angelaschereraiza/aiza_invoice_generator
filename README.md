# Aiza Invoice Generator

The Aiza Invoice Generator automates the creation of PDF invoices for Aiza GmbH, streamlining your billing process with ease and efficiency.

## Requirements
- .NET 8.0 SDK or later

## Step 1: Clone the Repository
```bash
git clone https://github.com/angelaschereraiza/aiza_invoice_generator.git
cd aiza-invoice-generator
```

## Step 2: Restore Dependencies
```bash
dotnet restore
```

## Step 3: Build and Run
```bash
dotnet build
dotnet run
```

## Dependencies
The project relies on the following libraries:

* DocumentFormat.OpenXml
* Open-XML-SDK
* QRCoder
* SkiaSharp
* PdfSharpCore
* SkiaSharp.QrCode
* System.Drawing.Common
* SixLabors.ImageSharp
* SixLabors.ImageSharp.Drawing

All dependencies are specified in the InvoiceGenerator.csproj file and will be restored when you run dotnet restore.