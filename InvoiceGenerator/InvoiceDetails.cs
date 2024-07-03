using System.Globalization;

public class InvoiceDetails
{
    public string Date { get; }
    public string FirstDateMonth { get; }
    public float HourlyWage { get; }
    public float Hours { get; }
    public string LastDateMonth { get; }
    public float MWSTRate { get; }
    public float MWSTPrice { get; }
    public string MonthYear { get; }
    public string Place { get; }
    public string Recipient { get; }
    public string Street { get; }
    public float TotalPrice { get; }
    public float TotalPriceInclMWST { get; }
    public string ZIP { get; }


    public InvoiceDetails()
    {
        Date = DateTime.Now.ToString("dd.MM.yyyy");
        HourlyWage = 150;
        MWSTRate = 8.1f;

        // Determine the invoice period based on the current date
        DateTime currentDate = DateTime.Now;
        if (currentDate.Day <= 15)
        {
            FirstDateMonth = new DateTime(currentDate.Year, currentDate.Month - 1, 1).ToString("dd.MM.yyyy");
            LastDateMonth = new DateTime(currentDate.Year, currentDate.Month - 1, DateTime.DaysInMonth(currentDate.Year, currentDate.Month - 1)).ToString("dd.MM.yyyy");
            MonthYear = new DateTime(currentDate.Year, currentDate.Month - 1, 1).ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("de-DE"));
        }
        else
        {
            FirstDateMonth = new DateTime(currentDate.Year, currentDate.Month, 1).ToString("dd.MM.yyyy");
            LastDateMonth = new DateTime(currentDate.Year, currentDate.Month, DateTime.DaysInMonth(currentDate.Year, currentDate.Month)).ToString("dd.MM.yyyy");
            MonthYear = new DateTime(currentDate.Year, currentDate.Month, 1).ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("de-DE"));
        }

        Console.Write("Hours worked: ");
        string? hoursInput = Console.ReadLine();
        Hours = float.TryParse(hoursInput, out float h) ? h : 0f; // Default to 0 if parsing fails

        Place = "Test";
        Recipient = "Test Customer AG";
        Street = "Test Street";
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