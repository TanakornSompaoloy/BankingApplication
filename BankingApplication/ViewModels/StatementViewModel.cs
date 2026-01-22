using BankingApplication.Models;
using X.PagedList;

namespace BankingApplication.ViewModels;
public class StatementViewModel
{
    private const decimal OverdraftLimit = 500;

    public List<Account> Accounts { get; set; }
    public int? SelectedAccountNumber { get; set; }
    public Account SelectedAccount => SelectedAccountNumber.HasValue? Accounts?.FirstOrDefault(x => x.AccountNumber == SelectedAccountNumber): null;
    public IPagedList<Transaction> PagedTransactions { get; set; }

    public decimal AvailableBalance
    {
        get
        {
            if (SelectedAccount == null)
                return 0;

            return SelectedAccount.AccountType == AccountType.C
                ? SelectedAccount.Balance + OverdraftLimit
                : SelectedAccount.Balance;
        }
    }
} 