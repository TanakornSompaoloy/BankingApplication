using BankingApplication.Filters;
using BankingApplication.Models;
using BankingApplication.Interfaces;
using BankingApplication.ViewModels;
using Microsoft.AspNetCore.Mvc;
using X.PagedList.Extensions;

namespace McbaApplication.Controllers;

[AuthorizeCustomer]
public class CustomerController : Controller
{
    private readonly IAccountService _accountService;
    private readonly ITransactionService _transactionService;
    private readonly ICustomerService _customerService;
    private readonly IBillPayService _billPayService;
    private readonly IReportingService _reportingService;
    private int CustomerID => HttpContext.Session.GetInt32(nameof(Customer.CustomerID)).Value;

    public CustomerController(
        IAccountService accountService,
        ITransactionService transactionService,
        ICustomerService customerService,
        IBillPayService billPayService,
        IReportingService reportingService)
    {
        _accountService = accountService;
        _transactionService = transactionService;
        _customerService = customerService;
        _billPayService = billPayService;
        _reportingService = reportingService;
    }

    public async Task<IActionResult> Index()
    {
        var customer = await _customerService.GetCustomerWithAccountsAsync(CustomerID);
        return View(customer);
    }

    public async Task<IActionResult> Deposit()
    {
        var accounts = await _accountService.GetCustomerAccountsAsync(CustomerID);
        return View(accounts);
    }

    [HttpPost]
    public async Task<IActionResult> Deposit(int accountNumber, decimal amount, string comment)
    {
        var accounts = await _accountService.GetCustomerAccountsAsync(CustomerID);
        var account = accounts.Find(x => x.AccountNumber == accountNumber);

        var (isValid, errorMessage) = _transactionService.ValidateAmount(amount);
        if (!isValid)
        {
            ModelState.AddModelError(nameof(amount), errorMessage);
            ViewBag.Amount = amount;
            return View(accounts);
        }

        if (!string.IsNullOrEmpty(comment) && comment.Length > 30)
        {
            ModelState.AddModelError(nameof(comment), "Comment cannot exceed 30 characters.");
            ViewBag.Amount = amount;
            return View(accounts);
        }

        var viewModel = new TransactionConfirmationViewModel
        {
            Account = account,
            TransactionType = TransactionType.D,
            Amount = amount,
            Comment = comment,
            NewBalance = account.Balance + amount
        };

        return View(nameof(ConfirmTransaction), viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmTransaction(TransactionType transactionType, int accountNumber, decimal amount, string comment, int? destinationAccountNumber = null)
    {
        if (transactionType == TransactionType.D)
        {
            await _transactionService.ProcessDepositAsync(accountNumber, amount, comment);
            TempData["SuccessMessage"] = $"Deposit of {amount:C} successful!";
        }
        else if (transactionType == TransactionType.W)
        {
            await _transactionService.ProcessWithdrawalAsync(accountNumber, amount, comment);
            TempData["SuccessMessage"] = $"Withdrawal of {amount:C} successful!";
        }
        else if (transactionType == TransactionType.T && destinationAccountNumber.HasValue)
        {
            await _transactionService.ProcessTransferAsync(accountNumber, destinationAccountNumber.Value, amount, comment);
            TempData["SuccessMessage"] = $"Transfer of {amount:C} to account number {destinationAccountNumber} successful!";
        }

        return RedirectToAction("Index");
    }

    public async Task<IActionResult> Withdraw()
    {
        var accounts = await _accountService.GetCustomerAccountsAsync(CustomerID);
        return View(accounts);
    }

    [HttpPost]
    public async Task<IActionResult> Withdraw(int accountNumber, decimal amount, string comment)
    {
        var accounts = await _accountService.GetCustomerAccountsWithTransactionsAsync(CustomerID);
        var account = accounts.Find(x => x.AccountNumber == accountNumber);

        var (isValidAmount, amountError) = _transactionService.ValidateAmount(amount);
        if (!isValidAmount)
            ModelState.AddModelError(nameof(amount), amountError);

        var (isValidBalance, balanceError) = _transactionService.ValidateMinimumBalance(account, amount, TransactionType.W);
        if (!isValidBalance)
            ModelState.AddModelError(nameof(amount), balanceError);

        if (!string.IsNullOrEmpty(comment) && comment.Length > 30)
            ModelState.AddModelError(nameof(comment), "Comment cannot exceed 30 characters.");

        if (!ModelState.IsValid)
        {
            ViewBag.Amount = amount;
            return View(accounts);
        }

        var serviceCharge = _transactionService.GetServiceCharge(account, TransactionType.W);

        var viewModel = new TransactionConfirmationViewModel
        {
            Account = account,
            TransactionType = TransactionType.W,
            Amount = amount,
            Comment = comment,
            NewBalance = account.Balance - amount - serviceCharge,
            ServiceCharge = serviceCharge
        };

        return View(nameof(ConfirmTransaction), viewModel);
    }

    public async Task<IActionResult> Transfer()
    {
        var accounts = await _accountService.GetCustomerAccountsAsync(CustomerID);
        return View(accounts);
    }

    [HttpPost]
    public async Task<IActionResult> Transfer(int accountNumber, decimal amount, string comment, int destinationAccountNumber)
    {
        var accounts = await _accountService.GetCustomerAccountsWithTransactionsAsync(CustomerID);
        var account = accounts.Find(x => x.AccountNumber == accountNumber);

        var (isValidAmount, amountError) = _transactionService.ValidateAmount(amount);
        if (!isValidAmount)
            ModelState.AddModelError(nameof(amount), amountError);

        var (isValidBalance, balanceError) = _transactionService.ValidateMinimumBalance(account, amount, TransactionType.T);
        if (!isValidBalance)
            ModelState.AddModelError(nameof(amount), balanceError);

        if (!string.IsNullOrEmpty(comment) && comment.Length > 30)
            ModelState.AddModelError(nameof(comment), "Comment cannot exceed 30 characters.");

        // Check if destination account exists and is different from source account
        var destinationAccount = await _accountService.GetAccountByNumberAsync(destinationAccountNumber);
        if (destinationAccount == null)
            ModelState.AddModelError(nameof(destinationAccountNumber), "Destination account does not exist.");
        else if (accountNumber == destinationAccountNumber)
            ModelState.AddModelError(nameof(destinationAccountNumber), "Cannot transfer to the same account.");

        if (!ModelState.IsValid)
        {
            ViewBag.Amount = amount;
            return View(accounts);
        }

        var serviceCharge = _transactionService.GetServiceCharge(account, TransactionType.T);

        var viewModel = new TransactionConfirmationViewModel
        {
            Account = account,
            DestinationAccountNumber = destinationAccountNumber,
            TransactionType = TransactionType.T,
            Amount = amount,
            Comment = comment,
            NewBalance = account.Balance - amount - serviceCharge,
            ServiceCharge = serviceCharge
        };

        return View(nameof(ConfirmTransaction), viewModel);
    }

    public async Task<IActionResult> Statements(int? selectedAccountNumber = null, int page = 1)
    {
        var accounts = await _accountService.GetCustomerAccountsWithTransactionsAsync(CustomerID);

        var viewModel = new StatementViewModel
        {
            Accounts = accounts,
            SelectedAccountNumber = selectedAccountNumber
        };

        if (selectedAccountNumber.HasValue)
        {
            var selectedAccount = accounts.FirstOrDefault(a => a.AccountNumber == selectedAccountNumber.Value);
            if (selectedAccount != null)
            {
                // Page the transactions, maximum of 4 per page.
                const int pageSize = 4;
                var transactions = selectedAccount.Transactions.OrderByDescending(t => t.TransactionTimeUtc).ToList();
                viewModel.PagedTransactions = transactions.ToPagedList(page, pageSize);
            }
        }
        else
        {
            viewModel.PagedTransactions = new List<Transaction>().ToPagedList(1, 4);
        }

        return View(viewModel);
    }

    public async Task<IActionResult> ExportTransactions(int accountNumber)
    {
        var pdfBytes = await _reportingService.GenerateTransactionPdfAsync(accountNumber, CustomerID);
        Response.Headers.Append("Content-Disposition", $"inline; filename=\"TransactionHistory_Account_{accountNumber}.pdf\"");
        return File(pdfBytes, "application/pdf");
    }

    public async Task<IActionResult> Profile()
    {
        var customer = await _customerService.GetCustomerAsync(CustomerID);
        return View(customer);
    }

    public async Task<IActionResult> EditProfile()
    {
        var customer = await _customerService.GetCustomerAsync(CustomerID);
        return View(customer);
    }

    [HttpPost]
    public async Task<IActionResult> EditProfile(Customer editedCustomer)
    {
        if (!ModelState.IsValid)
        {
            return View(editedCustomer);
        }

        var success = await _customerService.UpdateProfileAsync(CustomerID, editedCustomer);

        if (!success)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Profile updated successfully!";
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> EditPassword()
    {
        var customer = await _customerService.GetCustomerAsync(CustomerID);
        return View(customer);
    }

    [HttpPost]
    public async Task<IActionResult> EditPassword(string currentPassword, string newPassword, string confirmPassword)
    {
        var (success, errorMessage) = await _customerService.ChangePasswordAsync(CustomerID, currentPassword, newPassword, confirmPassword);

        if (!success)
        {
            var customer = await _customerService.GetCustomerAsync(CustomerID);

            if (errorMessage.Contains("Login information not found"))
                TempData["ErrorMessage"] = errorMessage;
            else if (errorMessage.Contains("Current password"))
                ModelState.AddModelError("currentPassword", errorMessage);
            else if (errorMessage.Contains("New password") && errorMessage.Contains("required"))
                ModelState.AddModelError("newPassword", errorMessage);
            else if (errorMessage.Contains("confirm"))
                ModelState.AddModelError("confirmPassword", errorMessage);
            else if (errorMessage.Contains("do not match"))
                ModelState.AddModelError("confirmPassword", errorMessage);

            return View(customer);
        }

        TempData["SuccessMessage"] = "Password changed successfully!";
        return RedirectToAction(nameof(Profile));
    }

    public async Task<IActionResult> BillPay()
    {
        var billPays = await _billPayService.GetCustomerBillPaysAsync(CustomerID);
        return View(billPays);
    }

    public async Task<IActionResult> CreateBillPay()
    {
        var accounts = await _accountService.GetCustomerAccountsAsync(CustomerID);
        var payees = await _billPayService.GetAllPayeesAsync();

        var viewModel = new CreateBillPayViewModel
        {
            Accounts = accounts,
            Payees = payees
        };

        return View(viewModel);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBillPay(CreateBillPayViewModel model)
    {
        if (model.Amount.HasValue)
        {
            var (isValid, errorMessage) = _transactionService.ValidateAmount(model.Amount.Value);
            if (!isValid)
                ModelState.AddModelError(nameof(model.Amount), errorMessage);
        }

        if (!ModelState.IsValid)
        {
            model.Accounts = await _accountService.GetCustomerAccountsAsync(CustomerID);
            model.Payees = await _billPayService.GetAllPayeesAsync();
            return View(model);
        }

        var (success, billPayError) = await _billPayService.CreateBillPayAsync(
            model.AccountNumber,
            model.PayeeId,
            model.Amount.Value,
            model.ScheduleTime,
            model.Period,
            CustomerID);

        if (!success)
        {
            ModelState.AddModelError(string.Empty, billPayError);
            model.Accounts = await _accountService.GetCustomerAccountsAsync(CustomerID);
            model.Payees = await _billPayService.GetAllPayeesAsync();
            return View(model);
        }

        TempData["SuccessMessage"] = "Bill pay created successfully!";
        return RedirectToAction(nameof(BillPay));
    }

    [HttpPost]
    public async Task<IActionResult> CancelBillPay(int billPayId)
    {
        var (success, errorMessage) = await _billPayService.CancelBillPayAsync(billPayId, CustomerID);

        if (!success)
        {
            TempData["ErrorMessage"] = errorMessage;
            return RedirectToAction(nameof(BillPay));
        }

        TempData["SuccessMessage"] = "Bill pay cancelled successfully!";
        return RedirectToAction(nameof(BillPay));
    }
}
