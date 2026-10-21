using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BWell.Loyalty.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChronicDiseases",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChronicDiseases", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Governorates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    NameAr = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governorates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LoyaltyPrograms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Icon = table.Column<int>(type: "int", nullable: false),
                    ColorGradient = table.Column<int>(type: "int", nullable: false),
                    IndividualPriceEgp = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FamilyPriceEgp = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OfferExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    StripeProductId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StripeIndividualPriceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StripeFamilyPriceId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoyaltyPrograms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MedicalServices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalServices", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Role = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProgramBenefits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoyaltyProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppliesTo = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramBenefits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramBenefits_LoyaltyPrograms_LoyaltyProgramId",
                        column: x => x.LoyaltyProgramId,
                        principalTable: "LoyaltyPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProgramServiceDiscounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoyaltyProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstVisitDiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    SecondVisitDiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    AdditionalVisitDiscountPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProgramServiceDiscounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProgramServiceDiscounts_LoyaltyPrograms_LoyaltyProgramId",
                        column: x => x.LoyaltyProgramId,
                        principalTable: "LoyaltyPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProgramServiceDiscounts_MedicalServices_MedicalServiceId",
                        column: x => x.MedicalServiceId,
                        principalTable: "MedicalServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MobileNumber = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalIdOrPassport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MedicalNotes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StripeCustomerId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Patients_Users_AppUserId",
                        column: x => x.AppUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FamilyMembers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Age = table.Column<int>(type: "int", nullable: true),
                    Relationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NationalIdOrPassport = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FamilyMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FamilyMembers_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientChronicDiseases",
                columns: table => new
                {
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ChronicDiseaseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientChronicDiseases", x => new { x.PatientId, x.ChronicDiseaseId });
                    table.ForeignKey(
                        name: "FK_PatientChronicDiseases_ChronicDiseases_ChronicDiseaseId",
                        column: x => x.ChronicDiseaseId,
                        principalTable: "ChronicDiseases",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientChronicDiseases_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionNumber = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoyaltyProgramId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanType = table.Column<int>(type: "int", nullable: false),
                    BillingCycle = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AmountEgp = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextRenewalDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelledAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StripeSubscriptionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_LoyaltyPrograms_LoyaltyProgramId",
                        column: x => x.LoyaltyProgramId,
                        principalTable: "LoyaltyPrograms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DeliveryAddresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Governorate = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PostalCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    AddressDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeliveryAddresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeliveryAddresses_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AmountEgp = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    PaymentMethod = table.Column<int>(type: "int", nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StripeChargeId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PatientId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MedicalServiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    VisitDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VisitSequenceForService = table.Column<int>(type: "int", nullable: false),
                    DiscountAppliedPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visits_MedicalServices_MedicalServiceId",
                        column: x => x.MedicalServiceId,
                        principalTable: "MedicalServices",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Visits_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Visits_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChronicDiseases_Code",
                table: "ChronicDiseases",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeliveryAddresses_SubscriptionId",
                table: "DeliveryAddresses",
                column: "SubscriptionId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FamilyMembers_PatientId",
                table: "FamilyMembers",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientChronicDiseases_ChronicDiseaseId",
                table: "PatientChronicDiseases",
                column: "ChronicDiseaseId");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_AppUserId",
                table: "Patients",
                column: "AppUserId",
                unique: true,
                filter: "[AppUserId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_PatientCode",
                table: "Patients",
                column: "PatientCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId",
                table: "Payments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramBenefits_LoyaltyProgramId",
                table: "ProgramBenefits",
                column: "LoyaltyProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramServiceDiscounts_LoyaltyProgramId",
                table: "ProgramServiceDiscounts",
                column: "LoyaltyProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_ProgramServiceDiscounts_MedicalServiceId",
                table: "ProgramServiceDiscounts",
                column: "MedicalServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_LoyaltyProgramId",
                table: "Subscriptions",
                column: "LoyaltyProgramId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PatientId",
                table: "Subscriptions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriptionNumber",
                table: "Subscriptions",
                column: "SubscriptionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Visits_MedicalServiceId",
                table: "Visits",
                column: "MedicalServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_PatientId",
                table: "Visits",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_SubscriptionId",
                table: "Visits",
                column: "SubscriptionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeliveryAddresses");

            migrationBuilder.DropTable(
                name: "FamilyMembers");

            migrationBuilder.DropTable(
                name: "Governorates");

            migrationBuilder.DropTable(
                name: "PatientChronicDiseases");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "ProgramBenefits");

            migrationBuilder.DropTable(
                name: "ProgramServiceDiscounts");

            migrationBuilder.DropTable(
                name: "Visits");

            migrationBuilder.DropTable(
                name: "ChronicDiseases");

            migrationBuilder.DropTable(
                name: "MedicalServices");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "LoyaltyPrograms");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
