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
        var invoice = new Invoice();

        // Assert
        Assert.That(invoice.Date, Is.Not.Null.Or.Empty);
        Assert.That(invoice.FirstDateMonth, Is.Not.Null.Or.Empty);
        Assert.That(invoice.HourlyWage, Is.EqualTo(1f));
        Assert.That(invoice.Hours, Is.EqualTo(0f));
        Assert.That(invoice.LastDateMonth, Is.Not.Null.Or.Empty);
        Assert.That(invoice.MWSTRate, Is.EqualTo(8.1f));
        Assert.That(invoice.MWSTPrice, Is.EqualTo(0f));
        Assert.That(invoice.MonthYear, Is.Not.Null.Or.Empty);
        Assert.That(invoice.Place, Is.EqualTo("Test"));
        Assert.That(invoice.Recipient, Is.EqualTo("Test Customer AG"));
        Assert.That(invoice.Street, Is.EqualTo("Test Street"));
        Assert.That(invoice.TotalPrice, Is.EqualTo(0f));
        Assert.That(invoice.TotalPriceInclMWST, Is.EqualTo(0f));
        Assert.That(invoice.ZIP, Is.EqualTo("0000"));
    }

    [Test]
    public void RoundToNearest5RappenRoundsCorrectly()
    {
        // Arrange
        var invoice = new Invoice();

        // Act
        float roundedValue = Invoice.RoundToNearest5Rappen(12.345f);

        // Assert
        Assert.That(roundedValue, Is.EqualTo(12.35f));
    }

    [Test]
    public void FormatCurrencyFormatsCorrectly()
    {
        // Arrange
        var invoice = new Invoice();

        // Act
        string formattedValue = invoice.FormatCurrency(1234.56f);

        // Assert
         Assert.That(formattedValue, Is.EqualTo("1'234.56"));
    }
}
