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
        HourlyWage = 110;
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
        
    // Method to round a float to the nearest 0.05
    private static float RoundToNearest5Rappen(float value)
    {
        return (float)(Math.Round(value * 20, MidpointRounding.AwayFromZero) / 20.0);
    }

    // Method to format the float value as currency with two decimal places and thousands separator
    public string FormatCurrency(float value)
    {
        return string.Format(CultureInfo.CreateSpecificCulture("de-CH"), "{0:N2}", value);
    }
}