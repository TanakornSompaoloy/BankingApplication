using BankingApplication.Models;

namespace BankingApplication.Interfaces;

public interface IReportingService
{
    public Task<byte[]> GenerateTransactionPdfAsync(int accountNumber, int customerID);
}