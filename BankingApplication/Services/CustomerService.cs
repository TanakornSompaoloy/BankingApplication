using BankingApplication.Data;
using BankingApplication.Models;
using BankingApplication.Interfaces;
using Microsoft.EntityFrameworkCore;
using SimpleHashing.Net;

namespace BankingApplication.Services;

public class CustomerService : ICustomerService
{
    private readonly McbaContext _context;
    private static readonly ISimpleHash s_simpleHash = new SimpleHash();

    public CustomerService(McbaContext context)
    {
        _context = context;
    }

    public async Task<Customer> GetCustomerAsync(int customerID)
    {
        return await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == customerID);
    }

    public async Task<Customer> GetCustomerWithAccountsAsync(int customerID)
    {
        return await _context.Customers
            .Include(c => c.Accounts)
            .FirstOrDefaultAsync(c => c.CustomerID == customerID);
    }

    public async Task<bool> UpdateProfileAsync(int customerID, Customer updatedCustomer)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerID == customerID);

        if (customer == null)
            return false;

        customer.Name = updatedCustomer.Name;
        customer.TFN = updatedCustomer.TFN;
        customer.Address = updatedCustomer.Address;
        customer.City = updatedCustomer.City;
        customer.State = updatedCustomer.State;
        customer.Postcode = updatedCustomer.Postcode;
        customer.Mobile = updatedCustomer.Mobile;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string ErrorMessage)> ChangePasswordAsync(int customerID, string currentPassword, string newPassword, string confirmPassword)
    {
        var login = await _context.Logins.FirstOrDefaultAsync(l => l.CustomerID == customerID);

        if (login == null)
            return (false, "Login information not found.");

        if (string.IsNullOrEmpty(currentPassword))
            return (false, "Current password is required.");

        if (string.IsNullOrEmpty(newPassword))
            return (false, "New password is required.");

        if (string.IsNullOrEmpty(confirmPassword))
            return (false, "Please confirm your new password.");

        if (!string.IsNullOrEmpty(currentPassword) && !s_simpleHash.Verify(currentPassword, login.PasswordHash))
            return (false, "Current password is incorrect.");

        if (!string.IsNullOrEmpty(newPassword) && !string.IsNullOrEmpty(confirmPassword) && newPassword != confirmPassword)
            return (false, "New password and confirmation do not match.");

        login.PasswordHash = s_simpleHash.Compute(newPassword);
        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }
}