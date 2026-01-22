using BankingApplication.Data;
using BankingApplication.Models;
using BankingApplication.Interfaces;
using Microsoft.EntityFrameworkCore;
using SimpleHashing.Net;

namespace BankingApplication.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly McbaContext _context;
    private static readonly ISimpleHash s_simpleHash = new SimpleHash();

    public AuthenticationService(McbaContext context)
    {
        _context = context;
    }

    public async Task<(bool Success, Customer Customer, string ErrorMessage)> AuthenticateAsync(string loginID, string password)
    {
        var login = await _context.Logins.Include(x => x.Customer).FirstOrDefaultAsync(x => x.LoginID == loginID);

        if (login == null || string.IsNullOrEmpty(password) || !VerifyPassword(password, login.PasswordHash))
        {
            return (false, null, "Login failed, please try again.");
        }

        return (true, login.Customer, string.Empty);
    }

    public bool VerifyPassword(string password, string hash)
    {
        return s_simpleHash.Verify(password, hash);
    }
}