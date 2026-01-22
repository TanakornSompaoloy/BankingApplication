using BankingApplication.Data;
using BankingApplication.Models;
using BankingApplication.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace BankingApplication.Services;

public class ReportingService : IReportingService
{
    private readonly McbaContext _context;

    public ReportingService(McbaContext context)
    {
        _context = context;
    }

    public async Task<byte[]> GenerateTransactionPdfAsync(int accountNumber, int customerID)
    {
        var customer = await _context.Customers
            .Include(c => c.Accounts)
            .ThenInclude(a => a.Transactions)
            .FirstOrDefaultAsync(c => c.CustomerID == customerID);

        var account = customer.Accounts.FirstOrDefault(a => a.AccountNumber == accountNumber);


        //Use QuestPdf to create pdf
        //https://www.milanjovanovic.tech/blog/how-to-easily-create-pdf-documents-in-aspnetcore
        //https://www.youtube.com/watch?v=_M0IgtGWnvE&t=201s
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);

                page.Header()
                    .Text($"Transaction History - Account {account.AccountNumber}")
                    .SemiBold().FontSize(20).FontColor(Colors.Black);

                page.Content()
                    .PaddingVertical(1, Unit.Centimetre)
                    .Column(column =>
                    {
                        column.Item().Text($"Customer: {customer.Name}").FontSize(12);
                        column.Item().Text($"Address: {customer.Address}").FontSize(12);
                        column.Item().Text($"City: {customer.City}, {customer.Postcode}").FontSize(12);
                        column.Item().Text($"Account Type: {(account.AccountType == AccountType.S ? "Savings" : "Checking")}").FontSize(12);
                        column.Item().Text($"Current Balance: {account.Balance:C}").FontSize(12).SemiBold();
                        column.Item().PaddingTop(20);

                        if (account.Transactions.Any())
                        {
                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(0.8f);
                                    columns.RelativeColumn(1.2f);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2.5f);
                                });

                                // Header
                                table.Header(header =>
                                {
                                    header.Cell().Element(CellStyle).Text("ID").FontSize(10).SemiBold();
                                    header.Cell().Element(CellStyle).Text("Type").FontSize(10).SemiBold();
                                    header.Cell().Element(CellStyle).Text("Destination").FontSize(10).SemiBold();
                                    header.Cell().Element(CellStyle).Text("Amount").FontSize(10).SemiBold();
                                    header.Cell().Element(CellStyle).Text("Time").FontSize(10).SemiBold();
                                    header.Cell().Element(CellStyle).Text("Comment").FontSize(10).SemiBold();
                                });

                                // Rows
                                foreach (var transaction in account.Transactions.OrderBy(t => t.TransactionID))
                                {
                                    var typeText = transaction.TransactionType switch
                                    {
                                        TransactionType.D => "Deposit",
                                        TransactionType.W => "Withdrawal",
                                        TransactionType.T => "Transfer",
                                        TransactionType.S => "Service Charge",
                                        _ => transaction.TransactionType.ToString()
                                    };

                                    table.Cell().Element(CellStyle).Text(transaction.TransactionID.ToString()).FontSize(9);
                                    table.Cell().Element(CellStyle).Text(typeText).FontSize(9);
                                    table.Cell().Element(CellStyle).Text(transaction.DestinationAccountNumber?.ToString() ?? "").FontSize(9);
                                    table.Cell().Element(CellStyle).Text(transaction.Amount.ToString("C")).FontSize(9);
                                    table.Cell().Element(CellStyle).Text(transaction.TransactionTimeUtc.ToLocalTime().ToString("dd/MM/yyyy HH:mm tt")).FontSize(9);
                                    table.Cell().Element(CellStyle).Text(transaction.Comment ?? "").FontSize(9);
                                }
                            });
                        }
                    });
            });
        });

        return document.GeneratePdf();
    }

    private static IContainer CellStyle(IContainer container)
    {
        return container.DefaultTextStyle(x => x.FontSize(9))
                       .PaddingVertical(3)
                       .BorderBottom(1)
                       .BorderColor(Colors.Grey.Lighten2);
    }
}