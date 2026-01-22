using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BankingApplication.Models;

public class Login
{
    [Column(TypeName = "char")]
    [Required, StringLength(8)]
    public string LoginID  { get; set; }

    [Column(TypeName = "char")]
    [Required, StringLength(94)]
    public string PasswordHash  { get; set; }

    public int CustomerID { get; set; }
    public virtual Customer Customer { get; set; }
}
