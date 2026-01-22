using BankingApplication.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace BankingApplication.Data;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<McbaContext>();

        Initialize(context);
    }

    public static void Initialize(McbaContext context)
    {

        // Look for customers.
        if (context.Customers.Any())
            return; // DB has already been seeded.

        //Fetch customers data from API and create list of customer objects
        const string Url = "https://coreteaching01.csit.rmit.edu.au/~e103884/wdt/services/customers/";

        using var client = new HttpClient();
        var json = client.GetStringAsync(Url).Result;
        
        //Deserialize data from API
        var customers = JsonConvert.DeserializeObject<List<Customer>>(json, new JsonSerializerSettings
        {
            DateFormatString = "dd/MM/yyyy hh:mm:ss tt"
        });

        foreach (var customer in customers)
        {
            // Add customer
            context.Customers.Add(customer);

            // Add login
            if (customer.Login != null)
            {
                customer.Login.CustomerID = customer.CustomerID;
                context.Logins.Add(customer.Login);
            }

            // Add accounts
            if (customer.Accounts != null)
            {
                foreach (var account in customer.Accounts)
                {
                    account.CustomerID = customer.CustomerID;
                    context.Accounts.Add(account);

                    //Add transactions and calculate balance
                    if (account.Transactions != null)
                    {
                        decimal totalBalance = 0;
                        foreach (var transaction in account.Transactions)
                        {
                            transaction.AccountNumber = account.AccountNumber;
                            transaction.TransactionType = TransactionType.D;
                            totalBalance += transaction.Amount;
                            context.Transactions.Add(transaction);
                        }
                        // Set the balance
                        account.Balance = totalBalance;
                    }
                }
            }
        }

        context.Payees.AddRange(
            new Payee
            {
                Name = "Optus",
                Address = "123 Swanston Street",
                City = "Melbourne",
                PostCode = "3000",
                State = State.VIC,
                Phone = "(04) 1234 5678"
            },
            new Payee
            {
                Name = "Telstra",
                Address = "123 Flinder Street",
                City = "Melbourne",
                PostCode = "3000",
                State = State.VIC,
                Phone = "(04) 5555 5555"
            },
            new Payee
            {
                Name = "Vodafone",
                Address = "123 Main Street",
                City = "Sydney",
                PostCode = "2000",
                State = State.NSW,
                Phone = "(01) 1234 4567"
            });

        context.SaveChanges();
    }
}
