using BWell.Loyalty.Domain.Entities;
using BWell.Loyalty.Domain.Enums;
using BWell.Loyalty.Infrastructure.Data;
using BWell.Loyalty.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BWell.Loyalty.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<BWellDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");

        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync())
        {
            db.Users.Add(new AppUser
            {
                Email = "admin@bwell.local",
                FullName = "Admin User",
                Role = UserRole.Admin,
                PasswordHash = PasswordHasher.Hash("Admin@123"),
                IsActive = true
            });
        }

        if (!await db.ChronicDiseases.AnyAsync())
        {
            db.ChronicDiseases.AddRange(
                new ChronicDisease { Name = "Diabetes", Code = "diabetes" },
                new ChronicDisease { Name = "Hypertension (High Blood Pressure)", Code = "hypertension" },
                new ChronicDisease { Name = "Asthma", Code = "asthma" },
                new ChronicDisease { Name = "Heart Disease", Code = "heart_disease" },
                new ChronicDisease { Name = "Kidney Disease", Code = "kidney_disease" },
                new ChronicDisease { Name = "Thyroid Disorders", Code = "thyroid" },
                new ChronicDisease { Name = "Arthritis", Code = "arthritis" },
                new ChronicDisease { Name = "Cancer", Code = "cancer" },
                new ChronicDisease { Name = "COPD", Code = "copd" },
                new ChronicDisease { Name = "Other", Code = "other" });
        }

        if (!await db.MedicalServices.AnyAsync())
        {
            db.MedicalServices.AddRange(
                new MedicalService { Name = "Specialist Consultation", Description = "Doctor specialist visit" },
                new MedicalService { Name = "Lab Tests", Description = "Laboratory diagnostics" },
                new MedicalService { Name = "Health Checkup", Description = "Annual / bi-annual checkup" },
                new MedicalService { Name = "Medications", Description = "Pharmacy medications" },
                new MedicalService { Name = "Home Healthcare", Description = "Home care visits" },
                new MedicalService { Name = "Emergency Services", Description = "Priority emergency care" });
        }

        if (!await db.Governorates.AnyAsync())
        {
            db.Governorates.AddRange(AllGovernorates());
        }
        else
        {
            var existing = await db.Governorates.Select(g => g.Name).ToListAsync();
            var missing = AllGovernorates().Where(g => !existing.Contains(g.Name)).ToList();
            if (missing.Count > 0)
                db.Governorates.AddRange(missing);
        }

        await db.SaveChangesAsync();

        if (!await db.LoyaltyPrograms.AnyAsync())
        {
            var medicalServices = await db.MedicalServices.ToListAsync();
            MedicalService S(string name) => medicalServices.First(x => x.Name == name);

            var platinum = CreateProgram(
                "Platinum",
                "Premium care with unlimited access and exclusive privileges.",
                ProgramIcon.Crown,
                ColorGradient.Purple,
                2999m,
                5499m,
                1,
                new[]
                {
                    "Unlimited specialist consultations",
                    "50% discount on all lab tests",
                    "Free annual health checkup",
                    "Priority emergency services",
                    "Priority appointment booking",
                    "Extended visiting hours",
                    "Free home healthcare visits (4x/year)",
                    "30% discount on medications"
                },
                new[]
                {
                    "Unlimited specialist consultations per family member",
                    "50% discount on all lab tests for all",
                    "Free annual health checkup for each member",
                    "Priority emergency services for family",
                    "Coverage for up to 4 family members",
                    "Maternity care package included",
                    "Free home healthcare visits (4x/year per member)",
                    "30% discount on medications"
                });

            var gold = CreateProgram(
                "Gold",
                "Comprehensive coverage for you and your family.",
                ProgramIcon.Star,
                ColorGradient.Gold,
                1999m,
                3699m,
                2,
                new[]
                {
                    "15 specialist consultations/year",
                    "30% discount on lab tests",
                    "Free annual health checkup",
                    "Priority appointment booking",
                    "Extended visiting hours",
                    "Free home healthcare visits (2x/year)",
                    "20% discount on medications"
                },
                new[]
                {
                    "15 specialist consultations/year per family member",
                    "30% discount on lab tests for all",
                    "Free annual health checkup for each member",
                    "Priority appointment booking for family",
                    "Extended visiting hours",
                    "Free home healthcare visits (2x/year per member)",
                    "20% discount on medications",
                    "Coverage for up to 4 family members",
                    "Maternity care package included"
                });

            var silver = CreateProgram(
                "Silver",
                "Essential healthcare benefits for regular care.",
                ProgramIcon.Medal,
                ColorGradient.Silver,
                999m,
                1899m,
                3,
                new[]
                {
                    "8 specialist consultations/year",
                    "20% discount on lab tests",
                    "Bi-annual health checkup",
                    "Online appointment booking",
                    "Online medical records access",
                    "10% discount on medications"
                },
                new[]
                {
                    "8 specialist consultations/year per family member",
                    "20% discount on lab tests for all",
                    "Bi-annual health checkup for each member",
                    "Online appointment booking",
                    "Coverage for up to 4 family members",
                    "10% discount on medications"
                });

            var bronze = CreateProgram(
                "Bronze",
                "Basic membership with valuable healthcare savings.",
                ProgramIcon.Shield,
                ColorGradient.Bronze,
                499m,
                999m,
                4,
                new[]
                {
                    "4 specialist consultations/year",
                    "10% discount on lab tests",
                    "Annual health checkup",
                    "Online medical records access",
                    "5% discount on medications"
                },
                new[]
                {
                    "4 specialist consultations/year per family member",
                    "10% discount on lab tests for all",
                    "Annual health checkup for each member",
                    "Online medical records access",
                    "Coverage for up to 4 family members"
                });

            AddDiscount(platinum, S("Lab Tests"), 100, 50, 50);
            AddDiscount(platinum, S("Specialist Consultation"), 100, 100, 100);
            AddDiscount(gold, S("Lab Tests"), 100, 50, 30);
            AddDiscount(gold, S("Specialist Consultation"), 100, 50, 0);
            AddDiscount(silver, S("Lab Tests"), 100, 50, 20);
            AddDiscount(bronze, S("Lab Tests"), 100, 50, 10);

            db.LoyaltyPrograms.AddRange(platinum, gold, silver, bronze);
        }

        await db.SaveChangesAsync();

        if (!await db.Patients.AnyAsync())
        {
            var gold = await db.LoyaltyPrograms.FirstAsync(p => p.Name == "Gold");
            var bronze = await db.LoyaltyPrograms.FirstAsync(p => p.Name == "Bronze");
            var silver = await db.LoyaltyPrograms.FirstAsync(p => p.Name == "Silver");
            var diabetes = await db.ChronicDiseases.FirstAsync(d => d.Code == "diabetes");
            var hypertension = await db.ChronicDiseases.FirstAsync(d => d.Code == "hypertension");
            var asthma = await db.ChronicDiseases.FirstAsync(d => d.Code == "asthma");
            var lab = await db.MedicalServices.FirstAsync(s => s.Name == "Lab Tests");

            var p1 = new Patient
            {
                PatientCode = "P001",
                FullName = "Ahmed Mohamed Ali",
                MobileNumber = "+20 100 123 4567",
                Email = "ahmed@example.com",
                Age = 42,
                Nationality = "Egyptian",
                NationalIdOrPassport = "29001011234567"
            };
            p1.ChronicDiseases.Add(new PatientChronicDisease { ChronicDiseaseId = diabetes.Id });
            p1.ChronicDiseases.Add(new PatientChronicDisease { ChronicDiseaseId = hypertension.Id });

            var sub1 = new Subscription
            {
                SubscriptionNumber = "SUB-2024-00001",
                LoyaltyProgram = gold,
                PlanType = PlanType.Individual,
                BillingCycle = BillingCycle.Annual,
                Status = SubscriptionStatus.Active,
                AmountEgp = gold.IndividualPriceEgp,
                StartDateUtc = new DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                NextRenewalDateUtc = new DateTime(2025, 1, 15, 0, 0, 0, DateTimeKind.Utc),
                DeliveryAddress = new DeliveryAddress
                {
                    Country = "Egypt",
                    Governorate = "Cairo",
                    PostalCode = "11511",
                    AddressDetails = "12 Nile Street, Nasr City"
                }
            };
            p1.Subscriptions.Add(sub1);
            p1.Visits.Add(new Visit { MedicalService = lab, VisitDateUtc = DateTime.UtcNow.AddDays(-30), VisitSequenceForService = 1, DiscountAppliedPercent = 100 });
            p1.Visits.Add(new Visit { MedicalService = lab, VisitDateUtc = DateTime.UtcNow.AddDays(-10), VisitSequenceForService = 2, DiscountAppliedPercent = 50 });
            p1.Visits.Add(new Visit { MedicalService = lab, VisitDateUtc = DateTime.UtcNow.AddDays(-2), VisitSequenceForService = 3, DiscountAppliedPercent = 30 });

            var p2 = new Patient
            {
                PatientCode = "P002",
                FullName = "Sara Hassan",
                MobileNumber = "+20 101 222 3344",
                Email = "sara@example.com",
                Age = 35,
                Nationality = "Egyptian"
            };
            p2.ChronicDiseases.Add(new PatientChronicDisease { ChronicDiseaseId = asthma.Id });
            p2.Subscriptions.Add(new Subscription
            {
                SubscriptionNumber = "SUB-2024-00002",
                LoyaltyProgram = bronze,
                PlanType = PlanType.Individual,
                BillingCycle = BillingCycle.Annual,
                Status = SubscriptionStatus.Active,
                AmountEgp = bronze.IndividualPriceEgp,
                StartDateUtc = DateTime.UtcNow.AddMonths(-3),
                NextRenewalDateUtc = DateTime.UtcNow.AddMonths(9),
                DeliveryAddress = new DeliveryAddress
                {
                    Country = "Egypt",
                    Governorate = "Alexandria",
                    PostalCode = "21500",
                    AddressDetails = "5 Corniche Road"
                }
            });
            p2.Visits.Add(new Visit { VisitDateUtc = DateTime.UtcNow.AddDays(-5), VisitSequenceForService = 1 });

            var p3 = new Patient
            {
                PatientCode = "P003",
                FullName = "Omar Khaled",
                MobileNumber = "+20 102 555 7788",
                Email = "omar@example.com",
                Age = 50
            };
            p3.Subscriptions.Add(new Subscription
            {
                SubscriptionNumber = "SUB-2024-00003",
                LoyaltyProgram = silver,
                PlanType = PlanType.Family,
                BillingCycle = BillingCycle.Annual,
                Status = SubscriptionStatus.Active,
                AmountEgp = silver.FamilyPriceEgp,
                StartDateUtc = DateTime.UtcNow.AddMonths(-1),
                NextRenewalDateUtc = DateTime.UtcNow.AddMonths(11),
                DeliveryAddress = new DeliveryAddress
                {
                    Country = "Egypt",
                    Governorate = "Giza",
                    PostalCode = "12511",
                    AddressDetails = "8 Pyramids Street"
                }
            });
            p3.FamilyMembers.Add(new FamilyMember { FullName = "Mona Khaled", Age = 45, Relationship = "Spouse" });
            p3.FamilyMembers.Add(new FamilyMember { FullName = "Youssef Khaled", Age = 12, Relationship = "Son" });
            p3.Visits.Add(new Visit { VisitDateUtc = DateTime.UtcNow.AddDays(-20), VisitSequenceForService = 1 });
            p3.Visits.Add(new Visit { VisitDateUtc = DateTime.UtcNow.AddDays(-7), VisitSequenceForService = 2 });

            var p4 = new Patient
            {
                PatientCode = "P004",
                FullName = "Nour Ibrahim",
                MobileNumber = "+20 111 999 0011",
                Email = "nour@example.com",
                Age = 28
            };
            p4.Subscriptions.Add(new Subscription
            {
                SubscriptionNumber = "SUB-2024-00004",
                LoyaltyProgram = gold,
                PlanType = PlanType.Individual,
                BillingCycle = BillingCycle.Annual,
                Status = SubscriptionStatus.Active,
                AmountEgp = gold.IndividualPriceEgp,
                StartDateUtc = DateTime.UtcNow.AddDays(-40),
                NextRenewalDateUtc = DateTime.UtcNow.AddDays(325),
                DeliveryAddress = new DeliveryAddress
                {
                    Country = "Egypt",
                    Governorate = "Mansoura",
                    PostalCode = "35511",
                    AddressDetails = "22 University Street"
                }
            });

            db.Patients.AddRange(p1, p2, p3, p4);
            await db.SaveChangesAsync();

            // Family member visits for Omar (Family plan demo)
            var omar = await db.Patients.Include(p => p.FamilyMembers).FirstAsync(p => p.PatientCode == "P003");
            var spouse = omar.FamilyMembers.First(m => m.Relationship == "Spouse");
            var son = omar.FamilyMembers.First(m => m.Relationship == "Son");
            var service = await db.MedicalServices.OrderBy(s => s.Name).FirstAsync();
            db.Visits.AddRange(
                new Visit
                {
                    PatientId = omar.Id,
                    FamilyMemberId = spouse.Id,
                    MedicalServiceId = service.Id,
                    VisitDateUtc = DateTime.UtcNow.AddDays(-14),
                    VisitSequenceForService = 1,
                    DiscountAppliedPercent = 20,
                    Notes = "Spouse checkup"
                },
                new Visit
                {
                    PatientId = omar.Id,
                    FamilyMemberId = son.Id,
                    MedicalServiceId = service.Id,
                    VisitDateUtc = DateTime.UtcNow.AddDays(-4),
                    VisitSequenceForService = 1,
                    DiscountAppliedPercent = 15,
                    Notes = "Son pediatric visit"
                });
            await db.SaveChangesAsync();
        }

        // Ensure every patient has a login account (email + Patient@123)
        var patientsNeedingUsers = await db.Patients
            .Where(p => p.AppUserId == null)
            .ToListAsync();
        foreach (var patient in patientsNeedingUsers)
        {
            var email = string.IsNullOrWhiteSpace(patient.Email)
                ? $"{patient.PatientCode.ToLowerInvariant()}@bwell.local"
                : patient.Email.Trim().ToLowerInvariant();

            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user is null)
            {
                user = new AppUser
                {
                    Email = email,
                    FullName = patient.FullName,
                    Role = UserRole.Patient,
                    PasswordHash = PasswordHasher.Hash("Patient@123"),
                    IsActive = true
                };
                db.Users.Add(user);
                await db.SaveChangesAsync();
            }

            patient.AppUserId = user.Id;
            patient.Email ??= email;
        }

        if (patientsNeedingUsers.Count > 0)
            await db.SaveChangesAsync();

        logger.LogInformation("B.Well database seeded successfully.");
    }

    private static LoyaltyProgram CreateProgram(
        string name,
        string description,
        ProgramIcon icon,
        ColorGradient color,
        decimal individual,
        decimal family,
        int sort,
        string[] individualBenefits,
        string[] familyBenefits)
    {
        var program = new LoyaltyProgram
        {
            Name = name,
            Description = description,
            Icon = icon,
            ColorGradient = color,
            IndividualPriceEgp = individual,
            FamilyPriceEgp = family,
            SortOrder = sort,
            IsActive = true
        };

        for (var i = 0; i < individualBenefits.Length; i++)
        {
            program.Benefits.Add(new ProgramBenefit
            {
                AppliesTo = PlanType.Individual,
                Text = individualBenefits[i],
                SortOrder = i
            });
        }

        for (var i = 0; i < familyBenefits.Length; i++)
        {
            program.Benefits.Add(new ProgramBenefit
            {
                AppliesTo = PlanType.Family,
                Text = familyBenefits[i],
                SortOrder = i
            });
        }

        return program;
    }

    private static void AddDiscount(LoyaltyProgram program, MedicalService service, decimal first, decimal second, decimal additional)
    {
        program.ServiceDiscounts.Add(new ProgramServiceDiscount
        {
            MedicalService = service,
            FirstVisitDiscountPercent = first,
            SecondVisitDiscountPercent = second,
            AdditionalVisitDiscountPercent = additional
        });
    }

    private static Governorate[] AllGovernorates() =>
    [
        new() { Name = "Cairo", NameAr = "القاهرة", Latitude = 30.0444, Longitude = 31.2357 },
        new() { Name = "Giza", NameAr = "الجيزة", Latitude = 30.0131, Longitude = 31.2089 },
        new() { Name = "Alexandria", NameAr = "الإسكندرية", Latitude = 31.2001, Longitude = 29.9187 },
        new() { Name = "Dakahlia", NameAr = "الدقهلية", Latitude = 31.0409, Longitude = 31.3785 },
        new() { Name = "Mansoura", NameAr = "المنصورة", Latitude = 31.0409, Longitude = 31.3785 },
        new() { Name = "Sharqia", NameAr = "الشرقية", Latitude = 30.7327, Longitude = 31.7195 },
        new() { Name = "Qalyubia", NameAr = "القليوبية", Latitude = 30.3292, Longitude = 31.2175 },
        new() { Name = "Kafr El Sheikh", NameAr = "كفر الشيخ", Latitude = 31.1107, Longitude = 30.9388 },
        new() { Name = "Gharbia", NameAr = "الغربية", Latitude = 30.8754, Longitude = 31.0335 },
        new() { Name = "Tanta", NameAr = "طنطا", Latitude = 30.7865, Longitude = 31.0004 },
        new() { Name = "Monufia", NameAr = "المنوفية", Latitude = 30.5971, Longitude = 30.9876 },
        new() { Name = "Beheira", NameAr = "البحيرة", Latitude = 30.8481, Longitude = 30.3436 },
        new() { Name = "Port Said", NameAr = "بورسعيد", Latitude = 31.2653, Longitude = 32.3019 },
        new() { Name = "Suez", NameAr = "السويس", Latitude = 29.9668, Longitude = 32.5498 },
        new() { Name = "Ismailia", NameAr = "الإسماعيلية", Latitude = 30.5965, Longitude = 32.2715 },
        new() { Name = "Damietta", NameAr = "دمياط", Latitude = 31.4175, Longitude = 31.8144 },
        new() { Name = "Fayoum", NameAr = "الفيوم", Latitude = 29.3084, Longitude = 30.8428 },
        new() { Name = "Beni Suef", NameAr = "بني سويف", Latitude = 29.0661, Longitude = 31.0994 },
        new() { Name = "Minya", NameAr = "المنيا", Latitude = 28.1099, Longitude = 30.7503 },
        new() { Name = "Asyut", NameAr = "أسيوط", Latitude = 27.1809, Longitude = 31.1837 },
        new() { Name = "Sohag", NameAr = "سوهاج", Latitude = 26.5569, Longitude = 31.6948 },
        new() { Name = "Qena", NameAr = "قنا", Latitude = 26.1551, Longitude = 32.7160 },
        new() { Name = "Luxor", NameAr = "الأقصر", Latitude = 25.6872, Longitude = 32.6396 },
        new() { Name = "Aswan", NameAr = "أسوان", Latitude = 24.0889, Longitude = 32.8998 },
        new() { Name = "Red Sea", NameAr = "البحر الأحمر", Latitude = 25.3407, Longitude = 34.0768 },
        new() { Name = "New Valley", NameAr = "الوادي الجديد", Latitude = 25.4390, Longitude = 30.5580 },
        new() { Name = "Matrouh", NameAr = "مطروح", Latitude = 31.3543, Longitude = 27.2373 },
        new() { Name = "North Sinai", NameAr = "شمال سيناء", Latitude = 30.2824, Longitude = 33.6176 },
        new() { Name = "South Sinai", NameAr = "جنوب سيناء", Latitude = 28.2368, Longitude = 33.6254 },
    ];
}
