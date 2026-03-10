using Application.Interfaces.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;

namespace Infrastructure.Data;

public static class DbInitializer
{
    public static void Seed(AppDbContext context,IPasswordHasher passwordHasher)
    {
        SeedAdmin(context,passwordHasher);
        SeedPlans(context);
    }

    private static void SeedAdmin(AppDbContext context,IPasswordHasher passwordHasher)
    {
        if (context.Users.Any(u => u.Role == UserRole.Admin)) return;

        _ = context.Users.Add(new User
        {
            FullName = "System Admin",
            Email = "admin@lims.com",
            PasswordHash = passwordHasher.Hash("Admin@123"),
            Role = UserRole.Admin,
            PhoneNumber = "9999999999",
            IsActive = true,
            DateOfBirth = new DateTime(1985, 5, 20),
            BankAccountName = "NexaLife Admin",
            BankAccountNumber = "000012345678",
            BankIfscCode = "NEXT0000001"
        });

        context.SaveChanges();
        Console.WriteLine("✅ Admin seeded: admin@lims.com / Admin@123");
    }

    private static void SeedPlans(AppDbContext context)
    {
        if (context.InsurancePlans.Any()) return;

        var plans = new List<InsurancePlan>
        {
            // ══════════════════════════════════════════════════
            //  TERM LIFE PLANS — Pure death benefit, no maturity
            // ══════════════════════════════════════════════════
            new InsurancePlan
            {
                PlanName = "SecureLife Basic",
                Description = "Affordable term life cover for young earners starting their journey.",
                PlanType = PlanType.TermLife,
                MinSumAssured = 500000,
                MaxSumAssured = 2500000,
                TenureOptions = "10,15,20",
                MinEntryAge = 18,
                MaxEntryAge = 45,
                MinAnnualIncome = 200000,

                BaseRatePer1000 = 1.2m,
                LowRiskMultiplier = 1.0m,
                MediumRiskMultiplier = 1.3m,
                HighRiskMultiplier = 1.7m,
                CommissionPercentage = 5,
                IsActive = true
            },
            new InsurancePlan
            {
                PlanName = "SecureLife Plus",
                Description = "Enhanced term life coverage with higher sum assured for growing families.",
                PlanType = PlanType.TermLife,
                MinSumAssured = 2500000,
                MaxSumAssured = 10000000,
                TenureOptions = "15,20,25,30",
                MinEntryAge = 21,
                MaxEntryAge = 50,
                MinAnnualIncome = 500000,

                BaseRatePer1000 = 1.5m,
                LowRiskMultiplier = 1.0m,
                MediumRiskMultiplier = 1.25m,
                HighRiskMultiplier = 1.6m,
                CommissionPercentage = 8,
                IsActive = true
            },
            new InsurancePlan
            {
                PlanName = "SecureLife Premium",
                Description = "Maximum term life protection for high-income individuals and families.",
                PlanType = PlanType.TermLife,
                MinSumAssured = 10000000,
                MaxSumAssured = 50000000,
                TenureOptions = "20,25,30",
                MinEntryAge = 25,
                MaxEntryAge = 55,
                MinAnnualIncome = 1500000,

                BaseRatePer1000 = 1.8m,
                LowRiskMultiplier = 1.0m,
                MediumRiskMultiplier = 1.2m,
                HighRiskMultiplier = 1.5m,
                CommissionPercentage = 10,
                IsActive = true
            },

            // ══════════════════════════════════════════════════
            //  WHOLE LIFE PLANS — Coverage until age 99
            // ══════════════════════════════════════════════════
            new InsurancePlan
            {
                PlanName = "WholeLife Shield",
                Description = "Lifelong protection until age 99 with guaranteed death benefit for your loved ones.",
                PlanType = PlanType.WholeLife,
                CoverageToAge = 99,
                MinSumAssured = 1000000,
                MaxSumAssured = 5000000,
                TenureOptions = "20,25,30,35",
                MinEntryAge = 20,
                MaxEntryAge = 50,
                MinAnnualIncome = 300000,

                BaseRatePer1000 = 3.5m,
                LowRiskMultiplier = 1.0m,
                MediumRiskMultiplier = 1.3m,
                HighRiskMultiplier = 1.7m,
                CommissionPercentage = 8,
                IsActive = true
            },
            new InsurancePlan
            {
                PlanName = "WholeLife Elite",
                Description = "Premium whole life coverage until age 99 with high sum assured for complete family security.",
                PlanType = PlanType.WholeLife,
                CoverageToAge = 99,
                MinSumAssured = 5000000,
                MaxSumAssured = 20000000,
                TenureOptions = "25,30,35,40",
                MinEntryAge = 25,
                MaxEntryAge = 45,
                MinAnnualIncome = 1000000,

                BaseRatePer1000 = 4.5m,
                LowRiskMultiplier = 1.0m,
                MediumRiskMultiplier = 1.25m,
                HighRiskMultiplier = 1.6m,
                CommissionPercentage = 10,
                IsActive = true
            },

            // ══════════════════════════════════════════════════
            //  ENDOWMENT PLANS — Death benefit + maturity payout
            // ══════════════════════════════════════════════════
            new InsurancePlan
            {
                PlanName = "SecureLife Endowment",
                Description = "Savings-cum-protection plan with guaranteed maturity benefit. If you survive the term, receive sum assured plus accumulated bonuses.",
                PlanType = PlanType.Endowment,
                BonusRatePerYear = 2.5m,
                MinSumAssured = 500000,
                MaxSumAssured = 2500000,
                TenureOptions = "15,20,25",
                MinEntryAge = 18,
                MaxEntryAge = 50,
                MinAnnualIncome = 250000,

                BaseRatePer1000 = 5.5m,
                LowRiskMultiplier = 1.0m,
                MediumRiskMultiplier = 1.2m,
                HighRiskMultiplier = 1.5m,
                CommissionPercentage = 7,
                IsActive = true
            },
            new InsurancePlan
            {
                PlanName = "SecureLife Endowment Plus",
                Description = "High-value endowment plan with enhanced bonus rate. Ideal for long-term wealth creation alongside life protection.",
                PlanType = PlanType.Endowment,
                BonusRatePerYear = 3.0m,
                MinSumAssured = 2500000,
                MaxSumAssured = 10000000,
                TenureOptions = "20,25,30",
                MinEntryAge = 21,
                MaxEntryAge = 50,
                MinAnnualIncome = 600000,

                BaseRatePer1000 = 7.0m,
                LowRiskMultiplier = 1.0m,
                MediumRiskMultiplier = 1.2m,
                HighRiskMultiplier = 1.5m,
                CommissionPercentage = 9,
                IsActive = true
            }
        };

        context.InsurancePlans.AddRange(plans);
        context.SaveChanges();
        Console.WriteLine("✅ 7 Insurance Plans seeded (3 Term Life, 2 Whole Life, 2 Endowment).");
    }
}