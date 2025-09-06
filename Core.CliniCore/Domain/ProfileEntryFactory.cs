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

        private const string PATIENT_NAME_KEY = "patient_name";
        private const string NAME_DISP = "Full Name";

        private const string PATIENT_ADDRESS_KEY = "patient_address";
        private const string ADDRESS_DISP = "Address";

        private const string PATIENT_BIRTHDATE_KEY = "patient_birthdate";
        private const string BIRTHDATE_DISP = "Birthdate";

        private const string PATIENT_RACE_KEY = "patient_race";
        private const string RACE_DISP = "Race";

        private const string PATIENT_GENDER_KEY = "patient_gender";
        private const string GENDER_DISP = "Gender";

        private const string PHYSICIAN_NAME_KEY = "physician_name";

        private const string PHYSICIAN_LICENSE_KEY = "physician_license";
        private const string PHYSICIAN_LICENSE_DISP = "License Number";

        private const string PHYSICIAN_GRADDATE_KEY = "physician_graduation";
        private const string PHYSICIAN_GRADDATE_DISP = "Graduation Date";

        private const string PHYSICIAN_SPEC_KEY = "physician_specializations";
        private const string PHYSICIAN_SPEC_DISP = "Clinical Specializations";

        #endregion

        #region Generic Factory Methods

        public static ProfileEntry<T> Create<T>(string key, string displayName,
            IValidator<T>? validator = null, bool isRequired = false)
        {
            // convert IValidator<T> to Func<T, bool> for ProfileEntry constructor
            // we need to implicitly handle the null case
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

        #region Patient Profile Entries

        public static ProfileEntry<string> CreatePatientName()
            => Create(PATIENT_NAME_KEY, NAME_DISP, ValidatorFactory.PatientName());

        public static ProfileEntry<string> CreatePatientAddress()
            => Create(PATIENT_ADDRESS_KEY, ADDRESS_DISP, ValidatorFactory.Composite(
                    ValidatorFactory.Required<string>("Address is required"),
                    ValidatorFactory.StringLength(maxLength: 200, errorMessage: "Address must be 200 characters or less")
            ));

        public static ProfileEntry<DateTime> CreatePatientBirthDate()
            => Create(PATIENT_BIRTHDATE_KEY, BIRTHDATE_DISP, ValidatorFactory.PatientBirthDate());

        public static ProfileEntry<string> CreatePatientRace()
            => Create(PATIENT_RACE_KEY, RACE_DISP, ValidatorFactory.StringLength(
                maxLength: 50,
                errorMessage: "Race must be 40 characters or less"
            ));

        public static ProfileEntry<string> CreatePatientGender()
            => Create(PATIENT_GENDER_KEY, GENDER_DISP, ValidatorFactory.PatientGender());

        #endregion

        #region Physician Profile Entries

        public static ProfileEntry<string> CreatePhysicianName()
            => Create(PHYSICIAN_NAME_KEY, NAME_DISP, ValidatorFactory.PhysicianName());

        public static ProfileEntry<string> CreatePhysicianLicenseNumber()
            => Create(PHYSICIAN_LICENSE_KEY, PHYSICIAN_LICENSE_DISP, ValidatorFactory.Composite(
                    ValidatorFactory.Required<string>(ValidatorFactory.LICENSE_ERROR_STR),
                    ValidatorFactory.LicenseNumber()
            ));

        public static ProfileEntry<DateTime> CreatePhysicianGraduationDate()
            => Create(PHYSICIAN_GRADDATE_KEY, PHYSICIAN_GRADDATE_DISP, ValidatorFactory.Composite(
                    ValidatorFactory.Required<DateTime>(ValidatorFactory.GRADDATE_SIZE_ERROR_STR),
                    ValidatorFactory.GraduationDate()
            ));

        #endregion


    }
}