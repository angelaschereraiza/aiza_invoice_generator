using System.Globalization;
using Newtonsoft.Json;

namespace InvoiceGenerator.Models
{
    public class Invoice
    {
        // Properties defining invoice details
        public Recipient SelectedRecipient { get; } // Selected recipient details
        public decimal MWSTRate { get; } // MWST rate
        public bool InclMWST { get; } // Indicates if the hourly wage includes MWST
        public string Date { get; } // Invoice issue date
        public string FirstDateMonth { get; } // First date of the billing month
        public decimal? Hours { get; } // Hours worked
        public decimal? Minutes { get; } // Minutes worked
        public string LastDateMonth { get; } // Last date of the billing month
        public decimal MWSTPrice { get; } // MWST amount applied to the invoice
        public string MonthYear { get; } // Billing month and year in readable format
        public decimal TotalPrice { get; } // Total price before MWST (if MWST is not included)
        public decimal TotalPriceInclMWST { get; } // Total price including MWST

        public Invoice()
        {
            // Load configuration settings from a JSON file
            var config = LoadConfig();
            SelectedRecipient = SelectRecipient(config.Recipients);
            
            // Assign recipient-specific values
            MWSTRate = config.MWSTRate;
            InclMWST = SelectedRecipient.InclMWST;

            // Set the invoice issue date to the current date
            DateTimeOffset currentDate = DateTimeOffset.Now;
            Date = currentDate.ToString("dd.MM.yyyy");

            // Determine the billing period
            DateTimeOffset billingDate = currentDate.AddMonths(-1);
            FirstDateMonth = new DateTimeOffset(billingDate.Year, billingDate.Month, 1, 0, 0, 0, currentDate.Offset).ToString("dd.MM.yyyy");
            LastDateMonth = new DateTimeOffset(billingDate.Year, billingDate.Month, DateTime.DaysInMonth(billingDate.Year, billingDate.Month), 0, 0, 0, currentDate.Offset).ToString("dd.MM.yyyy");
            MonthYear = billingDate.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("de-DE"));

            if (SelectedRecipient.Multiple)
            {
                Hours = 1m;
                Minutes = 0m;
                decimal grossTotal = SelectedRecipient.Price + SelectedRecipient.Price1 + SelectedRecipient.Price2;

                if (SelectedRecipient.InclMWST)
                {
                    decimal divisor = 1 + (MWSTRate / 100m);
                    TotalPrice = RoundToNearest5Rappen(grossTotal / divisor);
                    TotalPriceInclMWST = RoundToNearest5Rappen(grossTotal);
                    MWSTPrice = RoundToNearest5Rappen(TotalPriceInclMWST - TotalPrice);

                    SelectedRecipient.Price = Math.Round(SelectedRecipient.Price / divisor, 2);
                    SelectedRecipient.Price1 = Math.Round(SelectedRecipient.Price1 / divisor, 2);
                    SelectedRecipient.Price2 = Math.Round(SelectedRecipient.Price2 / divisor, 2);
                }
                else
                {
                    TotalPrice = RoundToNearest5Rappen(grossTotal);
                    TotalPriceInclMWST = RoundToNearest5Rappen(grossTotal + (grossTotal * (MWSTRate / 100m)) + SelectedRecipient.Expenses);
                    MWSTPrice = TotalPriceInclMWST - TotalPrice;
                }
            }
            else
            {
                // Prompt user for worked hours and minutes
                Console.Write("Hours worked: ");
                string? hoursInput = Console.ReadLine();
                Hours = decimal.TryParse(hoursInput, out decimal h) ? h : 0m;

                Console.Write("Minutes worked: ");
                string? minutesInput = Console.ReadLine();
                Minutes = decimal.TryParse(minutesInput, out decimal m) ? m : 0m;

                if (Hours == 0m && Minutes == 0m)
                {
                    throw new ArgumentException("Invoice file should not be generated for zero hours and zero minutes.");
                }

                // Convert minutes to fractional hours
                Hours = Hours.Value + (Minutes.Value / 60m);

                // Calculate prices based on whether MWST is included in the hourly wage
                if (InclMWST)
                {
                    TotalPriceInclMWST = RoundToNearest5Rappen(SelectedRecipient.HourlyWage * Hours.Value);
                    TotalPrice = TotalPriceInclMWST / (1 + (MWSTRate / 100m));
                    MWSTPrice = TotalPriceInclMWST - TotalPrice;
                    SelectedRecipient.HourlyWage = SelectedRecipient.HourlyWage / (1 + (MWSTRate / 100m));
                }
                else
                {
                    TotalPrice = RoundToNearest5Rappen(SelectedRecipient.HourlyWage * Hours.Value);
                    MWSTPrice = TotalPrice * (MWSTRate / 100m) + SelectedRecipient.Expenses * (MWSTRate / 100m);
                    TotalPriceInclMWST = RoundToNearest5Rappen(TotalPrice + MWSTPrice + SelectedRecipient.Expenses);
                }

                Hours = Math.Round(Hours.Value, 2);
            }
        }

        // Loads configuration settings from a JSON file
        public static Config LoadConfig()
        {
            string configPath = Path.Combine(Directory.GetCurrentDirectory(), "Config", "config.json");
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException("Config file not found.");
            }
            string json = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<Config>(json) ?? throw new InvalidOperationException("Invalid config file.");
        }

        // Allows the user to select a recipient from a list
        public static Recipient SelectRecipient(List<Recipient> recipients)
        {
            Console.WriteLine("Select a recipient:");
            for (int i = 0; i < recipients.Count; i++)
            {
                Console.WriteLine($"[{i + 1}] {recipients[i].Name}");
            }
            Console.Write("Enter number: ");
            string? input = Console.ReadLine();
            if (int.TryParse(input, out int index) && index > 0 && index <= recipients.Count)
            {
                return recipients[index - 1];
            }
            throw new ArgumentException("Invalid selection.");
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

    // Configuration class containing MWST rate and recipient list
    public class Config
    {
        public decimal MWSTRate { get; set; } // MWST rate
        public List<Recipient> Recipients { get; set; } = new(); // List of available recipients
    }

    // Represents a recipient/customer with relevant details
    public class Recipient
    {
        public string Name { get; set; } = string.Empty; // Recipient name
        public string Street { get; set; } = string.Empty; // Street address
        public string ZIP { get; set; } = string.Empty; // ZIP code
        public string Place { get; set; } = string.Empty; // City or place
        public decimal HourlyWage { get; set; } // Hourly wage
        public bool InclMWST { get; set; } // Indicates if MWST is included in the hourly wage
        public string TypeOfService { get; set; } = string.Empty; // Defines the type of service of the invoice
        public string TypeOfService1 { get; set; } = string.Empty; // First additional service for multi-service invoices
        public string TypeOfService2 { get; set; } = string.Empty; // Second additional service for multi-service invoices
        public decimal Price { get; set; } // Price for the main service of a multi-service invoice
        public decimal Price1 { get; set; } // Price for the first additional service of a multi-service invoice
        public decimal Price2 { get; set; } // Price for the second additional service of a multi-service invoice
        public bool IsMonthlyPeriod { get; set; } // Determines whether the first and last date of the month should be displayed
        public decimal Expenses { get; set; } = 118.60m; // Total expenses
        public bool Multiple { get; set; } // Marks recipients that require the multi-service invoice template
        public string Organisation { get; set; } = string.Empty; // Organisation or department for multi-service invoices
        public string InvoiceTitle { get; set; } = string.Empty; // Invoice title for multi-service invoices
    }
}