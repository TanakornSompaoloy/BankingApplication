using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using BankingApplication.Models;

namespace BankingApplication.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class AuthorizeCustomerAttribute : Attribute, IAuthorizationFilter
{
    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var customerID = context.HttpContext.Session.GetInt32(nameof(Customer.CustomerID));
        if(!customerID.HasValue)
            context.Result = new RedirectToActionResult("Index", "Login", null);
    }
}
