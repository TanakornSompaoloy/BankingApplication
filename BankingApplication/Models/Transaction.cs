using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankingApplication.Models;

public enum TransactionType
{
    D = 1,
    W = 2,
    T = 3,
    S = 4,
    B = 5
}

public class Transaction
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TransactionID { get; set; }

    [Required, Display(Name = "Type")]
    public TransactionType TransactionType { get; set; }

    [Required, ForeignKey("Account")]
    public int AccountNumber { get; set; }
    public virtual Account Account { get; set; }

    [ForeignKey("DestinationAccount")]
    [Display(Name = "Destination")]
    public int? DestinationAccountNumber { get; set; }
    public virtual Account DestinationAccount { get; set; }

    [Required, Column(TypeName = "money")]
    public decimal Amount { get; set; }

    [StringLength(30)]
    public string Comment { get; set; }

    [Required, Display(Name = "Time")]
    public DateTime TransactionTimeUtc { get; set; }

}
