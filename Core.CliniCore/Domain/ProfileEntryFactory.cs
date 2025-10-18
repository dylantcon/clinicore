using Core.CliniCore.Commands.Profile;
using Core.CliniCore.Domain.Enumerations;
using Core.CliniCore.Domain.Enumerations.EntryTypes;
using Core.CliniCore.Domain.Enumerations.Extensions;
using Core.CliniCore.Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain
{
    public static class ProfileEntryFactory
    {

        #region Generic Factory Methods

        public static ProfileEntry<T> Create<T>(string key, string displayName,
            IValidator<T>? validator = null, bool isRequired = false)
        {
            Func<T, bool>? validatorFunc = validator != null ? validator.IsValid : null;
            string? errorMessage = validator?.ErrorMessage;
            return new ProfileEntry<T>(key, displayName, isRequired, validatorFunc, errorMessage);
        }

        public static ProfileEntry<T> CreateRequired<T>(string key, string displayName,
            IValidator<T>? validator = null)
        {
            return Create(key, displayName, validator, isRequired: true);
        }

        #endregion

        #region Common Person Entries

        public static ProfileEntry<string> CreateName()
            => CreateRequired(
                CommonEntryType.Name.GetKey(),
                CommonEntryType.Name.GetDisplayName(),
                ValidatorFactory.Composite(
                    ValidatorFactory.Required<string>("Name is required"),
                    ValidatorFactory.StringLength(2, 100, "Name must be 2-100 characters")
                ));

        public static ProfileEntry<string> CreateAddress()
            => CreateRequired(
                CommonEntryType.Address.GetKey(),
                CommonEntryType.Address.GetDisplayName(),
                ValidatorFactory.Composite(
                    ValidatorFactory.Required<string>("Address is required"),
                    ValidatorFactory.StringLength(maxLength: 200, errorMessage: "Address must be 200 characters or less")
                ));

        public static ProfileEntry<DateTime> CreateBirthDate()
            => CreateRequired(
                CommonEntryType.BirthDate.GetKey(),
                CommonEntryType.BirthDate.GetDisplayName(),
                ValidatorFactory.Composite(
                    ValidatorFactory.Required<DateTime>("Birth date is required"),
                    ValidatorFactory.BirthDate()
                ));

        #endregion

        #region Patient-Specific Entries

        public static ProfileEntry<string> CreatePatientRace()
            => Create(
                PatientEntryType.Race.GetKey(),
                PatientEntryType.Race.GetDisplayName(),
                ValidatorFactory.StringLength(
                    maxLength: 50,
                    errorMessage: "Race must be 50 characters or less"
                ));

        public static ProfileEntry<Gender> CreatePatientGender()
            => CreateRequired(
                PatientEntryType.Gender.GetKey(),
                PatientEntryType.Gender.GetDisplayName(),
                ValidatorFactory.ValidEnum<Gender>(allowNull: false));

        #endregion

        #region Physician-Specific Entries

        public static ProfileEntry<string> CreatePhysicianLicenseNumber()
            => CreateRequired(
                PhysicianEntryType.LicenseNumber.GetKey(),
                PhysicianEntryType.LicenseNumber.GetDisplayName(),
                ValidatorFactory.Composite(
                    ValidatorFactory.Required<string>(ValidatorFactory.LICENSE_ERROR_STR),
                    ValidatorFactory.LicenseNumber()
                ));

        public static ProfileEntry<DateTime> CreatePhysicianGraduationDate()
            => CreateRequired(
                PhysicianEntryType.GraduationDate.GetKey(),
                PhysicianEntryType.GraduationDate.GetDisplayName(),
                ValidatorFactory.Composite(
                    ValidatorFactory.Required<DateTime>(ValidatorFactory.GRADDATE_SIZE_ERROR_STR),
                    ValidatorFactory.GraduationDate()
                ));

        public static ProfileEntry<List<MedicalSpecialization>> CreatePhysicianSpecializationList()
            => CreateRequired(
                PhysicianEntryType.Specializations.GetKey(),
                PhysicianEntryType.Specializations.GetDisplayName(),
                ValidatorFactory.List(
                    minCount: 1,
                    maxCount: CreatePhysicianCommand.MEDSPECMAXCOUNT,
                    itemValidator: ValidatorFactory.ValidEnum<MedicalSpecialization>(allowNull: false),
                    errorMessage: $"Physician must have 1-{CreatePhysicianCommand.MEDSPECMAXCOUNT} valid specializations"
                ));

        #endregion

        #region Miscellaneous

        public static ProfileEntry<string> CreateEmail()
            => CreateRequired(
                AdministratorEntryType.Email.GetKey(),
                AdministratorEntryType.Email.GetDisplayName(),
                ValidatorFactory.Email());

        #endregion
    }
}