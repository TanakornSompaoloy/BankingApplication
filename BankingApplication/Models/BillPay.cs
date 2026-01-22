using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankingApplication.Models;

public enum PeriodType
{
    O = 1, // One-off
    M = 2 // Monthly
}

public enum BillPayStatus
{
    P = 1, // Pending
    S = 2, // Succeeded
    F = 3, // Failed
    C = 4  // Cancelled
}

public class BillPay
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int BillPayID { get; set; }

    [Required, ForeignKey(nameof(Account))]
    public int AccountNumber { get; set; }
    public virtual Account Account { get; set; }

    [Required]
    public int PayeeID { get; set; }
    public virtual Payee Payee { get; set; }

    [Required, Column(TypeName = "money")]
    public decimal Amount { get; set; }

    [Required]
    public DateTime ScheduleTimeUtc { get; set; }

    [Required, Column(TypeName = "char")]
    public PeriodType Period { get; set; }

    [Required, Column(TypeName = "char")]
    public BillPayStatus Status { get; set; } = BillPayStatus.P;

    [Required]
    public bool IsBlocked { get; set; } = false;

}
