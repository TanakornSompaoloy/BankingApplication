using BankingApplication.Models;

namespace BankingApplication.Interfaces;

public interface ITransactionService
{
    public (bool IsValid, string ErrorMessage) ValidateAmount(decimal amount);
    public (bool IsValid, string ErrorMessage) ValidateMinimumBalance(Account account, decimal amount, TransactionType transactionType);
    public decimal GetServiceCharge(Account account, TransactionType transactionType);
    public void ApplyServiceCharge(Account account, TransactionType transactionType);
    public Task ProcessDepositAsync(int accountNumber, decimal amount, string comment);
    public Task ProcessWithdrawalAsync(int accountNumber, decimal amount, string comment);
    public Task ProcessTransferAsync(int sourceAccountNumber, int destinationAccountNumber, decimal amount, string comment);
}