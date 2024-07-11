using System.Globalization;

namespace InvoiceGenerator.Models;

public class Invoice
{
    // Public properties representing various details of an invoice
    public decimal HourlyWage { get; } = 1m;
    public decimal MWSTRate { get; } = 8.1m;
    public string Recipient { get; } = "Test Customer AG";
    public string Street { get; } = "Test Street";
    public string ZIP { get; } = "0000";
    public string Place { get; } = "Test";
    public string Date { get; }
    public string FirstDateMonth { get; }
    public decimal HourlyWageValue { get; }
    public decimal? Hours { get; }
    public string LastDateMonth { get; }
    public decimal MWSTRateValue { get; }
    public decimal MWSTPrice { get; }
    public string MonthYear { get; }
    public string PlaceValue { get; }
    public string RecipientValue { get; }
    public string StreetValue { get; }
    public decimal TotalPrice { get; }
    public decimal TotalPriceInclMWST { get; }
    public string ZIPValue { get; }

    public Invoice()
    {
        DateTimeOffset currentDate = DateTimeOffset.Now;
        PlaceValue = Place;
        RecipientValue = Recipient;
        StreetValue = Street;
        ZIPValue = ZIP;

        // Set the current date as the invoice date
        Date = currentDate.ToString("dd.MM.yyyy");

        // Determine the invoice period based on the current date
        DateTimeOffset billingDate = currentDate.Day <= 15 ?
            currentDate.AddMonths(-1) :
            currentDate;

        FirstDateMonth = new DateTimeOffset(billingDate.Year, billingDate.Month, 1, 0, 0, 0, currentDate.Offset).ToString("dd.MM.yyyy");
        LastDateMonth = new DateTimeOffset(billingDate.Year, billingDate.Month, DateTime.DaysInMonth(billingDate.Year, billingDate.Month), 0, 0, 0, currentDate.Offset).ToString("dd.MM.yyyy");
        MonthYear = billingDate.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("de-DE"));

        // Prompt the user to input the hours worked
        Console.Write("Hours worked: ");
        string? hoursInput = Console.ReadLine();

        // Parse the input hours, default to 0 if parsing fails
        Hours = decimal.TryParse(hoursInput, out decimal h) ? h : 0m;

        if (Hours == 0m)
        {
            throw new ArgumentException("Invoice file should not be generated for zero hours.");
        }

        HourlyWageValue = HourlyWage;
        MWSTRateValue = MWSTRate;

        // Calculate total prices based on hours worked and hourly wage
        TotalPrice = RoundToNearest5Rappen(HourlyWageValue * Hours.Value);
        MWSTPrice = TotalPrice * (MWSTRateValue / 100m);
        TotalPriceInclMWST = RoundToNearest5Rappen(TotalPrice + MWSTPrice);
    }
        
    /// <summary>
    /// Rounds a given value to the nearest 0.05 (5 Rappen).
    /// This method multiplies the input value by 20 to shift the decimal place,
    /// rounds the result to the nearest integer using the MidpointRounding.AwayFromZero strategy,
    /// and then divides the result by 20 to shift the decimal place back, achieving rounding to the nearest 0.05.
    /// </summary>
    /// <param name="value">The value to be rounded.</param>
    /// <returns>The value rounded to the nearest 0.05.</returns>
    public static decimal RoundToNearest5Rappen(decimal value)
    {
        return Math.Round(value * 20m, MidpointRounding.AwayFromZero) / 20m;
    }

    /// <summary>
    /// Formats a decimal value as a currency string with two decimal places and a thousands separator.
    /// This method uses the "de-CH" (Swiss German) culture to format the value according to Swiss currency conventions.
    /// </summary>
    /// <param name="value">The decimal value to be formatted as currency.</param>
    /// <returns>A string representing the formatted currency value.</returns>
    public string FormatCurrency(decimal value)
    {
        return string.Format(CultureInfo.CreateSpecificCulture("de-CH"), "{0:N2}", value);
    }
}