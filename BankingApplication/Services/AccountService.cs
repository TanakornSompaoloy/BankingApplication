using BankingApplication.Data;
using BankingApplication.Models;
using BankingApplication.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankingApplication.Services;

public class AccountService : IAccountService
{
    private readonly McbaContext _context;

    public AccountService(McbaContext context)
    {
        _context = context;
    }

    public async Task<Account> GetAccountByNumberAsync(int accountNumber)
    {
        return await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<Account> GetAccountWithTransactionsAsync(int accountNumber)
    {
        return await _context.Accounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);
    }

    public async Task<List<Account>> GetCustomerAccountsAsync(int customerID)
    {
        var customer = await _context.Customers
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.CustomerID == customerID);

        return customer?.Accounts ?? new List<Account>();
    }

    public async Task<List<Account>> GetCustomerAccountsWithTransactionsAsync(int customerID)
    {
        var customer = await _context.Customers
            .Include(c => c.Accounts)
            .ThenInclude(a => a.Transactions)
            .FirstOrDefaultAsync(c => c.CustomerID == customerID);

        return customer?.Accounts ?? new List<Account>();
    }
}