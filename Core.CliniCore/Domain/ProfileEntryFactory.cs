using Core.CliniCore.Domain.Enumerations;
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
        #region Useful Constants

        // Generic person fields
        private const string NAME_KEY = "name";
        private const string NAME_DISP = "Full Name";

        private const string ADDRESS_KEY = "address";
        private const string ADDRESS_DISP = "Address";

        private const string BIRTHDATE_KEY = "birthdate";
        private const string BIRTHDATE_DISP = "Date of Birth";

        // Patient-specific fields
        private const string PATIENT_RACE_KEY = "patient_race";
        private const string RACE_DISP = "Race";

        private const string PATIENT_GENDER_KEY = "patient_gender";
        private const string GENDER_DISP = "Gender";

        // Physician-specific fields
        private const string PHYSICIAN_LICENSE_KEY = "physician_license";
        private const string LICENSE_DISP = "License Number";

        private const string PHYSICIAN_GRADDATE_KEY = "physician_graduation";
        private const string GRADDATE_DISP = "Graduation Date";

        private const string PHYSICIAN_SPEC_KEY = "physician_specializations";
        private const string SPEC_DISP = "Clinical Specializations";

        #endregion

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
            => CreateRequired(NAME_KEY, NAME_DISP, ValidatorFactory.Composite(
                ValidatorFactory.Required<string>("Name is required"),
                ValidatorFactory.StringLength(2, 100, "Name must be 2-100 characters")
            ));

        public static ProfileEntry<string> CreateAddress()
            => CreateRequired(ADDRESS_KEY, ADDRESS_DISP, ValidatorFactory.Composite(
                ValidatorFactory.Required<string>("Address is required"),
                ValidatorFactory.StringLength(maxLength: 200, errorMessage: "Address must be 200 characters or less")
            ));

        public static ProfileEntry<DateTime> CreateBirthDate()
            => CreateRequired(BIRTHDATE_KEY, BIRTHDATE_DISP, ValidatorFactory.Composite(
                ValidatorFactory.Required<DateTime>("Birth date is required"),
                ValidatorFactory.BirthDate()
            ));

        #endregion

        #region Patient-Specific Entries

        public static ProfileEntry<string> CreatePatientRace()
            => Create(PATIENT_RACE_KEY, RACE_DISP, ValidatorFactory.StringLength(
                maxLength: 50,
                errorMessage: "Race must be 50 characters or less"
            ));

        public static ProfileEntry<Gender> CreatePatientGender()
            => CreateRequired(PATIENT_GENDER_KEY, GENDER_DISP,
                ValidatorFactory.ValidEnum<Gender>(allowNull: false));

        #endregion

        #region Physician-Specific Entries

        public static ProfileEntry<string> CreatePhysicianLicenseNumber()
            => CreateRequired(PHYSICIAN_LICENSE_KEY, LICENSE_DISP, ValidatorFactory.Composite(
                ValidatorFactory.Required<string>(ValidatorFactory.LICENSE_ERROR_STR),
                ValidatorFactory.LicenseNumber()
            ));

        public static ProfileEntry<DateTime> CreatePhysicianGraduationDate()
            => CreateRequired(PHYSICIAN_GRADDATE_KEY, GRADDATE_DISP, ValidatorFactory.Composite(
                ValidatorFactory.Required<DateTime>(ValidatorFactory.GRADDATE_SIZE_ERROR_STR),
                ValidatorFactory.GraduationDate()
            ));

        public static ProfileEntry<List<MedicalSpecialization>> CreatePhysicianSpecializationList()
            => CreateRequired(PHYSICIAN_SPEC_KEY, SPEC_DISP,
                ValidatorFactory.List(
                    minCount: 1,
                    maxCount: 3,
                    itemValidator: ValidatorFactory.ValidEnum<MedicalSpecialization>(allowNull: false),
                    errorMessage: "Physician must have 1-3 valid specializations"
                ));

        #endregion

        #region Miscellaneous

        public static ProfileEntry<string> CreateEmail()
            => CreateRequired("user_email", "Email",
                ValidatorFactory.Email());

        #endregion
    }
}