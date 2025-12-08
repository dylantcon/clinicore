using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using API.CliniCore.Data.Entities;
using API.CliniCore.Data.Entities.Auth;
using API.CliniCore.Data.Entities.Clinical;
using API.CliniCore.Data.Entities.ClinicalEntries;
using API.CliniCore.Data.Entities.User;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.Scheduling;

namespace API.CliniCore.Data
{
    /// <summary>
    /// Maps between domain entities and database entities.
    /// Handles JSON serialization for complex properties.
    /// </summary>
    public static class EntityMappers
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        #region Patient Mapping

        public static PatientEntity ToEntity(this PatientProfile patient)
        {
            return new PatientEntity
            {
                Id = patient.Id,
                Username = patient.Username,
                CreatedAt = patient.CreatedAt,
                Name = patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                BirthDate = patient.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()),
                Gender = patient.GetValue<Gender>(PatientEntryType.Gender.GetKey()).ToString(),
                Race = patient.GetValue<string>(PatientEntryType.Race.GetKey()) ?? string.Empty,
                Address = patient.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty,
                PrimaryPhysicianId = patient.PrimaryPhysicianId,
                AppointmentIdsJson = JsonSerializer.Serialize(patient.AppointmentIds, JsonOptions),
                ClinicalDocumentIdsJson = JsonSerializer.Serialize(patient.ClinicalDocumentIds, JsonOptions)
            };
        }

        public static PatientProfile ToDomain(this PatientEntity entity)
        {
            var patient = new PatientProfile
            {
                Username = entity.Username,
                PrimaryPhysicianId = entity.PrimaryPhysicianId
            };

            // Set the Id using reflection (it's read-only)
            typeof(AbstractUserProfile)
                .GetProperty("Id")!
                .GetBackingField()
                ?.SetValue(patient, entity.Id);

            // Set CreatedAt using reflection
            typeof(AbstractUserProfile)
                .GetProperty("CreatedAt")!
                .GetBackingField()
                ?.SetValue(patient, entity.CreatedAt);

            // Set profile values (common)
            patient.SetValue(CommonEntryType.Name.GetKey(), entity.Name);
            patient.SetValue(CommonEntryType.BirthDate.GetKey(), entity.BirthDate);
            patient.SetValue(CommonEntryType.Address.GetKey(), entity.Address);

            // Set profile values (patient-specific)
            if (Enum.TryParse<Gender>(entity.Gender, true, out var gender))
                patient.SetValue(PatientEntryType.Gender.GetKey(), gender);
            patient.SetValue(PatientEntryType.Race.GetKey(), entity.Race);

            // Restore relationship IDs
            var appointmentIds = JsonSerializer.Deserialize<List<Guid>>(entity.AppointmentIdsJson, JsonOptions) ?? new();
            foreach (var id in appointmentIds)
                patient.AppointmentIds.Add(id);

            var docIds = JsonSerializer.Deserialize<List<Guid>>(entity.ClinicalDocumentIdsJson, JsonOptions) ?? new();
            foreach (var id in docIds)
                patient.ClinicalDocumentIds.Add(id);

            return patient;
        }

        #endregion

        #region Physician Mapping

        public static PhysicianEntity ToEntity(this PhysicianProfile physician)
        {
            var specs = physician.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey())?.Select(s => s.ToString()).ToList() ?? new();
            return new PhysicianEntity
            {
                Id = physician.Id,
                Username = physician.Username,
                CreatedAt = physician.CreatedAt,
                Name = physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                Address = physician.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty,
                BirthDate = physician.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()),
                LicenseNumber = physician.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty,
                GraduationDate = physician.GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey()),
                SpecializationsJson = JsonSerializer.Serialize(specs, JsonOptions),
                PatientIdsJson = JsonSerializer.Serialize(physician.PatientIds, JsonOptions),
                AppointmentIdsJson = JsonSerializer.Serialize(physician.AppointmentIds, JsonOptions)
            };
        }

        public static PhysicianProfile ToDomain(this PhysicianEntity entity)
        {
            var physician = new PhysicianProfile
            {
                Username = entity.Username
            };

            // Set the Id using reflection
            typeof(AbstractUserProfile)
                .GetProperty("Id")!
                .GetBackingField()
                ?.SetValue(physician, entity.Id);

            // Set CreatedAt using reflection
            typeof(AbstractUserProfile)
                .GetProperty("CreatedAt")!
                .GetBackingField()
                ?.SetValue(physician, entity.CreatedAt);

            // Set profile values (common)
            physician.SetValue(CommonEntryType.Name.GetKey(), entity.Name);
            physician.SetValue(CommonEntryType.BirthDate.GetKey(), entity.BirthDate);
            physician.SetValue(CommonEntryType.Address.GetKey(), entity.Address);

            // Set profile values (physician-specific)
            physician.SetValue(PhysicianEntryType.LicenseNumber.GetKey(), entity.LicenseNumber);
            physician.SetValue(PhysicianEntryType.GraduationDate.GetKey(), entity.GraduationDate);

            // Parse specializations
            var specStrings = JsonSerializer.Deserialize<List<string>>(entity.SpecializationsJson, JsonOptions) ?? new();
            var specializations = specStrings
                .Where(s => Enum.TryParse<MedicalSpecialization>(s, true, out _))
                .Select(s => Enum.Parse<MedicalSpecialization>(s, true))
                .ToList();
            physician.SetValue(PhysicianEntryType.Specializations.GetKey(), specializations);

            // Restore relationship IDs
            var patientIds = JsonSerializer.Deserialize<List<Guid>>(entity.PatientIdsJson, JsonOptions) ?? new();
            foreach (var id in patientIds)
                physician.PatientIds.Add(id);

            var appointmentIds = JsonSerializer.Deserialize<List<Guid>>(entity.AppointmentIdsJson, JsonOptions) ?? new();
            foreach (var id in appointmentIds)
                physician.AppointmentIds.Add(id);

            return physician;
        }

        #endregion

        #region Administrator Mapping

        public static AdministratorEntity ToEntity(this AdministratorProfile admin)
        {
            var permissions = admin.GrantedPermissions?.Select(p => p.ToString()).ToList() ?? new();
            return new AdministratorEntity
            {
                Id = admin.Id,
                Username = admin.Username,
                CreatedAt = admin.CreatedAt,
                Name = admin.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                Address = admin.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty,
                BirthDate = admin.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()),
                Email = admin.GetValue<string>(AdministratorEntryType.Email.GetKey()) ?? string.Empty,
                Department = admin.Department,
                PermissionsJson = JsonSerializer.Serialize(permissions, JsonOptions)
            };
        }

        public static AdministratorProfile ToDomain(this AdministratorEntity entity)
        {
            var admin = new AdministratorProfile
            {
                Username = entity.Username,
                Department = entity.Department
            };

            // Set the Id using reflection
            typeof(AbstractUserProfile)
                .GetProperty("Id")!
                .GetBackingField()
                ?.SetValue(admin, entity.Id);

            // Set CreatedAt using reflection
            typeof(AbstractUserProfile)
                .GetProperty("CreatedAt")!
                .GetBackingField()
                ?.SetValue(admin, entity.CreatedAt);

            // Set profile values (common)
            admin.SetValue(CommonEntryType.Name.GetKey(), entity.Name);
            // Only set BirthDate if it has a valid value (not default)
            if (entity.BirthDate != default)
                admin.SetValue(CommonEntryType.BirthDate.GetKey(), entity.BirthDate);
            admin.SetValue(CommonEntryType.Address.GetKey(), entity.Address);

            // Set profile values (admin-specific)
            admin.SetValue(AdministratorEntryType.Email.GetKey(), entity.Email);

            // Restore permissions
            var permStrings = JsonSerializer.Deserialize<List<string>>(entity.PermissionsJson, JsonOptions) ?? new();
            foreach (var permStr in permStrings)
            {
                if (Enum.TryParse<Permission>(permStr, true, out var permission))
                    admin.GrantedPermissions.Add(permission);
            }

            return admin;
        }

        #endregion

        #region Appointment Mapping

        public static AppointmentEntity ToEntity(this AppointmentTimeInterval appointment)
        {
            return new AppointmentEntity
            {
                Id = appointment.Id,
                Start = appointment.Start,
                End = appointment.End,
                Description = appointment.Description,
                PatientId = appointment.PatientId,
                PhysicianId = appointment.PhysicianId,
                Status = appointment.Status.ToString(),
                CreatedAt = appointment.CreatedAt,
                ModifiedAt = appointment.ModifiedAt,
                AppointmentType = appointment.AppointmentType,
                ReasonForVisit = appointment.ReasonForVisit,
                Notes = appointment.Notes,
                ClinicalDocumentId = appointment.ClinicalDocumentId,
                RescheduledFromId = appointment.RescheduledFromId,
                CancellationReason = appointment.CancellationReason,
                RoomNumber = appointment.RoomNumber
            };
        }

        public static AppointmentTimeInterval ToDomain(this AppointmentEntity entity)
        {
            if (!Enum.TryParse<AppointmentStatus>(entity.Status, true, out var status))
                status = AppointmentStatus.Scheduled;

            var appointment = new AppointmentTimeInterval(
                entity.Start,
                entity.End,
                entity.PatientId,
                entity.PhysicianId,
                entity.Description,
                status);

            // Set the Id using reflection (protected set)
            typeof(AbstractTimeInterval)
                .GetProperty("Id")!
                .SetValue(appointment, entity.Id);

            // Set other properties
            appointment.CreatedAt = entity.CreatedAt;
            appointment.ModifiedAt = entity.ModifiedAt;
            appointment.AppointmentType = entity.AppointmentType;
            appointment.ReasonForVisit = entity.ReasonForVisit;
            appointment.Notes = entity.Notes;
            appointment.ClinicalDocumentId = entity.ClinicalDocumentId;
            appointment.RescheduledFromId = entity.RescheduledFromId;
            appointment.CancellationReason = entity.CancellationReason;
            if (entity.RoomNumber.HasValue)
                appointment.RoomNumber = entity.RoomNumber.Value;

            return appointment;
        }

        #endregion

        #region Clinical Document Mapping

        public static ClinicalDocumentEntity ToEntity(this ClinicalDocument document)
        {
            var entity = new ClinicalDocumentEntity
            {
                Id = document.Id,
                PatientId = document.PatientId,
                PhysicianId = document.PhysicianId,
                AppointmentId = document.AppointmentId,
                CreatedAt = document.CreatedAt,
                CompletedAt = document.CompletedAt,
                ChiefComplaint = document.ChiefComplaint
            };

            // Map observations
            foreach (var obs in document.GetObservations())
                entity.Observations.Add(obs.ToEntity(document.Id));

            // Map assessments
            foreach (var assessment in document.GetAssessments())
                entity.Assessments.Add(assessment.ToEntity(document.Id));

            // Map diagnoses
            foreach (var diagnosis in document.GetDiagnoses())
                entity.Diagnoses.Add(diagnosis.ToEntity(document.Id));

            // Map plans
            foreach (var plan in document.GetPlans())
                entity.Plans.Add(plan.ToEntity(document.Id));

            // Map prescriptions
            foreach (var prescription in document.GetPrescriptions())
                entity.Prescriptions.Add(prescription.ToEntity(document.Id));

            return entity;
        }

        public static ClinicalDocument ToDomain(this ClinicalDocumentEntity entity)
        {
            var document = new ClinicalDocument(entity.PatientId, entity.PhysicianId, entity.AppointmentId);

            // Set the Id using reflection
            typeof(ClinicalDocument)
                .GetField("<Id>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(document, entity.Id);

            // Set CreatedAt
            typeof(ClinicalDocument)
                .GetField("<CreatedAt>k__BackingField", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.SetValue(document, entity.CreatedAt);

            // Set CompletedAt
            if (entity.CompletedAt.HasValue)
            {
                typeof(ClinicalDocument)
                    .GetProperty("CompletedAt")!
                    .SetValue(document, entity.CompletedAt);
            }

            document.ChiefComplaint = entity.ChiefComplaint;

            // Map diagnoses first (prescriptions depend on them)
            foreach (var diagEntity in entity.Diagnoses.Where(d => d.IsActive))
            {
                var diagnosis = diagEntity.ToDomain();
                try { document.AddEntry(diagnosis); } catch { }
            }

            // Map observations
            foreach (var obsEntity in entity.Observations.Where(o => o.IsActive))
            {
                var observation = obsEntity.ToDomain();
                try { document.AddEntry(observation); } catch { }
            }

            // Map assessments
            foreach (var assessEntity in entity.Assessments.Where(a => a.IsActive))
            {
                var assessment = assessEntity.ToDomain();
                try { document.AddEntry(assessment); } catch { }
            }

            // Map plans
            foreach (var planEntity in entity.Plans.Where(p => p.IsActive))
            {
                var plan = planEntity.ToDomain();
                try { document.AddEntry(plan); } catch { }
            }

            // Map prescriptions
            foreach (var rxEntity in entity.Prescriptions.Where(p => p.IsActive))
            {
                var prescription = rxEntity.ToDomain();
                try { document.AddEntry(prescription); } catch { }
            }

            return document;
        }

        #endregion

        #region Clinical Entry Entity Mapping

        public static ObservationEntity ToEntity(this ObservationEntry entry, Guid documentId)
        {
            return new ObservationEntity
            {
                Id = entry.Id,
                ClinicalDocumentId = documentId,
                AuthorId = entry.AuthorId,
                Content = entry.Content,
                Type = entry.Type.ToString(),
                BodySystem = entry.BodySystem?.ToString(),
                IsAbnormal = entry.IsAbnormal,
                Severity = entry.Severity.ToString(),
                ReferenceRange = entry.ReferenceRange,
                Code = entry.Code,
                NumericValue = entry.NumericValue,
                Unit = entry.Unit,
                VitalSignsJson = entry.VitalSigns != null
                    ? JsonSerializer.Serialize(entry.VitalSigns, JsonOptions)
                    : null,
                CreatedAt = entry.CreatedAt,
                ModifiedAt = entry.ModifiedAt,
                IsActive = entry.IsActive
            };
        }

        public static ObservationEntry ToDomain(this ObservationEntity entity)
        {
            var entry = new ObservationEntry(entity.AuthorId, entity.Content);
            SetEntryId(entry, entity.Id);

            if (Enum.TryParse<ObservationType>(entity.Type, true, out var obsType))
                entry.Type = obsType;

            if (!string.IsNullOrEmpty(entity.BodySystem) && Enum.TryParse<BodySystem>(entity.BodySystem, true, out var bodySystem))
                entry.BodySystem = bodySystem;
            entry.IsAbnormal = entity.IsAbnormal;
            if (Enum.TryParse<EntrySeverity>(entity.Severity, true, out var severity))
                entry.Severity = severity;
            entry.ReferenceRange = entity.ReferenceRange;
            entry.Code = entity.Code;
            entry.NumericValue = entity.NumericValue;
            entry.Unit = entity.Unit;
            entry.ModifiedAt = entity.ModifiedAt;
            entry.IsActive = entity.IsActive;

            if (!string.IsNullOrEmpty(entity.VitalSignsJson))
            {
                var vitalSigns = JsonSerializer.Deserialize<Dictionary<string, string>>(entity.VitalSignsJson, JsonOptions);
                if (vitalSigns != null)
                {
                    foreach (var kvp in vitalSigns)
                        entry.AddVitalSign(kvp.Key, kvp.Value);
                }
            }

            return entry;
        }

        public static AssessmentEntity ToEntity(this AssessmentEntry entry, Guid documentId)
        {
            return new AssessmentEntity
            {
                Id = entry.Id,
                ClinicalDocumentId = documentId,
                AuthorId = entry.AuthorId,
                Content = entry.ClinicalImpression,
                Condition = entry.Condition.ToString(),
                Prognosis = entry.Prognosis.ToString(),
                Confidence = entry.Confidence.ToString(),
                Severity = entry.Severity.ToString(),
                RequiresImmediateAction = entry.RequiresImmediateAction,
                DifferentialDiagnosesJson = entry.DifferentialDiagnoses?.Count > 0
                    ? JsonSerializer.Serialize(entry.DifferentialDiagnoses, JsonOptions)
                    : null,
                RiskFactorsJson = entry.RiskFactors?.Count > 0
                    ? JsonSerializer.Serialize(entry.RiskFactors, JsonOptions)
                    : null,
                CreatedAt = entry.CreatedAt,
                ModifiedAt = entry.ModifiedAt,
                IsActive = entry.IsActive
            };
        }

        public static AssessmentEntry ToDomain(this AssessmentEntity entity)
        {
            var entry = new AssessmentEntry(entity.AuthorId, entity.Content);
            SetEntryId(entry, entity.Id);

            if (Enum.TryParse<PatientCondition>(entity.Condition, true, out var condition))
                entry.Condition = condition;
            if (Enum.TryParse<Prognosis>(entity.Prognosis, true, out var prognosis))
                entry.Prognosis = prognosis;
            if (Enum.TryParse<ConfidenceLevel>(entity.Confidence, true, out var confidence))
                entry.Confidence = confidence;
            if (Enum.TryParse<EntrySeverity>(entity.Severity, true, out var severity))
                entry.Severity = severity;

            entry.RequiresImmediateAction = entity.RequiresImmediateAction;
            entry.ModifiedAt = entity.ModifiedAt;
            entry.IsActive = entity.IsActive;

            if (!string.IsNullOrEmpty(entity.DifferentialDiagnosesJson))
            {
                var differentials = JsonSerializer.Deserialize<List<string>>(entity.DifferentialDiagnosesJson, JsonOptions);
                if (differentials != null)
                {
                    foreach (var diff in differentials)
                        entry.DifferentialDiagnoses.Add(diff);
                }
            }
            if (!string.IsNullOrEmpty(entity.RiskFactorsJson))
            {
                var riskFactors = JsonSerializer.Deserialize<List<string>>(entity.RiskFactorsJson, JsonOptions);
                if (riskFactors != null)
                {
                    foreach (var risk in riskFactors)
                        entry.RiskFactors.Add(risk);
                }
            }

            return entry;
        }

        public static DiagnosisEntity ToEntity(this DiagnosisEntry entry, Guid documentId)
        {
            return new DiagnosisEntity
            {
                Id = entry.Id,
                ClinicalDocumentId = documentId,
                AuthorId = entry.AuthorId,
                Content = entry.Content,
                ICD10Code = entry.ICD10Code,
                Type = entry.Type.ToString(),
                Status = entry.Status.ToString(),
                Severity = entry.Severity.ToString(),
                IsPrimary = entry.IsPrimary,
                OnsetDate = entry.OnsetDate,
                CreatedAt = entry.CreatedAt,
                ModifiedAt = entry.ModifiedAt,
                IsActive = entry.IsActive
            };
        }

        public static DiagnosisEntry ToDomain(this DiagnosisEntity entity)
        {
            var entry = new DiagnosisEntry(entity.AuthorId, entity.Content);
            SetEntryId(entry, entity.Id);

            // Set ICD10Code via reflection (readonly property)
            typeof(DiagnosisEntry)
                .GetProperty("ICD10Code")!
                .SetValue(entry, entity.ICD10Code);

            if (Enum.TryParse<DiagnosisType>(entity.Type, true, out var diagType))
                entry.Type = diagType;
            if (Enum.TryParse<DiagnosisStatus>(entity.Status, true, out var status))
                entry.Status = status;
            if (Enum.TryParse<EntrySeverity>(entity.Severity, true, out var severity))
                entry.Severity = severity;

            entry.IsPrimary = entity.IsPrimary;
            entry.OnsetDate = entity.OnsetDate;
            entry.ModifiedAt = entity.ModifiedAt;
            entry.IsActive = entity.IsActive;

            return entry;
        }

        public static PlanEntity ToEntity(this PlanEntry entry, Guid documentId)
        {
            return new PlanEntity
            {
                Id = entry.Id,
                ClinicalDocumentId = documentId,
                AuthorId = entry.AuthorId,
                Content = entry.Content,
                Type = entry.Type.ToString(),
                Priority = entry.Priority.ToString(),
                Severity = entry.Severity.ToString(),
                TargetDate = entry.TargetDate,
                IsCompleted = entry.IsCompleted,
                CompletedDate = entry.CompletedDate,
                FollowUpInstructions = entry.FollowUpInstructions,
                RelatedDiagnosisIdsJson = entry.RelatedDiagnoses?.Count > 0
                    ? JsonSerializer.Serialize(entry.RelatedDiagnoses, JsonOptions)
                    : null,
                CreatedAt = entry.CreatedAt,
                ModifiedAt = entry.ModifiedAt,
                IsActive = entry.IsActive
            };
        }

        public static PlanEntry ToDomain(this PlanEntity entity)
        {
            var entry = new PlanEntry(entity.AuthorId, entity.Content);
            SetEntryId(entry, entity.Id);

            if (Enum.TryParse<PlanType>(entity.Type, true, out var planType))
                entry.Type = planType;
            if (Enum.TryParse<PlanPriority>(entity.Priority, true, out var priority))
                entry.Priority = priority;
            if (Enum.TryParse<EntrySeverity>(entity.Severity, true, out var severity))
                entry.Severity = severity;

            entry.TargetDate = entity.TargetDate;
            entry.IsCompleted = entity.IsCompleted;
            entry.CompletedDate = entity.CompletedDate;
            entry.FollowUpInstructions = entity.FollowUpInstructions;
            entry.ModifiedAt = entity.ModifiedAt;
            entry.IsActive = entity.IsActive;

            if (!string.IsNullOrEmpty(entity.RelatedDiagnosisIdsJson))
            {
                var relatedDiagnoses = JsonSerializer.Deserialize<List<Guid>>(entity.RelatedDiagnosisIdsJson, JsonOptions);
                if (relatedDiagnoses != null)
                {
                    foreach (var diagId in relatedDiagnoses)
                        entry.RelatedDiagnoses.Add(diagId);
                }
            }

            return entry;
        }

        public static PrescriptionEntity ToEntity(this PrescriptionEntry entry, Guid documentId)
        {
            return new PrescriptionEntity
            {
                Id = entry.Id,
                ClinicalDocumentId = documentId,
                AuthorId = entry.AuthorId,
                DiagnosisId = entry.DiagnosisId,
                MedicationName = entry.MedicationName,
                Dosage = entry.Dosage,
                Frequency = entry.Frequency?.ToString(),
                Route = entry.Route.ToString(),
                Duration = entry.Duration,
                Refills = entry.Refills,
                GenericAllowed = entry.GenericAllowed,
                DEASchedule = entry.DEASchedule,
                ExpirationDate = entry.ExpirationDate,
                NDCCode = entry.NDCCode,
                Instructions = entry.Instructions,
                Severity = entry.Severity.ToString(),
                CreatedAt = entry.CreatedAt,
                ModifiedAt = entry.ModifiedAt,
                IsActive = entry.IsActive
            };
        }

        public static PrescriptionEntry ToDomain(this PrescriptionEntity entity)
        {
            var entry = new PrescriptionEntry(entity.AuthorId, entity.DiagnosisId, entity.MedicationName);
            SetEntryId(entry, entity.Id);

            entry.Dosage = entity.Dosage;
            if (!string.IsNullOrEmpty(entity.Frequency) && Enum.TryParse<DosageFrequency>(entity.Frequency, true, out var frequency))
                entry.Frequency = frequency;
            if (!string.IsNullOrEmpty(entity.Route) && Enum.TryParse<MedicationRoute>(entity.Route, true, out var route))
                entry.Route = route;
            entry.Duration = entity.Duration;
            entry.Refills = entity.Refills;
            entry.GenericAllowed = entity.GenericAllowed;
            entry.DEASchedule = entity.DEASchedule;
            entry.ExpirationDate = entity.ExpirationDate;
            entry.NDCCode = entity.NDCCode;
            entry.Instructions = entity.Instructions;
            entry.ModifiedAt = entity.ModifiedAt;
            entry.IsActive = entity.IsActive;

            if (Enum.TryParse<EntrySeverity>(entity.Severity, true, out var severity))
                entry.Severity = severity;

            return entry;
        }

        private static void SetEntryId(AbstractClinicalEntry entry, Guid id)
        {
            typeof(AbstractClinicalEntry)
                .GetProperty("Id")!
                .SetValue(entry, id);
        }

        #endregion

        #region UserCredential Mapping

        public static UserCredentialEntity ToEntity(this UserCredential credential)
        {
            return new UserCredentialEntity
            {
                Id = credential.Id,
                Username = credential.Username,
                PasswordHash = credential.PasswordHash,
                Role = credential.Role,
                CreatedAt = credential.CreatedAt,
                LastLoginAt = credential.LastLoginAt
            };
        }

        public static UserCredential ToDomain(this UserCredentialEntity entity)
        {
            return new UserCredential
            {
                Id = entity.Id,
                Username = entity.Username,
                PasswordHash = entity.PasswordHash,
                Role = entity.Role,
                CreatedAt = entity.CreatedAt,
                LastLoginAt = entity.LastLoginAt
            };
        }

        #endregion

        #region Helper Extensions

        private static System.Reflection.FieldInfo? GetBackingField(this System.Reflection.PropertyInfo property)
        {
            return property.DeclaringType?.GetField(
                $"<{property.Name}>k__BackingField",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        }

        #endregion
    }
}
