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
        if (context.Users.Any()) return;

        _ = context.Users.Add(new User
        {
            FullName = "System Admin",
            Email = "admin@lims.com",
            PasswordHash = passwordHasher.Hash("Admin@123"),
            Role = UserRole.Admin,
            PhoneNumber = "9999999999",
            IsActive = true
        });

        context.SaveChanges();
        Console.WriteLine("✅ Admin seeded: admin@lims.com / Admin@123");
    }

    private static void SeedPlans(AppDbContext context)
    {
        if (context.InsurancePlans.Any()) return;

        var plans = new List<InsurancePlan>
        {
            new InsurancePlan
            {
                PlanName = "SecureLife Basic",
                Description = "Affordable life cover for young earners starting their journey.",
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
                Description = "Enhanced coverage with higher sum assured for growing families.",
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
                Description = "Maximum protection for high-income individuals and families.",
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
            }
        };

        context.InsurancePlans.AddRange(plans);
        context.SaveChanges();
        Console.WriteLine("✅ 3 Insurance Plans seeded.");
    }
}