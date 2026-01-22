using BankingApplication.Models;

namespace BankingApplication.Interfaces;

public interface IAuthenticationService
{
    public Task<(bool Success, Customer Customer, string ErrorMessage)> AuthenticateAsync(string loginID, string password);
    public bool VerifyPassword(string password, string hash);
}