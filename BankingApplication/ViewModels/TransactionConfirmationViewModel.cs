using BankingApplication.Models;

namespace BankingApplication.ViewModels;

public class TransactionConfirmationViewModel
{
    public Account Account { get; set; }
    public TransactionType TransactionType { get; set; }
    public decimal Amount { get; set; }
    public string Comment { get; set; }
    public decimal NewBalance { get; set; }
    public int? DestinationAccountNumber { get; set; }
    public decimal ServiceCharge { get; set; }
    
    public string DisplayTransactionType => TransactionType switch
    {
        TransactionType.D => "Deposit",
        TransactionType.W => "Withdrawal",
        TransactionType.T => "Transfer",
        TransactionType.S => "Service Charge",
        TransactionType.B => "Bill Pay",
        _ => throw new NotImplementedException()
    };
    
    public string DisplayAccountLabel => TransactionType == TransactionType.T ? "From Account:" : "Account:";
    public string DisplayAccountType => Account.AccountType == AccountType.S ? "Savings" : "Checking";
}