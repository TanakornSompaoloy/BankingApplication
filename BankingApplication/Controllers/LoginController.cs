using BankingApplication.Models;
using BankingApplication.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankingApplication.Controllers;

public class LoginController : Controller
{
    private readonly IAuthenticationService _authenticationService;

    public LoginController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Index(string loginID, string password)
    {
        var (success, customer, errorMessage) = await _authenticationService.AuthenticateAsync(loginID, password);

        if (!success)
        {
            ModelState.AddModelError("LoginFailed", errorMessage);
            return View(new Login { LoginID = loginID });
        }

        // Login customer.
        HttpContext.Session.SetInt32(nameof(Customer.CustomerID), customer.CustomerID);
        HttpContext.Session.SetString(nameof(Customer.Name), customer.Name);

        return RedirectToAction("Index", "Customer");
    }

    public IActionResult Logout()
    {
        // Logout customer.
        HttpContext.Session.Clear();

        return RedirectToAction("Index", "Home");
    }
}
