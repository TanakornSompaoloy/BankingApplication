using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankingApplication.Models;

public class Payee
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PayeeId { get; set; }
    [Required, StringLength(50)]
    public string Name { get; set; }

    [Required, StringLength(50)]
    public string Address { get; set; }

    [Required, StringLength(40)]
    public string City { get; set; }

    [Required, StringLength(4)]
    public string PostCode { get; set; }

    [Required]
    public State State { get; set; }

    [Required, StringLength(14), RegularExpression(@"^\(0\d\) \d{4} \d{4}$")]
    public string Phone { get; set; }
}
