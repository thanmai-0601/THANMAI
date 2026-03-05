using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<InsurancePlan> InsurancePlans => Set<InsurancePlan>();
    public DbSet<Policy> Policies => Set<Policy>();
    public DbSet<Nominee> Nominees => Set<Nominee>();
    public DbSet<Document> Documents => Set<Document>();
    public DbSet<Commission> Commissions => Set<Commission>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Claim> Claims => Set<Claim>();
    public DbSet<ClaimDocument> ClaimDocuments => Set<ClaimDocument>();
    public DbSet<PolicyEndorsement> PolicyEndorsements => Set<PolicyEndorsement>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ────────────────────────────────────────────────────
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired().HasMaxLength(255);
            entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired();
            entity.Property(u => u.Role).HasConversion<int>();
        });

        // ── InsurancePlan ───────────────────────────────────────────
        modelBuilder.Entity<InsurancePlan>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.PlanName).IsRequired().HasMaxLength(150);
            entity.Property(p => p.Description).HasMaxLength(1000);
            entity.Property(p => p.MinSumAssured).HasColumnType("decimal(18,2)");
            entity.Property(p => p.MaxSumAssured).HasColumnType("decimal(18,2)");
            entity.Property(p => p.MinAnnualIncome).HasColumnType("decimal(18,2)");
            entity.Property(p => p.BaseRatePer1000).HasColumnType("decimal(18,4)");
            entity.Property(p => p.LowRiskMultiplier).HasColumnType("decimal(18,4)");
            entity.Property(p => p.MediumRiskMultiplier).HasColumnType("decimal(18,4)");
            entity.Property(p => p.HighRiskMultiplier).HasColumnType("decimal(18,4)");
            entity.Property(p => p.CommissionPercentage).HasColumnType("decimal(18,2)");
            entity.Property(p => p.TenureOptions).IsRequired().HasMaxLength(100);
        });

        // ── Policy ──────────────────────────────────────────────────
        modelBuilder.Entity<Policy>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.HasIndex(p => p.PolicyNumber).IsUnique();
            entity.Property(p => p.PolicyNumber).IsRequired().HasMaxLength(20);
            entity.Property(p => p.Status).HasConversion<int>();
            entity.Property(p => p.SumAssured).HasColumnType("decimal(18,2)");
            entity.Property(p => p.PremiumAmount).HasColumnType("decimal(18,2)");
            entity.Property(p => p.AnnualIncome).HasColumnType("decimal(18,2)");
            entity.Property(p => p.CustomerAddress).HasMaxLength(500);

            entity.HasOne(p => p.Customer)
                  .WithMany(u => u.CustomerPolicies)
                  .HasForeignKey(p => p.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Agent)
                  .WithMany(u => u.AgentPolicies)
                  .HasForeignKey(p => p.AgentId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.InsurancePlan)
                  .WithMany(ip => ip.Policies)
                  .HasForeignKey(p => p.InsurancePlanId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Nominee ─────────────────────────────────────────────────
        modelBuilder.Entity<Nominee>(entity =>
        {
            entity.HasKey(n => n.Id);
            entity.Property(n => n.AllocationPercentage).HasColumnType("decimal(5,2)");
            entity.HasOne(n => n.Policy)
                  .WithMany(p => p.Nominees)
                  .HasForeignKey(n => n.PolicyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Document ────────────────────────────────────────────────
        modelBuilder.Entity<Document>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Status).HasConversion<int>();
            entity.HasOne(d => d.Policy)
                  .WithMany(p => p.Documents)
                  .HasForeignKey(d => d.PolicyId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Commission ──────────────────────────────────────────────
        modelBuilder.Entity<Commission>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.PremiumAmount).HasColumnType("decimal(18,2)");
            entity.Property(c => c.CommissionPercentage).HasColumnType("decimal(18,2)");
            entity.Property(c => c.CommissionAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(c => c.Policy)
                  .WithMany()
                  .HasForeignKey(c => c.PolicyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Agent)
                  .WithMany()
                  .HasForeignKey(c => c.AgentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Invoice ─────────────────────────────────────────────────
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(i => i.Id);
            entity.HasIndex(i => i.InvoiceNumber).IsUnique();
            entity.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(20);
            entity.Property(i => i.AmountDue).HasColumnType("decimal(18,2)");
            entity.Property(i => i.Frequency).HasConversion<int>();
            entity.Property(i => i.Status).HasConversion<int>();


            entity.HasOne(i => i.Policy)
                  .WithMany(p => p.Invoices)
                  .HasForeignKey(i => i.PolicyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Payment ─────────────────────────────────────────────────
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.AmountPaid).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Status).HasConversion<int>();

            // One invoice has one payment
            entity.HasOne(p => p.Invoice)
                  .WithOne(i => i.Payment)
                  .HasForeignKey<Payment>(p => p.InvoiceId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Policy)
                  .WithMany()
                  .HasForeignKey(p => p.PolicyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Claim ────────────────────────────────────────────────────
        modelBuilder.Entity<Claim>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.HasIndex(c => c.ClaimNumber).IsUnique();
            entity.Property(c => c.ClaimNumber).IsRequired().HasMaxLength(20);
            entity.Property(c => c.Status).HasConversion<int>();
            entity.Property(c => c.ClaimAmount).HasColumnType("decimal(18,2)");
            entity.Property(c => c.SettledAmount).HasColumnType("decimal(18,2)");

            entity.HasOne(c => c.Policy)
                  .WithMany(p => p.Claims)
                  .HasForeignKey(c => c.PolicyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.Customer)
                  .WithMany()
                  .HasForeignKey(c => c.CustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(c => c.ClaimsOfficer)
                  .WithMany()
                  .HasForeignKey(c => c.ClaimsOfficerId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── ClaimDocument ────────────────────────────────────────────
        modelBuilder.Entity<ClaimDocument>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Status).HasConversion<int>();

            entity.HasOne(d => d.Claim)
                  .WithMany(c => c.ClaimDocuments)
                  .HasForeignKey(d => d.ClaimId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
        // ── PolicyEndorsement ────────────────────────────────────────
        modelBuilder.Entity<PolicyEndorsement>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Type).HasConversion<int>();
            entity.Property(e => e.Status).HasConversion<int>();
            entity.Property(e => e.ChangeRequestJson).IsRequired();
            entity.Property(e => e.OldValueJson).IsRequired();

            entity.HasOne(e => e.Policy)
                  .WithMany(p => p.Endorsements)
                  .HasForeignKey(e => e.PolicyId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RequestedByCustomer)
                  .WithMany()
                  .HasForeignKey(e => e.RequestedByCustomerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.ReviewedByAgent)
                  .WithMany()
                  .HasForeignKey(e => e.ReviewedByAgentId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}