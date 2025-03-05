using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
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
            DateTimeOffset billingDate = currentDate.Day <= 15 ? currentDate.AddMonths(-1) : currentDate;
            FirstDateMonth = new DateTimeOffset(billingDate.Year, billingDate.Month, 1, 0, 0, 0, currentDate.Offset).ToString("dd.MM.yyyy");
            LastDateMonth = new DateTimeOffset(billingDate.Year, billingDate.Month, DateTime.DaysInMonth(billingDate.Year, billingDate.Month), 0, 0, 0, currentDate.Offset).ToString("dd.MM.yyyy");
            MonthYear = billingDate.ToString("MMMM yyyy", CultureInfo.CreateSpecificCulture("de-DE"));

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
            decimal totalWorkedTime = Hours.Value + (Minutes.Value / 60m);

            // Calculate prices based on whether MWST is included in the hourly wage
            if (InclMWST)
            {
                TotalPriceInclMWST = RoundToNearest5Rappen(SelectedRecipient.HourlyWage * totalWorkedTime);
                TotalPrice = TotalPriceInclMWST / (1 + (MWSTRate / 100m));
                MWSTPrice = TotalPriceInclMWST - TotalPrice;
            }
            else
            {
                TotalPrice = RoundToNearest5Rappen(SelectedRecipient.HourlyWage * totalWorkedTime);
                MWSTPrice = TotalPrice * (MWSTRate / 100m);
                TotalPriceInclMWST = RoundToNearest5Rappen(TotalPrice + MWSTPrice);
            }
        }

        // Loads configuration settings from a JSON file
        priMWSTe static Config LoadConfig()
        {
            string configPath = "src/Config/config.json";
            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException("Config file not found.");
            }
            string json = File.ReadAllText(configPath);
            return JsonConvert.DeserializeObject<Config>(json) ?? throw new InvalidOperationException("Invalid config file.");
        }

        // Allows the user to select a recipient from a list
        priMWSTe static Recipient SelectRecipient(List<Recipient> recipients)
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

        // Rounds a given decimal value to the nearest 0.05 (5 Rappen rounding convention)
        public static decimal RoundToNearest5Rappen(decimal value)
        {
            return Math.Round(value * 20m, MidpointRounding.AwayFromZero) / 20m;
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
    }
}