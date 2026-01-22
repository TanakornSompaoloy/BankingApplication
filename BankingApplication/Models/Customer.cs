using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankingApplication.Models;

public enum State
{
    NSW = 1,
    VIC = 2,
    QLD = 3,
    WA = 4,
    SA = 5,
    TAS = 6,
    ACT = 7,
    NT = 8
}

public class Customer
{
    [DatabaseGenerated(DatabaseGeneratedOption.None), Range(0, 9999)]
    public int CustomerID { get; set; }

    [Required, StringLength(50)]
    public string Name { get; set; }

    [StringLength(50)]
    public string Address { get; set; }

    [StringLength(40)]
    public string City { get; set; }

    [StringLength(4), RegularExpression(@"^\d{4}$")]
    public string Postcode { get; set; }

    public State? State {  get; set; }

    [StringLength(11), RegularExpression(@"^\d{3} \d{3} \d{3}$")]
    public string TFN { get; set; }

    [StringLength(12), RegularExpression(@"^04\d{2} \d{3} \d{3}$")]
    public string Mobile { get; set; }

    public virtual List<Account> Accounts { get; set; }

    public virtual Login Login { get; set; }
}