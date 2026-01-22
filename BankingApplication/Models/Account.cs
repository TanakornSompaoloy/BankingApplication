using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankingApplication.Models;

public enum AccountType
{
    C = 0, // Checking
    S = 1 // Savings
}

public class Account
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None), Length(4, 4)]
    [Display(Name = "Account Number")]
    public int AccountNumber    { get; set; }

    [Required, Display(Name = "Type")]
    public AccountType AccountType { get; set; }

    [Required]
    public int CustomerID { get; set; }
    public virtual Customer Customer { get; set; }

    [Required]
    [Column(TypeName = "money")]
    [DataType(DataType.Currency)]
    public decimal Balance { get; set; }

    [InverseProperty("Account")]
    public virtual List<Transaction> Transactions { get; set; }

    [InverseProperty("Account")]
    public virtual List<BillPay> BillPay { get; set; }
}
