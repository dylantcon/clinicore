using System.Reflection;
using Core.CliniCore.Domain.Authentication.Representation;
using Core.CliniCore.Domain.ClinicalDocumentation;
using Core.CliniCore.Domain.ClinicalDocumentation.ClinicalEntries;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Users;
using Core.CliniCore.Domain.Users.Concrete;
using Core.CliniCore.DTOs.Administrators;
using Core.CliniCore.DTOs.Appointments;
using Core.CliniCore.DTOs.Auth;
using Core.CliniCore.DTOs.ClinicalDocuments;
using Core.CliniCore.DTOs.Patients;
using Core.CliniCore.DTOs.Physicians;
using Core.CliniCore.Requests.Administrators;
using Core.CliniCore.Requests.ClinicalDocuments;
using Core.CliniCore.Requests.Patients;
using Core.CliniCore.Requests.Physicians;
using Core.CliniCore.Scheduling;

namespace Core.CliniCore.Mapping
{
    /// <summary>
    /// Extension methods for mapping between domain models and DTOs
    /// </summary>
    public static class MappingExtensions
    {
        #region Patient Mappings

        public static PatientDto ToDto(this PatientProfile patient, string? physicianName = null)
        {
            return new PatientDto
            {
                Id = patient.Id,
                Username = patient.Username,
                Name = patient.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                BirthDate = patient.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()),
                Gender = patient.GetValue<Gender>(PatientEntryType.Gender.GetKey()).ToString(),
                Race = patient.GetValue<string>(PatientEntryType.Race.GetKey()) ?? string.Empty,
                Address = patient.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty,
                PrimaryPhysicianId = patient.PrimaryPhysicianId,
                PrimaryPhysicianName = physicianName,
                CreatedAt = patient.CreatedAt,
                AppointmentIds = [.. patient.AppointmentIds],
                ClinicalDocumentIds = [.. patient.ClinicalDocumentIds],
                AppointmentCount = patient.AppointmentIds.Count,
                ClinicalDocumentCount = patient.ClinicalDocumentIds.Count
            };
        }

        public static void ApplyUpdate(this PatientProfile patient, UpdatePatientRequest request)
        {
            if (!string.IsNullOrEmpty(request.Name))
                patient.SetValue(CommonEntryType.Name.GetKey(), request.Name);

            if (request.BirthDate.HasValue)
                patient.SetValue(CommonEntryType.BirthDate.GetKey(), request.BirthDate.Value);

            if (!string.IsNullOrEmpty(request.Gender) && Enum.TryParse<Gender>(request.Gender, true, out var gender))
                patient.SetValue(PatientEntryType.Gender.GetKey(), gender);

            if (request.Race != null)
                patient.SetValue(PatientEntryType.Race.GetKey(), request.Race);

            if (request.Address != null)
                patient.SetValue(CommonEntryType.Address.GetKey(), request.Address);

            if (request.PrimaryPhysicianId.HasValue)
                patient.PrimaryPhysicianId = request.PrimaryPhysicianId;
        }

        #endregion

        #region Physician Mappings

        public static PhysicianDto ToDto(this PhysicianProfile physician)
        {
            return new PhysicianDto
            {
                Id = physician.Id,
                Username = physician.Username,
                Name = physician.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                Address = physician.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty,
                BirthDate = physician.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()),
                LicenseNumber = physician.GetValue<string>(PhysicianEntryType.LicenseNumber.GetKey()) ?? string.Empty,
                GraduationDate = physician.GetValue<DateTime>(PhysicianEntryType.GraduationDate.GetKey()),
                Specializations = [.. (physician.GetValue<List<MedicalSpecialization>>(PhysicianEntryType.Specializations.GetKey()) ?? []).Select(s => s.ToString())],
                CreatedAt = physician.CreatedAt,
                PatientCount = physician.PatientIds.Count,
                AppointmentCount = physician.AppointmentIds.Count
            };
        }

        public static void ApplyUpdate(this PhysicianProfile physician, UpdatePhysicianRequest request)
        {
            // Common entry type fields
            if (!string.IsNullOrEmpty(request.Name))
                physician.SetValue(CommonEntryType.Name.GetKey(), request.Name);

            if (!string.IsNullOrEmpty(request.Address))
                physician.SetValue(CommonEntryType.Address.GetKey(), request.Address);

            if (request.BirthDate.HasValue)
                physician.SetValue(CommonEntryType.BirthDate.GetKey(), request.BirthDate.Value);

            // Physician-specific entry type fields
            if (!string.IsNullOrEmpty(request.LicenseNumber))
                physician.SetValue(PhysicianEntryType.LicenseNumber.GetKey(), request.LicenseNumber);

            if (request.GraduationDate.HasValue)
                physician.SetValue(PhysicianEntryType.GraduationDate.GetKey(), request.GraduationDate.Value);

            if (request.Specializations != null && request.Specializations.Count != 0)
            {
                var specs = request.Specializations
                    .Where(s => Enum.TryParse<MedicalSpecialization>(s, true, out _))
                    .Select(s => Enum.Parse<MedicalSpecialization>(s, true))
                    .ToList();
                physician.SetValue(PhysicianEntryType.Specializations.GetKey(), specs);
            }

            // Relationship IDs (for assignment operations)
            if (request.PatientIds != null)
            {
                physician.PatientIds.Clear();
                physician.PatientIds.AddRange(request.PatientIds);
            }

            if (request.AppointmentIds != null)
            {
                physician.AppointmentIds.Clear();
                physician.AppointmentIds.AddRange(request.AppointmentIds);
            }
        }

        #endregion

        #region Administrator Mappings

        public static AdministratorDto ToDto(this AdministratorProfile admin)
        {
            return new AdministratorDto
            {
                Id = admin.Id,
                Username = admin.Username,
                CreatedAt = admin.CreatedAt,
                Name = admin.GetValue<string>(CommonEntryType.Name.GetKey()) ?? string.Empty,
                Address = admin.GetValue<string>(CommonEntryType.Address.GetKey()) ?? string.Empty,
                BirthDate = admin.GetValue<DateTime>(CommonEntryType.BirthDate.GetKey()),
                Email = admin.GetValue<string>(AdministratorEntryType.Email.GetKey()) ?? string.Empty,
                Department = admin.Department,
                Permissions = [.. admin.GrantedPermissions.Select(p => p.ToString())]
            };
        }

        public static void ApplyUpdate(this AdministratorProfile admin, UpdateAdministratorRequest request)
        {
            if (!string.IsNullOrEmpty(request.Name))
                admin.SetValue(CommonEntryType.Name.GetKey(), request.Name);

            if (request.Address != null)
                admin.SetValue(CommonEntryType.Address.GetKey(), request.Address);

            if (request.BirthDate.HasValue)
                admin.SetValue(CommonEntryType.BirthDate.GetKey(), request.BirthDate.Value);

            if (request.Email != null)
                admin.SetValue(AdministratorEntryType.Email.GetKey(), request.Email);

            if (!string.IsNullOrEmpty(request.Department))
                admin.Department = request.Department;

            if (request.Permissions != null)
            {
                admin.GrantedPermissions.Clear();
                foreach (var permStr in request.Permissions)
                {
                    if (Enum.TryParse<Permission>(permStr, true, out var perm))
                        admin.GrantedPermissions.Add(perm);
                }
            }
        }

        #endregion

        #region Appointment Mappings

        public static AppointmentDto ToDto(this AppointmentTimeInterval appointment,
            string? patientName = null, string? physicianName = null)
        {
            return new AppointmentDto
            {
                Id = appointment.Id,
                Start = appointment.Start,
                End = appointment.End,
                DurationMinutes = (int)appointment.Duration.TotalMinutes,
                PatientId = appointment.PatientId,
                PatientName = patientName,
                PhysicianId = appointment.PhysicianId,
                PhysicianName = physicianName,
                Status = appointment.Status.ToString(),
                AppointmentType = appointment.AppointmentType,
                ReasonForVisit = appointment.ReasonForVisit,
                Notes = appointment.Notes,
                ClinicalDocumentId = appointment.ClinicalDocumentId,
                CancellationReason = appointment.CancellationReason,
                RoomNumber = appointment.RoomNumber,
                CreatedAt = appointment.CreatedAt,
                ModifiedAt = appointment.ModifiedAt
            };
        }

        // Note: Appointment updates are handled via SchedulerService.UpdateAppointment()
        // which properly validates conflicts. Direct property updates are not supported.

        #endregion

        #region Clinical Document Mappings

        public static ClinicalDocumentDto ToDto(this ClinicalDocument document,
            string? patientName = null, string? physicianName = null)
        {
            return new ClinicalDocumentDto
            {
                Id = document.Id,
                PatientId = document.PatientId,
                PatientName = patientName,
                PhysicianId = document.PhysicianId,
                PhysicianName = physicianName,
                AppointmentId = document.AppointmentId,
                ChiefComplaint = document.ChiefComplaint,
                CreatedAt = document.CreatedAt,
                CompletedAt = document.CompletedAt,
                IsCompleted = document.IsCompleted,
                Observations = [.. document.GetObservations().Select(o => o.ToObservationDto(document.Id))],
                Assessments = [.. document.GetAssessments().Select(a => a.ToAssessmentDto(document.Id))],
                Diagnoses = [.. document.GetDiagnoses().Select(d => d.ToDiagnosisDto(document.Id))],
                Prescriptions = [.. document.GetPrescriptions().Select(p => p.ToPrescriptionDto(document.Id))],
                Plans = [.. document.GetPlans().Select(p => p.ToPlanDto(document.Id))]
            };
        }

        public static ClinicalDocumentSummaryDto ToSummaryDto(this ClinicalDocument document,
            string? patientName = null, string? physicianName = null)
        {
            return new ClinicalDocumentSummaryDto
            {
                Id = document.Id,
                PatientId = document.PatientId,
                PatientName = patientName,
                PhysicianId = document.PhysicianId,
                PhysicianName = physicianName,
                ChiefComplaint = document.ChiefComplaint,
                CreatedAt = document.CreatedAt,
                IsCompleted = document.IsCompleted,
                DiagnosisCount = document.GetDiagnoses().Count(),
                PrescriptionCount = document.GetPrescriptions().Count()
            };
        }

        public static ClinicalEntryDto ToEntryDto(this AbstractClinicalEntry entry)
        {
            return new ClinicalEntryDto
            {
                Id = entry.Id,
                EntryType = entry.EntryType.ToString(),
                Content = entry.Content,
                RecordedAt = entry.CreatedAt
            };
        }

        public static ObservationDto ToObservationDto(this ObservationEntry observation, Guid documentId)
        {
            return new ObservationDto
            {
                Id = observation.Id,
                DocumentId = documentId,
                AuthorId = observation.AuthorId,
                Content = observation.Content,
                Type = observation.Type,
                BodySystem = observation.BodySystem,
                IsAbnormal = observation.IsAbnormal,
                Severity = observation.Severity,
                ReferenceRange = observation.ReferenceRange,
                Code = observation.Code,
                NumericValue = observation.NumericValue,
                Unit = observation.Unit,
                VitalSigns = observation.VitalSigns.Count > 0 ? new Dictionary<string, string>(observation.VitalSigns) : null,
                CreatedAt = observation.CreatedAt,
                ModifiedAt = observation.ModifiedAt,
                IsActive = observation.IsActive
            };
        }

        public static AssessmentDto ToAssessmentDto(this AssessmentEntry assessment, Guid documentId)
        {
            return new AssessmentDto
            {
                Id = assessment.Id,
                DocumentId = documentId,
                AuthorId = assessment.AuthorId,
                Content = assessment.Content,
                Condition = assessment.Condition,
                Prognosis = assessment.Prognosis,
                Confidence = assessment.Confidence,
                Severity = assessment.Severity,
                RequiresImmediateAction = assessment.RequiresImmediateAction,
                DifferentialDiagnoses = [.. assessment.DifferentialDiagnoses],
                RiskFactors = [.. assessment.RiskFactors],
                CreatedAt = assessment.CreatedAt,
                ModifiedAt = assessment.ModifiedAt,
                IsActive = assessment.IsActive
            };
        }

        public static DiagnosisDto ToDiagnosisDto(this DiagnosisEntry diagnosis, Guid documentId)
        {
            return new DiagnosisDto
            {
                Id = diagnosis.Id,
                DocumentId = documentId,
                AuthorId = diagnosis.AuthorId,
                Content = diagnosis.Content,
                ICD10Code = diagnosis.ICD10Code,
                Type = diagnosis.Type,
                Status = diagnosis.Status,
                Severity = diagnosis.Severity,
                IsPrimary = diagnosis.IsPrimary,
                OnsetDate = diagnosis.OnsetDate,
                RelatedPrescriptionIds = [.. diagnosis.RelatedPrescriptions],
                SupportingObservationIds = [.. diagnosis.SupportingObservations],
                CreatedAt = diagnosis.CreatedAt,
                ModifiedAt = diagnosis.ModifiedAt,
                IsActive = diagnosis.IsActive
            };
        }

        public static PlanDto ToPlanDto(this PlanEntry plan, Guid documentId)
        {
            return new PlanDto
            {
                Id = plan.Id,
                DocumentId = documentId,
                AuthorId = plan.AuthorId,
                Content = plan.Content,
                Type = plan.Type,
                Priority = plan.Priority,
                Severity = plan.Severity,
                TargetDate = plan.TargetDate,
                IsCompleted = plan.IsCompleted,
                CompletedDate = plan.CompletedDate,
                FollowUpInstructions = plan.FollowUpInstructions,
                RelatedDiagnosisIds = [.. plan.RelatedDiagnoses],
                CreatedAt = plan.CreatedAt,
                ModifiedAt = plan.ModifiedAt,
                IsActive = plan.IsActive
            };
        }

        public static PrescriptionDto ToPrescriptionDto(this PrescriptionEntry prescription, Guid documentId)
        {
            return new PrescriptionDto
            {
                Id = prescription.Id,
                DocumentId = documentId,
                AuthorId = prescription.AuthorId,
                DiagnosisId = prescription.DiagnosisId,
                MedicationName = prescription.MedicationName,
                Dosage = prescription.Dosage,
                Frequency = prescription.Frequency,
                Route = prescription.Route,
                Duration = prescription.Duration,
                Refills = prescription.Refills,
                GenericAllowed = prescription.GenericAllowed,
                DEASchedule = prescription.DEASchedule,
                ExpirationDate = prescription.ExpirationDate,
                NDCCode = prescription.NDCCode,
                Instructions = prescription.Instructions,
                Severity = prescription.Severity,
                CreatedAt = prescription.CreatedAt,
                ModifiedAt = prescription.ModifiedAt,
                IsActive = prescription.IsActive
            };
        }

        private static int ParseDurationDays(string? duration)
        {
            if (string.IsNullOrEmpty(duration)) return 0;

            // Try to parse common formats like "7 days", "14 days", etc.
            var parts = duration.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length > 0 && int.TryParse(parts[0], out int days))
            {
                return days;
            }
            return 0;
        }

        // Alias methods for entry->DTO conversion
        public static ObservationDto ToDto(this ObservationEntry entry, Guid documentId) => entry.ToObservationDto(documentId);
        public static AssessmentDto ToDto(this AssessmentEntry entry, Guid documentId) => entry.ToAssessmentDto(documentId);
        public static DiagnosisDto ToDto(this DiagnosisEntry entry, Guid documentId) => entry.ToDiagnosisDto(documentId);
        public static PlanDto ToDto(this PlanEntry entry, Guid documentId) => entry.ToPlanDto(documentId);
        public static PrescriptionDto ToDto(this PrescriptionEntry entry, Guid documentId) => entry.ToPrescriptionDto(documentId);

        // Entry->CreateRequest mappings (for repository POST operations)
        public static CreateObservationRequest ToCreateRequest(this ObservationEntry entry) => new()
        {
            Content = entry.Content,
            Type = entry.Type,
            BodySystem = entry.BodySystem,
            IsAbnormal = entry.IsAbnormal,
            Severity = entry.Severity,
            ReferenceRange = entry.ReferenceRange,
            Code = entry.Code,
            NumericValue = entry.NumericValue,
            Unit = entry.Unit,
            VitalSigns = entry.VitalSigns.Count > 0 ? new Dictionary<string, string>(entry.VitalSigns) : null
        };

        public static CreateAssessmentRequest ToCreateRequest(this AssessmentEntry entry) => new()
        {
            Content = entry.ClinicalImpression,
            Condition = entry.Condition,
            Prognosis = entry.Prognosis,
            Confidence = entry.Confidence,
            Severity = entry.Severity,
            RequiresImmediateAction = entry.RequiresImmediateAction,
            DifferentialDiagnoses = [.. entry.DifferentialDiagnoses],
            RiskFactors = [.. entry.RiskFactors]
        };

        public static CreateDiagnosisRequest ToCreateRequest(this DiagnosisEntry entry) => new()
        {
            Content = entry.Content,
            ICD10Code = entry.ICD10Code,
            Type = entry.Type,
            Status = entry.Status,
            Severity = entry.Severity,
            IsPrimary = entry.IsPrimary,
            OnsetDate = entry.OnsetDate
        };

        public static CreatePlanRequest ToCreateRequest(this PlanEntry entry) => new()
        {
            Content = entry.Content,
            Type = entry.Type,
            Priority = entry.Priority,
            Severity = entry.Severity,
            TargetDate = entry.TargetDate,
            FollowUpInstructions = entry.FollowUpInstructions,
            RelatedDiagnosisIds = [.. entry.RelatedDiagnoses]
        };

        public static CreatePrescriptionRequest ToCreateRequest(this PrescriptionEntry entry) => new()
        {
            DiagnosisId = entry.DiagnosisId,
            MedicationName = entry.MedicationName,
            Dosage = entry.Dosage,
            Frequency = entry.Frequency,
            Route = entry.Route,
            Duration = entry.Duration,
            Refills = entry.Refills,
            GenericAllowed = entry.GenericAllowed,
            DEASchedule = entry.DEASchedule,
            NDCCode = entry.NDCCode,
            Instructions = entry.Instructions,
            Severity = entry.Severity
        };

        // Entry->UpdateRequest mappings (for repository PUT operations)
        public static UpdateObservationRequest ToUpdateRequest(this ObservationEntry entry) => new()
        {
            Content = entry.Content,
            Type = entry.Type,
            BodySystem = entry.BodySystem,
            IsAbnormal = entry.IsAbnormal,
            Severity = entry.Severity,
            ReferenceRange = entry.ReferenceRange,
            Code = entry.Code,
            NumericValue = entry.NumericValue,
            Unit = entry.Unit,
            VitalSigns = entry.VitalSigns.Count > 0 ? new Dictionary<string, string>(entry.VitalSigns) : null,
            IsActive = entry.IsActive
        };

        public static UpdateAssessmentRequest ToUpdateRequest(this AssessmentEntry entry) => new()
        {
            Content = entry.ClinicalImpression,
            Condition = entry.Condition,
            Prognosis = entry.Prognosis,
            Confidence = entry.Confidence,
            Severity = entry.Severity,
            RequiresImmediateAction = entry.RequiresImmediateAction,
            DifferentialDiagnoses = [.. entry.DifferentialDiagnoses],
            RiskFactors = [.. entry.RiskFactors],
            IsActive = entry.IsActive
        };

        public static UpdateDiagnosisRequest ToUpdateRequest(this DiagnosisEntry entry) => new()
        {
            Content = entry.Content,
            ICD10Code = entry.ICD10Code,
            Type = entry.Type,
            Status = entry.Status,
            Severity = entry.Severity,
            IsPrimary = entry.IsPrimary,
            OnsetDate = entry.OnsetDate,
            IsActive = entry.IsActive
        };

        public static UpdatePlanRequest ToUpdateRequest(this PlanEntry entry) => new()
        {
            Content = entry.Content,
            Type = entry.Type,
            Priority = entry.Priority,
            Severity = entry.Severity,
            TargetDate = entry.TargetDate,
            IsCompleted = entry.IsCompleted,
            FollowUpInstructions = entry.FollowUpInstructions,
            RelatedDiagnosisIds = [.. entry.RelatedDiagnoses],
            IsActive = entry.IsActive
        };

        public static UpdatePrescriptionRequest ToUpdateRequest(this PrescriptionEntry entry) => new()
        {
            MedicationName = entry.MedicationName,
            Dosage = entry.Dosage,
            Frequency = entry.Frequency,
            Route = entry.Route,
            Duration = entry.Duration,
            Refills = entry.Refills,
            GenericAllowed = entry.GenericAllowed,
            DEASchedule = entry.DEASchedule,
            ExpirationDate = entry.ExpirationDate,
            NDCCode = entry.NDCCode,
            Instructions = entry.Instructions,
            Severity = entry.Severity,
            IsActive = entry.IsActive
        };

        // DTO->Domain entry mappings
        public static ObservationEntry ToDomain(this ObservationDto dto)
        {
            var entry = new ObservationEntry(dto.AuthorId, dto.Content)
            {
                Type = dto.Type,
                BodySystem = dto.BodySystem,
                IsAbnormal = dto.IsAbnormal,
                Severity = dto.Severity,
                ReferenceRange = dto.ReferenceRange,
                Code = dto.Code,
                NumericValue = dto.NumericValue,
                Unit = dto.Unit,
                IsActive = dto.IsActive
            };

            SetEntryId(entry, dto.Id);
            entry.ModifiedAt = dto.ModifiedAt;

            if (dto.VitalSigns != null)
            {
                foreach (var kvp in dto.VitalSigns)
                    entry.AddVitalSign(kvp.Key, kvp.Value);
            }

            return entry;
        }

        public static AssessmentEntry ToDomain(this AssessmentDto dto)
        {
            var entry = new AssessmentEntry(dto.AuthorId, dto.Content)
            {
                Condition = dto.Condition,
                Prognosis = dto.Prognosis,
                Confidence = dto.Confidence,
                Severity = dto.Severity,
                RequiresImmediateAction = dto.RequiresImmediateAction,
                IsActive = dto.IsActive
            };

            SetEntryId(entry, dto.Id);
            entry.ModifiedAt = dto.ModifiedAt;

            if (dto.DifferentialDiagnoses != null)
            {
                foreach (var diff in dto.DifferentialDiagnoses)
                    entry.DifferentialDiagnoses.Add(diff);
            }

            if (dto.RiskFactors != null)
            {
                foreach (var risk in dto.RiskFactors)
                    entry.RiskFactors.Add(risk);
            }

            return entry;
        }

        public static DiagnosisEntry ToDomain(this DiagnosisDto dto)
        {
            var entry = new DiagnosisEntry(dto.AuthorId, dto.Content)
            {
                ICD10Code = dto.ICD10Code,
                Type = dto.Type,
                Status = dto.Status,
                Severity = dto.Severity,
                IsPrimary = dto.IsPrimary,
                OnsetDate = dto.OnsetDate,
                IsActive = dto.IsActive
            };

            SetEntryId(entry, dto.Id);
            entry.ModifiedAt = dto.ModifiedAt;

            return entry;
        }

        public static PlanEntry ToDomain(this PlanDto dto)
        {
            var entry = new PlanEntry(dto.AuthorId, dto.Content)
            {
                Type = dto.Type,
                Priority = dto.Priority,
                Severity = dto.Severity,
                TargetDate = dto.TargetDate,
                FollowUpInstructions = dto.FollowUpInstructions,
                IsActive = dto.IsActive
            };

            SetEntryId(entry, dto.Id);
            entry.ModifiedAt = dto.ModifiedAt;

            if (dto.IsCompleted)
                entry.MarkCompleted();

            if (dto.RelatedDiagnosisIds != null)
            {
                foreach (var diagId in dto.RelatedDiagnosisIds)
                    entry.RelatedDiagnoses.Add(diagId);
            }

            return entry;
        }

        public static PrescriptionEntry ToDomain(this PrescriptionDto dto)
        {
            var entry = new PrescriptionEntry(dto.AuthorId, dto.DiagnosisId, dto.MedicationName)
            {
                Dosage = dto.Dosage,
                Frequency = dto.Frequency,
                Route = dto.Route,
                Duration = dto.Duration,
                Refills = dto.Refills,
                GenericAllowed = dto.GenericAllowed,
                DEASchedule = dto.DEASchedule,
                ExpirationDate = dto.ExpirationDate,
                NDCCode = dto.NDCCode,
                Instructions = dto.Instructions,
                Severity = dto.Severity,
                IsActive = dto.IsActive
            };

            SetEntryId(entry, dto.Id);
            entry.ModifiedAt = dto.ModifiedAt;

            return entry;
        }

        private static void SetEntryId(AbstractClinicalEntry entry, Guid id)
        {
            typeof(AbstractClinicalEntry)
                .GetProperty("Id")!
                .SetValue(entry, id);
        }

        #endregion

        #region Gender Parsing

        public static Gender ParseGender(string genderString)
        {
            if (Enum.TryParse<Gender>(genderString, true, out var gender))
                return gender;

            return Gender.PreferNotToSay;
        }

        #endregion

        #region Specialization Parsing

        public static List<MedicalSpecialization> ParseSpecializations(List<string> specs)
        {
            return [.. specs
                .Where(s => Enum.TryParse<MedicalSpecialization>(s, true, out _))
                .Select(s => Enum.Parse<MedicalSpecialization>(s, true))];
        }

        #endregion

        #region DTO to Domain Mappings (for Remote repositories)

        public static PatientProfile ToDomain(this PatientDto dto)
        {
            var patient = new PatientProfile
            {
                Username = dto.Username,
                PrimaryPhysicianId = dto.PrimaryPhysicianId
            };

            // Set read-only properties via reflection
            SetBackingField<AbstractUserProfile>(patient, "Id", dto.Id);
            SetBackingField<AbstractUserProfile>(patient, "CreatedAt", dto.CreatedAt);

            // Set profile values
            patient.SetValue(CommonEntryType.Name.GetKey(), dto.Name);
            patient.SetValue(CommonEntryType.BirthDate.GetKey(), dto.BirthDate);
            patient.SetValue(PatientEntryType.Gender.GetKey(), ParseGender(dto.Gender));
            if (!string.IsNullOrEmpty(dto.Race))
                patient.SetValue(PatientEntryType.Race.GetKey(), dto.Race);
            if (!string.IsNullOrEmpty(dto.Address))
                patient.SetValue(CommonEntryType.Address.GetKey(), dto.Address);

            // Populate relationship IDs
            if (dto.AppointmentIds != null)
                patient.AppointmentIds.AddRange(dto.AppointmentIds);
            if (dto.ClinicalDocumentIds != null)
                patient.ClinicalDocumentIds.AddRange(dto.ClinicalDocumentIds);

            return patient;
        }

        public static PhysicianProfile ToDomain(this PhysicianDto dto)
        {
            var physician = new PhysicianProfile
            {
                Username = dto.Username
            };

            // Set read-only properties via reflection
            SetBackingField<AbstractUserProfile>(physician, "Id", dto.Id);
            SetBackingField<AbstractUserProfile>(physician, "CreatedAt", dto.CreatedAt);

            // Set common profile values
            physician.SetValue(CommonEntryType.Name.GetKey(), dto.Name);
            physician.SetValue(CommonEntryType.Address.GetKey(), dto.Address);
            physician.SetValue(CommonEntryType.BirthDate.GetKey(), dto.BirthDate);

            // Set physician-specific profile values
            physician.SetValue(PhysicianEntryType.LicenseNumber.GetKey(), dto.LicenseNumber);
            physician.SetValue(PhysicianEntryType.GraduationDate.GetKey(), dto.GraduationDate);
            physician.SetValue(PhysicianEntryType.Specializations.GetKey(), ParseSpecializations(dto.Specializations));

            return physician;
        }

        public static AdministratorProfile ToDomain(this AdministratorDto dto)
        {
            var admin = new AdministratorProfile
            {
                Username = dto.Username,
                Department = dto.Department
            };

            // Set read-only properties via reflection
            SetBackingField<AbstractUserProfile>(admin, "Id", dto.Id);
            SetBackingField<AbstractUserProfile>(admin, "CreatedAt", dto.CreatedAt);

            // Set profile values (common entries)
            admin.SetValue(CommonEntryType.Name.GetKey(), dto.Name);
            admin.SetValue(CommonEntryType.Address.GetKey(), dto.Address);
            admin.SetValue(CommonEntryType.BirthDate.GetKey(), dto.BirthDate);

            // Set administrator-specific entry
            admin.SetValue(AdministratorEntryType.Email.GetKey(), dto.Email);

            // Set permissions
            foreach (var permStr in dto.Permissions)
            {
                if (Enum.TryParse<Permission>(permStr, true, out var perm))
                    admin.GrantedPermissions.Add(perm);
            }

            return admin;
        }

        public static AppointmentTimeInterval ToDomain(this AppointmentDto dto)
        {
            var appointment = new AppointmentTimeInterval(
                dto.Start,
                dto.End,
                dto.PatientId,
                dto.PhysicianId,
                dto.ReasonForVisit ?? "Appointment"
            );

            // Set read-only Id via reflection
            SetBackingField<AbstractTimeInterval>(appointment, "Id", dto.Id);
            SetBackingField<AbstractTimeInterval>(appointment, "CreatedAt", dto.CreatedAt);

            // Set mutable properties
            appointment.AppointmentType = dto.AppointmentType ?? "Standard Visit";
            appointment.ReasonForVisit = dto.ReasonForVisit;
            appointment.Notes = dto.Notes;
            appointment.ClinicalDocumentId = dto.ClinicalDocumentId;
            appointment.RoomNumber = dto.RoomNumber;

            if (dto.ModifiedAt.HasValue)
                appointment.ModifiedAt = dto.ModifiedAt.Value;

            // Set status via reflection (it has a private setter)
            if (Enum.TryParse<AppointmentStatus>(dto.Status, true, out var status))
            {
                typeof(AppointmentTimeInterval)
                    .GetProperty("Status")!
                    .SetValue(appointment, status);
            }

            return appointment;
        }

        public static ClinicalDocument ToDomain(this ClinicalDocumentDto dto)
        {
            var document = new ClinicalDocument(dto.PatientId, dto.PhysicianId, dto.AppointmentId);

            // Set read-only properties via reflection
            SetBackingField<ClinicalDocument>(document, "Id", dto.Id);
            SetBackingField<ClinicalDocument>(document, "CreatedAt", dto.CreatedAt);

            document.ChiefComplaint = dto.ChiefComplaint;

            // Restore entries using the domain's RestoreEntry method
            // Order matters: diagnoses must be restored before prescriptions
            foreach (var obsDto in dto.Observations)
                document.RestoreEntry(obsDto.ToDomain());

            foreach (var assessmentDto in dto.Assessments)
                document.RestoreEntry(assessmentDto.ToDomain());

            foreach (var diagnosisDto in dto.Diagnoses)
                document.RestoreEntry(diagnosisDto.ToDomain());

            foreach (var planDto in dto.Plans)
                document.RestoreEntry(planDto.ToDomain());

            foreach (var prescriptionDto in dto.Prescriptions)
                document.RestoreEntry(prescriptionDto.ToDomain());

            // Set CompletedAt last (after restoring entries)
            if (dto.CompletedAt.HasValue)
            {
                typeof(ClinicalDocument)
                    .GetProperty("CompletedAt")!
                    .SetValue(document, dto.CompletedAt);
            }

            return document;
        }

        #endregion

        #region UserCredential Mappings

        public static UserCredentialDto ToDto(this UserCredential credential)
        {
            return new UserCredentialDto
            {
                Id = credential.Id,
                Username = credential.Username,
                PasswordHash = credential.PasswordHash,
                Role = credential.Role,
                CreatedAt = credential.CreatedAt,
                LastLoginAt = credential.LastLoginAt
            };
        }

        public static UserCredential ToDomain(this UserCredentialDto dto)
        {
            return new UserCredential
            {
                Id = dto.Id,
                Username = dto.Username,
                PasswordHash = dto.PasswordHash,
                Role = dto.Role,
                CreatedAt = dto.CreatedAt,
                LastLoginAt = dto.LastLoginAt
            };
        }

        #endregion

        #region Reflection Helpers

        private static void SetBackingField<TDeclaring>(object instance, string propertyName, object? value)
        {
            var field = typeof(TDeclaring).GetField(
                $"<{propertyName}>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            field?.SetValue(instance, value);
        }

        #endregion
    }
}
