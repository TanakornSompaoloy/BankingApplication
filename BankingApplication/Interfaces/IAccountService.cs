using BankingApplication.Models;

namespace BankingApplication.Interfaces;

public interface IAccountService
{
    public Task<Account> GetAccountByNumberAsync(int accountNumber);
    public Task<Account> GetAccountWithTransactionsAsync(int accountNumber);
    public Task<List<Account>> GetCustomerAccountsAsync(int customerID);
    public Task<List<Account>> GetCustomerAccountsWithTransactionsAsync(int customerID);
}