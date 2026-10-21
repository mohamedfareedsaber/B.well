using BWell.Loyalty.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BWell.Loyalty.Infrastructure.Data;

public class BWellDbContext : DbContext
{
    public BWellDbContext(DbContextOptions<BWellDbContext> options) : base(options)
    {
    }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<LoyaltyProgram> LoyaltyPrograms => Set<LoyaltyProgram>();
    public DbSet<ProgramBenefit> ProgramBenefits => Set<ProgramBenefit>();
    public DbSet<MedicalService> MedicalServices => Set<MedicalService>();
    public DbSet<ProgramServiceDiscount> ProgramServiceDiscounts => Set<ProgramServiceDiscount>();
    public DbSet<ChronicDisease> ChronicDiseases => Set<ChronicDisease>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientChronicDisease> PatientChronicDiseases => Set<PatientChronicDisease>();
    public DbSet<FamilyMember> FamilyMembers => Set<FamilyMember>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<DeliveryAddress> DeliveryAddresses => Set<DeliveryAddress>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Governorate> Governorates => Set<Governorate>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Email).HasMaxLength(256);
            e.Property(x => x.FullName).HasMaxLength(200);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<LoyaltyProgram>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.Description).HasMaxLength(1000);
            e.Property(x => x.IndividualPriceEgp).HasPrecision(18, 2);
            e.Property(x => x.FamilyPriceEgp).HasPrecision(18, 2);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ProgramBenefit>(e =>
        {
            e.Property(x => x.Text).HasMaxLength(500).IsRequired();
            e.HasOne(x => x.LoyaltyProgram).WithMany(x => x.Benefits).HasForeignKey(x => x.LoyaltyProgramId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<MedicalService>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ProgramServiceDiscount>(e =>
        {
            e.Property(x => x.FirstVisitDiscountPercent).HasPrecision(5, 2);
            e.Property(x => x.SecondVisitDiscountPercent).HasPrecision(5, 2);
            e.Property(x => x.AdditionalVisitDiscountPercent).HasPrecision(5, 2);
            e.HasOne(x => x.LoyaltyProgram).WithMany(x => x.ServiceDiscounts).HasForeignKey(x => x.LoyaltyProgramId);
            e.HasOne(x => x.MedicalService).WithMany(x => x.ProgramDiscounts).HasForeignKey(x => x.MedicalServiceId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<ChronicDisease>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(150).IsRequired();
            e.Property(x => x.Code).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Code).IsUnique();
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Patient>(e =>
        {
            e.Property(x => x.PatientCode).HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.PatientCode).IsUnique();
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.Property(x => x.MobileNumber).HasMaxLength(30).IsRequired();
            e.Property(x => x.Email).HasMaxLength(256);
            e.HasOne(x => x.AppUser).WithOne(x => x.PatientProfile).HasForeignKey<Patient>(x => x.AppUserId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<PatientChronicDisease>(e =>
        {
            e.HasKey(x => new { x.PatientId, x.ChronicDiseaseId });
            e.HasOne(x => x.Patient).WithMany(x => x.ChronicDiseases).HasForeignKey(x => x.PatientId);
            e.HasOne(x => x.ChronicDisease).WithMany(x => x.Patients).HasForeignKey(x => x.ChronicDiseaseId);
        });

        modelBuilder.Entity<FamilyMember>(e =>
        {
            e.Property(x => x.FullName).HasMaxLength(200).IsRequired();
            e.HasOne(x => x.Patient).WithMany(x => x.FamilyMembers).HasForeignKey(x => x.PatientId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Subscription>(e =>
        {
            e.Property(x => x.SubscriptionNumber).HasMaxLength(40).IsRequired();
            e.HasIndex(x => x.SubscriptionNumber).IsUnique();
            e.Property(x => x.AmountEgp).HasPrecision(18, 2);
            e.HasOne(x => x.Patient).WithMany(x => x.Subscriptions).HasForeignKey(x => x.PatientId);
            e.HasOne(x => x.LoyaltyProgram).WithMany(x => x.Subscriptions).HasForeignKey(x => x.LoyaltyProgramId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<DeliveryAddress>(e =>
        {
            e.Property(x => x.Country).HasMaxLength(100);
            e.Property(x => x.Governorate).HasMaxLength(100);
            e.Property(x => x.PostalCode).HasMaxLength(20);
            e.Property(x => x.AddressDetails).HasMaxLength(500);
            e.HasOne(x => x.Subscription).WithOne(x => x.DeliveryAddress).HasForeignKey<DeliveryAddress>(x => x.SubscriptionId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Payment>(e =>
        {
            e.Property(x => x.AmountEgp).HasPrecision(18, 2);
            e.Property(x => x.Currency).HasMaxLength(10);
            e.Property(x => x.InvoiceNumber).HasMaxLength(50);
            e.HasOne(x => x.Subscription).WithMany(x => x.Payments).HasForeignKey(x => x.SubscriptionId);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Visit>(e =>
        {
            e.Property(x => x.DiscountAppliedPercent).HasPrecision(5, 2);
            e.HasOne(x => x.Patient).WithMany(x => x.Visits).HasForeignKey(x => x.PatientId);
            e.HasOne(x => x.MedicalService).WithMany().HasForeignKey(x => x.MedicalServiceId);
            e.HasOne(x => x.Subscription).WithMany().HasForeignKey(x => x.SubscriptionId);
            e.HasOne(x => x.FamilyMember).WithMany().HasForeignKey(x => x.FamilyMemberId).OnDelete(DeleteBehavior.NoAction);
            e.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Governorate>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(100).IsRequired();
            e.Property(x => x.NameAr).HasMaxLength(100).IsRequired();
            e.HasQueryFilter(x => !x.IsDeleted);
        });
    }
}
