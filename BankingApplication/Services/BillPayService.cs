using BankingApplication.Data;
using BankingApplication.Models;
using BankingApplication.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BankingApplication.Services;

public class BillPayService : IBillPayService
{
    private readonly McbaContext _context;
    private const decimal OverdraftLimit = 500;

    public BillPayService(McbaContext context)
    {
        _context = context;
    }

    public async Task<List<BillPay>> GetCustomerBillPaysAsync(int customerID)
    {
        var customer = await _context.Customers
            .Include(c => c.Accounts)
            .ThenInclude(a => a.BillPay)
            .ThenInclude(bp => bp.Payee)
            .FirstOrDefaultAsync(c => c.CustomerID == customerID);

        var billPays = customer.Accounts.SelectMany(a => a.BillPay).OrderByDescending(bp => bp.BillPayID).ToList();

        return billPays;
    }

    public async Task<(bool Success, string ErrorMessage)> CreateBillPayAsync(int accountNumber, int payeeID, decimal amount, DateTime scheduleTime, PeriodType period, int customerID)
    {
        // Validate account
        var account = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountNumber == accountNumber);

        // Validate schedule time
        if (scheduleTime < DateTime.Now)
            return (false, "Schedule time must be in the future.");

        // Validate payee
        var payee = await _context.Payees.FindAsync(payeeID);
        if (payee == null)
            return (false, "Invalid payee selected.");

        var billPay = new BillPay
        {
            AccountNumber = accountNumber,
            PayeeID = payeeID,
            Amount = amount,
            ScheduleTimeUtc = scheduleTime.ToUniversalTime(),
            Period = period,
            Status = BillPayStatus.P
        };

        _context.BillPays.Add(billPay);
        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<(bool Success, string ErrorMessage)> CancelBillPayAsync(int billPayID, int customerID)
    {
        var billPay = await _context.BillPays
            .Include(bp => bp.Account)
            .FirstOrDefaultAsync(bp => bp.BillPayID == billPayID && bp.Account.CustomerID == customerID);

        if (billPay == null)
            return (false, "Bill pay not found.");

        billPay.Status = BillPayStatus.C;
        await _context.SaveChangesAsync();

        return (true, string.Empty);
    }

    public async Task<List<BillPay>> GetPendingBillPaymentsAsync(DateTime currentTime)
    {
        var allPendingBills = await _context.BillPays
            .Include(bp => bp.Account)
            .ThenInclude(a => a.Transactions)
            .Include(bp => bp.Payee)
            .Where(bp => bp.Status == BillPayStatus.P && !bp.IsBlocked)
            .ToListAsync();

        return allPendingBills
            .Where(bp => bp.ScheduleTimeUtc.ToLocalTime() <= currentTime)
            .ToList();
    }

    public void ProcessBillPayment(BillPay billPay)
    {
        var account = billPay.Account;
        var minimumBalance = account.AccountType == AccountType.S ? 0 : -OverdraftLimit;

        if (account.Balance - billPay.Amount < minimumBalance)
        {
            billPay.Status = BillPayStatus.F;
            return;
        }

        account.Balance -= billPay.Amount;

        var transaction = new Transaction
        {
            TransactionType = TransactionType.B,
            Amount = billPay.Amount,
            TransactionTimeUtc = DateTime.UtcNow
        };

        account.Transactions.Add(transaction);

        billPay.Status = BillPayStatus.S;

        if (billPay.Period == PeriodType.M)
        {
            var nextScheduleLocal = billPay.ScheduleTimeUtc.ToLocalTime().AddMonths(1);
            billPay.ScheduleTimeUtc = nextScheduleLocal.ToUniversalTime();
            billPay.Status = BillPayStatus.P;
        }
    }

    public async Task ProcessScheduledBillPaymentsAsync()
    {
        var currentTime = DateTime.Now.ToLocalTime();
        var billPays = await GetPendingBillPaymentsAsync(currentTime);

        foreach (var billPay in billPays)
        {
            ProcessBillPayment(billPay);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<List<Payee>> GetAllPayeesAsync()
    {
        return await _context.Payees.ToListAsync();
    }
}