using BankingApplication.Models;

namespace BankingApplication.Interfaces;

public interface IBillPayService
{
    public Task<List<BillPay>> GetCustomerBillPaysAsync(int customerID);
    public Task<(bool Success, string ErrorMessage)> CreateBillPayAsync(int accountNumber, int payeeID, decimal amount, DateTime scheduleTime, PeriodType period, int customerID);
    public Task<(bool Success, string ErrorMessage)> CancelBillPayAsync(int billPayID, int customerID);
    public Task<List<BillPay>> GetPendingBillPaymentsAsync(DateTime currentTime);
    public void ProcessBillPayment(BillPay billPay);
    public Task ProcessScheduledBillPaymentsAsync();
    public Task<List<Payee>> GetAllPayeesAsync();
}