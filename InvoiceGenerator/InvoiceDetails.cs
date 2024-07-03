using System.Globalization;

public class InvoiceDetails
{
    // Public properties representing various details of an invoice
    public string Date { get; }               // The date of the invoice
    public string FirstDateMonth { get; }     // The first date of the billing month
    public float HourlyWage { get; }          // The hourly wage
    public float Hours { get; }               // The number of hours worked
    public string LastDateMonth { get; }      // The last date of the billing month
    public float MWSTRate { get; }            // The MWST rate
    public float MWSTPrice { get; }           // The MWST amount
    public string MonthYear { get; }          // The billing month and year
    public string Place { get; }              // The place of the recipient
    public string Recipient { get; }          // The recipient of the invoice
    public string Street { get; }             // The street address of the recipient
    public float TotalPrice { get; }          // The total price before MWST
    public float TotalPriceInclMWST { get; }  // The total price including MWST
    public string ZIP { get; }                // The ZIP code of the recipient

    public InvoiceDetails()
    {
        // Set the current date as the invoice date
        Date = DateTime.Now.ToString("dd.MM.yyyy");

        // Set a fixed hourly wage and MWST rate
        HourlyWage = 150;
        MWSTRate = 8.1f;

        // Determine the invoice period based on the current date
        DateTime currentDate = DateTime.Now;
        if (currentDate.Day <= 15)
        {
            // If the current day is on or before the 15th, use the previous month for billing
            FirstDateMonth = new DateTime(currentDate.Year, currentDate.Month - 1, 1).ToString("dd.MM.yyyy");
            LastDateMonth = new DateTime(currentDate.Year, currentDate.Month - 1, DateTime.DaysInMonth(currentDate.Year, currentDate.Month - 1)).ToString("dd.MM.yyyy");
            MonthYear = new DateTime(currentDate.Year, currentDate.Month - 1, 1).ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("de-DE"));
        }
        else
        {
            // If the current day is after the 15th, use the current month for billing
            FirstDateMonth = new DateTime(currentDate.Year, currentDate.Month, 1).ToString("dd.MM.yyyy");
            LastDateMonth = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)).ToString("dd.MM.yyyy");
            MonthYear = new DateTime(currentDate.Year, currentDate.Month, 1).ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("de-DE"));
        }

        // Prompt the user to input the hours worked
        Console.Write("Hours worked: ");
        string? hoursInput = Console.ReadLine();

        // Parse the input hours, default to 0 if parsing fails
        Hours = float.TryParse(hoursInput, out float h) ? h : 0f;

        // Set fixed values for place, recipient, street, and ZIP code
        Place = "Test";
        Recipient = "Test Customer AG";
        Street = "Test Street";

        // Calculate total prices based on hours worked and hourly wage
        TotalPrice = RoundToNearest5Rappen(HourlyWage * Hours);
        MWSTPrice = TotalPrice * (MWSTRate / 100);
        TotalPriceInclMWST = RoundToNearest5Rappen(TotalPrice + MWSTPrice);
        ZIP = "0000";
    }
        
    /// <summary>
    /// Rounds a given value to the nearest 0.05 (5 Rappen).
    /// This method multiplies the input value by 20 to shift the decimal place,
    /// rounds the result to the nearest integer using the MidpointRounding.AwayFromZero strategy,
    /// and then divides the result by 20 to shift the decimal place back, achieving rounding to the nearest 0.05.
    /// </summary>
    /// <param name="value">The value to be rounded.</param>
    /// <returns>The value rounded to the nearest 0.05.</returns>
    private static float RoundToNearest5Rappen(float value)
    {
        return (float)(Math.Round(value * 20, MidpointRounding.AwayFromZero) / 20.0);
    }

    /// <summary>
    /// Formats a float value as a currency string with two decimal places and a thousands separator.
    /// This method uses the "de-CH" (Swiss German) culture to format the value according to Swiss currency conventions.
    /// </summary>
    /// <param name="value">The float value to be formatted as currency.</param>
    /// <returns>A string representing the formatted currency value.</returns>
    public string FormatCurrency(float value)
    {
        return string.Format(CultureInfo.CreateSpecificCulture("de-CH"), "{0:N2}", value);
    }
}