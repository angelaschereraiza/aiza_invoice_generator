// Class to hold invoice details
public class InvoiceDetails
{
    public string Recipient { get; }
    public string Street { get; }
    public string Place { get; }
    public int HourlyWage { get; }
    public int Mwst { get; }
    public string Date { get; }
    public string Hours { get; }

    public InvoiceDetails()
    {
        Recipient = "Customer AG";
        Street = "Test Street";
        Place = "0000 Test";
        HourlyWage = 130;
        Mwst = 8;
        Date = DateTime.Now.ToString("dd.MM.yyyy");
        Console.Write("Hours worked: ");
        string? hoursInput = Console.ReadLine();
        Hours = hoursInput ?? "0"; // Default to "0" if hoursInput is null
    }
}