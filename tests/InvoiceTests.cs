using NUnit.Framework;
using InvoiceGenerator.Models;
using System.IO;
using System;

namespace InvoiceGenerator.Tests;

[TestFixture]
public class InvoiceTests
{
    [Test]
    public void ConstructorInitializesPropertiesCorrectly()
    {
        // Act
        // Simulate user input of 1 hours
        StringReader inputReader = new("1");
        Console.SetIn(inputReader);
        Invoice invoice = new Invoice();


        // Assert
        Assert.That(invoice.Date, Is.Not.Null.Or.Empty);
        Assert.That(invoice.FirstDateMonth, Is.Not.Null.Or.Empty);
        Assert.That(invoice.HourlyWage, Is.EqualTo(1));
        Assert.That(invoice.Hours, Is.EqualTo(1));
        Assert.That(invoice.LastDateMonth, Is.Not.Null.Or.Empty);
        Assert.That(invoice.MWSTRate, Is.EqualTo(8.1));
        Assert.That(invoice.MWSTPrice, Is.EqualTo(0.081m));
        Assert.That(invoice.MonthYear, Is.Not.Null.Or.Empty);
        Assert.That(invoice.Place, Is.EqualTo("Test"));
        Assert.That(invoice.Recipient, Is.EqualTo("Test Customer AG"));
        Assert.That(invoice.Street, Is.EqualTo("Test Street"));
        Assert.That(invoice.TotalPrice, Is.EqualTo(1.0m));
        Assert.That(invoice.TotalPriceInclMWST, Is.EqualTo(1.1m));
        Assert.That(invoice.ZIP, Is.EqualTo("0000"));
    }

    [Test]
    public void RoundToNearest5RappenRoundsCorrectly()
    {
        // Arrange
        StringReader inputReader = new("1");
        Console.SetIn(inputReader);
        Invoice invoice = new Invoice();

        // Act
        decimal roundedValue = Invoice.RoundToNearest5Rappen(12.345m);

        // Assert
        Assert.That(roundedValue, Is.EqualTo(12.35m));
    }

    [Test]
    public void FormatCurrencyFormatsCorrectly()
    {
        // Arrange
        StringReader inputReader = new("1");
        Console.SetIn(inputReader);
        Invoice invoice = new Invoice();

        // Act
        string formattedValue = invoice.FormatCurrency(1234.56m);

        // Assert
        Assert.That(formattedValue, Is.EqualTo("1’234.56"));
    }
}
