using Microsoft.EntityFrameworkCore;
using BankingApplication.Models;

namespace BankingApplication.Data;

public class McbaContext : DbContext
{
    public McbaContext(DbContextOptions<McbaContext> options) : base(options)
    { }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Login> Logins { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Transaction> Transactions { get; set; }
    public DbSet<BillPay> BillPays { get; set; }
    public DbSet<Payee> Payees { get; set; }

    // Fluent-API.
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Set check constraints (cannot be expressed with data annotations).
        // Skip for SQLite (incompatible with some SQL Server functions and syntax)
        var isSqlite = Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";
        if (!isSqlite)
        {
            builder.Entity<Login>().ToTable(b =>
            {
                b.HasCheckConstraint("CH_Login_LoginID", "len(LoginID) = 8");
                b.HasCheckConstraint("CH_Login_PasswordHash", "len(PasswordHash) = 94");
            });
            builder.Entity<Account>().ToTable(b => b.HasCheckConstraint(
                    "CH_Account_Balance",
                    $"AccountType = {(int)AccountType.S} and Balance >= 0 or " +
                    $"AccountType = {(int)AccountType.C} and Balance >= -500"));
            builder.Entity<Transaction>().ToTable(b => b.HasCheckConstraint("CH_Transaction_Amount", "Amount > 0"));
            builder.Entity<BillPay>().ToTable(b => b.HasCheckConstraint("CH_BillPay_Amount", "Amount > 0"));

            // Bonus Material: Create constraint for enum values to restrict valid values in the database.
            builder.Entity<Account>().ToTable(b =>
            {
                var validValues = string.Join(',', Enum.GetValues<AccountType>().Cast<int>());
                b.HasCheckConstraint("CH_Account_AccountType", $"AccountType in ({validValues})");
            });
            builder.Entity<Transaction>().ToTable(b =>
            {
                var validValues = string.Join(',', Enum.GetValues<TransactionType>().Cast<int>());
                b.HasCheckConstraint("CH_Transaction_TransactionType", $"TransactionType in ({validValues})");
            });

            // Check constraint if State is correct.
            builder.Entity<Customer>().ToTable(b =>
            {
                var validValues = string.Join(',', Enum.GetValues<State>().Cast<int>());
                b.HasCheckConstraint("CH_Customer_State", $"State in ({validValues})");
            });
            builder.Entity<BillPay>().ToTable(b =>
            {
                var validValues = string.Join("','", Enum.GetNames<PeriodType>());
                b.HasCheckConstraint("CH_BillPay_Period", $"Period in ('{validValues}')");

                var statusValues = string.Join("','", Enum.GetNames<BillPayStatus>());
                b.HasCheckConstraint("CH_BillPay_Status", $"Status in ('{statusValues}')");
            });
        }

        // Configure ambiguous Account.Transactions navigation property relationship.
        builder.Entity<Transaction>().
            HasOne(x => x.Account).WithMany(x => x.Transactions).HasForeignKey(x => x.AccountNumber);
    }
}
