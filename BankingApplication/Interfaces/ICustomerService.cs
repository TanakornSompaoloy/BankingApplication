using BankingApplication.Models;

namespace BankingApplication.Interfaces;

public interface ICustomerService
{
    public Task<Customer> GetCustomerAsync(int customerID);
    public Task<Customer> GetCustomerWithAccountsAsync(int customerID);
    public Task<bool> UpdateProfileAsync(int customerID, Customer updatedCustomer);
    public Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(int customerID, string currentPassword, string newPassword, string confirmPassword);
}