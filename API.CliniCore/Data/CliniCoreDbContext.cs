using API.CliniCore.Data.Entities;
using API.CliniCore.Data.Entities.Auth;
using API.CliniCore.Data.Entities.Clinical;
using API.CliniCore.Data.Entities.ClinicalEntries;
using API.CliniCore.Data.Entities.User;
using Microsoft.EntityFrameworkCore;

namespace API.CliniCore.Data
{
    /// <summary>
    /// Entity Framework Core database context for CliniCore.
    /// Uses SQLite for portable data persistence.
    /// </summary>
    public class CliniCoreDbContext : DbContext
    {
        public CliniCoreDbContext(DbContextOptions<CliniCoreDbContext> options)
            : base(options)
        {
        }

        public DbSet<PatientEntity> Patients => Set<PatientEntity>();
        public DbSet<PhysicianEntity> Physicians => Set<PhysicianEntity>();
        public DbSet<AdministratorEntity> Administrators => Set<AdministratorEntity>();
        public DbSet<AppointmentEntity> Appointments => Set<AppointmentEntity>();
        public DbSet<ClinicalDocumentEntity> ClinicalDocuments => Set<ClinicalDocumentEntity>();
        public DbSet<UserCredentialEntity> UserCredentials => Set<UserCredentialEntity>();

        // Clinical Entry DbSets
        public DbSet<ObservationEntity> Observations => Set<ObservationEntity>();
        public DbSet<AssessmentEntity> Assessments => Set<AssessmentEntity>();
        public DbSet<DiagnosisEntity> Diagnoses => Set<DiagnosisEntity>();
        public DbSet<PlanEntity> Plans => Set<PlanEntity>();
        public DbSet<PrescriptionEntity> Prescriptions => Set<PrescriptionEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Patient configuration
            modelBuilder.Entity<PatientEntity>(entity =>
            {
                entity.ToTable("Patients");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Gender).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Race).HasMaxLength(50);
                entity.Property(e => e.Address).HasMaxLength(200);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Physician configuration
            modelBuilder.Entity<PhysicianEntity>(entity =>
            {
                entity.ToTable("Physicians");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LicenseNumber).IsRequired().HasMaxLength(50);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Administrator configuration
            modelBuilder.Entity<AdministratorEntity>(entity =>
            {
                entity.ToTable("Administrators");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Appointment configuration
            modelBuilder.Entity<AppointmentEntity>(entity =>
            {
                entity.ToTable("Appointments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Status).IsRequired().HasMaxLength(20);
                entity.Property(e => e.AppointmentType).HasMaxLength(50);
                entity.Property(e => e.ReasonForVisit).HasMaxLength(500);
                entity.Property(e => e.Notes).HasMaxLength(2000);
                entity.Property(e => e.CancellationReason).HasMaxLength(500);
                entity.HasIndex(e => e.PatientId);
                entity.HasIndex(e => e.PhysicianId);
                entity.HasIndex(e => e.Start);
            });

            // Clinical Document configuration
            modelBuilder.Entity<ClinicalDocumentEntity>(entity =>
            {
                entity.ToTable("ClinicalDocuments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ChiefComplaint).HasMaxLength(500);
                entity.HasIndex(e => e.PatientId);
                entity.HasIndex(e => e.PhysicianId);
                entity.HasIndex(e => e.AppointmentId);
            });

            // User Credentials configuration
            // Note: Id is the same as the associated profile's Id (1:1 shared identity)
            modelBuilder.Entity<UserCredentialEntity>(entity =>
            {
                entity.ToTable("UserCredentials");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
                entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Role).IsRequired().HasMaxLength(20);
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // Observation configuration
            modelBuilder.Entity<ObservationEntity>(entity =>
            {
                entity.ToTable("Observations");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
                entity.Property(e => e.BodySystem).HasMaxLength(50);
                entity.Property(e => e.Severity).HasMaxLength(20);
                entity.Property(e => e.Code).HasMaxLength(20);
                entity.Property(e => e.Unit).HasMaxLength(20);
                entity.HasOne(e => e.ClinicalDocument)
                    .WithMany(d => d.Observations)
                    .HasForeignKey(e => e.ClinicalDocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.ClinicalDocumentId);
                entity.HasIndex(e => e.Type);
                entity.HasIndex(e => e.IsActive);
            });

            // Assessment configuration
            modelBuilder.Entity<AssessmentEntity>(entity =>
            {
                entity.ToTable("Assessments");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Condition).HasMaxLength(30);
                entity.Property(e => e.Prognosis).HasMaxLength(30);
                entity.Property(e => e.Confidence).HasMaxLength(30);
                entity.Property(e => e.Severity).HasMaxLength(20);
                entity.HasOne(e => e.ClinicalDocument)
                    .WithMany(d => d.Assessments)
                    .HasForeignKey(e => e.ClinicalDocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.ClinicalDocumentId);
                entity.HasIndex(e => e.IsActive);
            });

            // Diagnosis configuration
            modelBuilder.Entity<DiagnosisEntity>(entity =>
            {
                entity.ToTable("Diagnoses");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.ICD10Code).HasMaxLength(20);
                entity.Property(e => e.Type).HasMaxLength(30);
                entity.Property(e => e.Status).HasMaxLength(30);
                entity.Property(e => e.Severity).HasMaxLength(20);
                entity.HasOne(e => e.ClinicalDocument)
                    .WithMany(d => d.Diagnoses)
                    .HasForeignKey(e => e.ClinicalDocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.ClinicalDocumentId);
                entity.HasIndex(e => e.ICD10Code);
                entity.HasIndex(e => e.IsActive);
            });

            // Plan configuration
            modelBuilder.Entity<PlanEntity>(entity =>
            {
                entity.ToTable("Plans");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.Type).HasMaxLength(30);
                entity.Property(e => e.Priority).HasMaxLength(20);
                entity.Property(e => e.Severity).HasMaxLength(20);
                entity.HasOne(e => e.ClinicalDocument)
                    .WithMany(d => d.Plans)
                    .HasForeignKey(e => e.ClinicalDocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasIndex(e => e.ClinicalDocumentId);
                entity.HasIndex(e => e.IsActive);
            });

            // Prescription configuration
            modelBuilder.Entity<PrescriptionEntity>(entity =>
            {
                entity.ToTable("Prescriptions");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.MedicationName).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Dosage).HasMaxLength(100);
                entity.Property(e => e.Frequency).HasMaxLength(100);
                entity.Property(e => e.Route).HasMaxLength(50);
                entity.Property(e => e.Duration).HasMaxLength(100);
                entity.Property(e => e.NDCCode).HasMaxLength(20);
                entity.Property(e => e.Severity).HasMaxLength(20);
                entity.HasOne(e => e.ClinicalDocument)
                    .WithMany(d => d.Prescriptions)
                    .HasForeignKey(e => e.ClinicalDocumentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(e => e.Diagnosis)
                    .WithMany(d => d.Prescriptions)
                    .HasForeignKey(e => e.DiagnosisId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasIndex(e => e.ClinicalDocumentId);
                entity.HasIndex(e => e.DiagnosisId);
                entity.HasIndex(e => e.IsActive);
            });
        }
    }
}
