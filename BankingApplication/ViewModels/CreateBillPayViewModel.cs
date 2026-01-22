using BankingApplication.Models;
using System.ComponentModel.DataAnnotations;

namespace BankingApplication.ViewModels;

public class CreateBillPayViewModel
{
    public List<Account> Accounts { get; set; }
    public List<Payee> Payees { get; set; }

    [Required]
    [Display(Name = "Account")]
    public int AccountNumber { get; set; }

    [Required]
    [Display(Name = "Payee")]
    public int PayeeId { get; set; }

    [Required]
    [Display(Name = "Amount")]
    public decimal? Amount { get; set; }

    [Required]
    [Display(Name = "Schedule Date & Time")]
    [DataType(DataType.DateTime)]
    public DateTime ScheduleTime { get; set; } = DateTime.Now;

    [Required]
    public PeriodType Period { get; set; }
}