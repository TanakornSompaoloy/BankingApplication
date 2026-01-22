using BankingApplication.Data;
using BankingApplication.Models;
using BankingApplication.Interfaces;
using BankingApplication.Utilities;
using Microsoft.EntityFrameworkCore;

namespace BankingApplication.Services;

public class TransactionService : ITransactionService
{
    private readonly McbaContext _context;
    private const decimal WithdrawalServiceCharge = 0.01m;
    private const decimal TransferServiceCharge = 0.05m;
    private const int FreeTransactions = 2;
    private const decimal OverdraftLimit = 500;

    public TransactionService(McbaContext context)
    {
        _context = context;
    }

    public (bool IsValid, string ErrorMessage) ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            return (false, "Amount must be positive.");
        }

        if (amount.HasMoreThanTwoDecimalPlaces())
        {
            return (false, "Amount cannot have more than 2 decimal places.");
        }

        return (true, string.Empty);
    }

    public (bool IsValid, string ErrorMessage) ValidateMinimumBalance(Account account, decimal amount, TransactionType transactionType)
    {
        var serviceCharge = GetServiceCharge(account, transactionType);
        var newBalance = account.Balance - amount - serviceCharge;
        var minimumBalance = account.AccountType == AccountType.S ? 0 : -OverdraftLimit;

        if (newBalance < minimumBalance)
        {
            return (false, "Insufficient funds.");
        }

        return (true, string.Empty);
    }

    public decimal GetServiceCharge(Account account, TransactionType transactionType)
    {
        var chargeableCount = account.Transactions.Count(t => t.TransactionType == TransactionType.W ||
                                                            (t.TransactionType == TransactionType.T && t.DestinationAccountNumber != null));

        return chargeableCount >= FreeTransactions
            ? (transactionType == TransactionType.W ? WithdrawalServiceCharge : TransferServiceCharge) : 0;
    }

    public void ApplyServiceCharge(Account account, TransactionType transactionType)
    {
        var serviceCharge = GetServiceCharge(account, transactionType);

        if (serviceCharge > 0)
        {
            account.Balance -= serviceCharge;

            account.Transactions.Add(new Transaction
            {
                TransactionType = TransactionType.S,
                Amount = serviceCharge,
                TransactionTimeUtc = DateTime.UtcNow
            });
        }
    }
    
    public async Task ProcessDepositAsync(int accountNumber, decimal amount, string comment)
    {
        var account = await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        account.Balance += amount;

        account.Transactions.Add(new Transaction
        {
            TransactionType = TransactionType.D,
            Amount = amount,
            Comment = comment,
            TransactionTimeUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task ProcessWithdrawalAsync(int accountNumber, decimal amount, string comment)
    {
        var account = await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        account.Balance -= amount;
        ApplyServiceCharge(account, TransactionType.W);

        account.Transactions.Add(new Transaction
        {
            TransactionType = TransactionType.W,
            Amount = amount,
            Comment = comment,
            TransactionTimeUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }

    public async Task ProcessTransferAsync(int sourceAccountNumber, int destinationAccountNumber, decimal amount, string comment)
    {
        var sourceAccount = await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountNumber == sourceAccountNumber);

        var destinationAccount = await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountNumber == destinationAccountNumber);

        // Deduct from source account
        sourceAccount.Balance -= amount;
        ApplyServiceCharge(sourceAccount, TransactionType.T);

        // Add transaction to source account
        sourceAccount.Transactions.Add(new Transaction
        {
            TransactionType = TransactionType.T,
            Amount = amount,
            Comment = comment,
            DestinationAccountNumber = destinationAccountNumber,
            TransactionTimeUtc = DateTime.UtcNow
        });

        // Add to destination account
        destinationAccount.Balance += amount;

        // Add transaction to destination account
        destinationAccount.Transactions.Add(new Transaction
        {
            TransactionType = TransactionType.T,
            Amount = amount,
            Comment = comment,
            TransactionTimeUtc = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();
    }
}