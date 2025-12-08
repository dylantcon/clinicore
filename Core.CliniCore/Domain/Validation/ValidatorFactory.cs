using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using Core.CliniCore.Domain.Validation.Validators;
using System.Linq.Expressions;
using Core.CliniCore.Service;

namespace Core.CliniCore.Domain.Validation
{
    /// <summary>
    /// Factory for creating validators
    /// </summary>
    public static class ValidatorFactory
    {
        #region Useful Constants

        private const string EMAIL_REGEX = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        private const string LICENSE_REGEX = @"^[A-Z0-9]{6,20}$";

        public const int MAX_PATIENT_AGE = 150;
        public const int MAX_GRAD_YEARS_ELAPSED = 75;

        public const int MIN_NAME_CHARS = 2;
        public const int MAX_NAME_CHARS = 100;
        public const int MIN_LICENSE_CHARS = 6;
        public const int MAX_LICENSE_CHARS = 20;

        // parameterized strings
        public static readonly string BIRTHDATE_SIZE_ERROR_STR = $"Birth date must be within last {MAX_PATIENT_AGE} years";
        public static readonly string GRADDATE_SIZE_ERROR_STR = $"Graduation date must be within last {MAX_GRAD_YEARS_ELAPSED}";
        public static readonly string PATIENT_NAME_SIZE_ERROR_STR = $"Patient name must be {MIN_NAME_CHARS}-{MAX_NAME_CHARS} characters";
        public static readonly string PHYSICIAN_NAME_SIZE_ERROR_STR = $"Physician name must be {MIN_NAME_CHARS}-{MAX_NAME_CHARS} characters";

        // constant strings
        public const string EMAIL_ERROR_STR = "Must be a valid email address";
        public const string LICENSE_ERROR_STR = "License number must be 6-20 alphanumeric characters";
        public const string PATIENT_NAME_NULL_ERROR_STR = "Patient name is required";
        public const string PHYSICIAN_NAME_NULL_ERROR_STR = "Physician name is required";
        public const string BIRTHDATE_NULL_ERROR_STR = "Birth date is required";

        #endregion

        #region Generic Validators

        public static IValidator<T> Required<T>(string? errorMessage = null)
            => new RequiredValidator<T>(errorMessage);

        public static IValidator<T> Composite<T>(params IValidator<T>[] validators)
            => new CompositeValidator<T>(validators);

        #endregion

        #region String Validators

        public static IValidator<string> StringLength(int? minLength = null, int? maxLength = null, string? errorMessage = null)
            => new StringLengthValidator(minLength, maxLength, errorMessage);

        public static IValidator<string> Regex(string pattern, string errorMessage, RegexOptions options = RegexOptions.None)
            => new RegexValidator(pattern, errorMessage, options);

        public static IValidator<string> Email()
            => Regex(EMAIL_REGEX, EMAIL_ERROR_STR);

        // would tie in with real medical physician credentials database
        public static IValidator<string> LicenseNumber()
            => Regex(LICENSE_REGEX, LICENSE_ERROR_STR);

        #endregion

        #region Date Validators

        public static IValidator<DateTime> DateRange(DateTime? minDate = null, DateTime? maxDate = null, string? errorMessage = null)
            => new DateRangeValidator(minDate, maxDate, errorMessage);

        public static IValidator<DateTime> BirthDate()
            => DateRange(DateTime.Now.AddYears(-MAX_PATIENT_AGE), DateTime.Now, BIRTHDATE_SIZE_ERROR_STR);

        public static IValidator<DateTime> GraduationDate()
            => DateRange(DateTime.Now.AddYears(-MAX_GRAD_YEARS_ELAPSED), DateTime.Now, GRADDATE_SIZE_ERROR_STR);

        public static IValidator<string> UsernameUniqueness(ProfileService profileRegistry, string? errorMessage = null)
            => new Validators.UsernameUniquenessValidator(profileRegistry, errorMessage);

        #endregion

        #region Domain-Specific Validators

        public static IValidator<string> PatientName()
            => Composite(
                Required<string>(PATIENT_NAME_NULL_ERROR_STR),
                StringLength(2, 100, PATIENT_NAME_SIZE_ERROR_STR)
            );

        public static IValidator<string> PhysicianName()
            => Composite(
                Required<string>(PHYSICIAN_NAME_NULL_ERROR_STR),
                StringLength(2, 100, PHYSICIAN_NAME_SIZE_ERROR_STR)
            );

        public static IValidator<DateTime> PatientBirthDate()
            => Composite(
                Required<DateTime>(BIRTHDATE_NULL_ERROR_STR),
                BirthDate()
            );

        #endregion

        #region Enum Validators

        /// <summary>
        /// Creates validator for a specific set of allowed values
        /// </summary>
        public static IValidator<T> OneOf<T>(IEnumerable<T> validValues, string? errorMessage = null, bool allowNull = true)
        {
            return new EnumValidator<T>(validValues, errorMessage, allowNull);
        }

        /// <summary>
        /// Creates validator for a specific set of allowed values (params overload)
        /// </summary>
        public static IValidator<T> OneOf<T>(string? errorMessage = null, bool allowNull = true, params T[] validValues)
        {
            return new EnumValidator<T>(validValues, errorMessage, allowNull);
        }

        /// <summary>
        /// Creates validator for actual C# enum types
        /// </summary>
        public static IValidator<T> ValidEnum<T>(string? errorMessage = null, bool allowNull = true) where T : struct, Enum
        {
            var enumValues = Enum.GetValues<T>();
            var customMessage = errorMessage ?? $"Must be a valid {typeof(T).Name}";
            return new EnumValidator<T>(enumValues, customMessage, allowNull);
        }

        /// <summary>
        /// Creates validator for nullable C# enum types
        /// </summary>
        public static IValidator<T?> ValidEnum<T>(string? errorMessage = null) where T : struct, Enum
        {
            var enumValues = Enum.GetValues<T>().Cast<T?>().Append(null);
            var customMessage = errorMessage ?? $"Must be a valid {typeof(T).Name} or null";
            return new EnumValidator<T?>(enumValues, customMessage, allowNull: true);
        }

        #endregion

        #region List Validation

        public static IValidator<List<T>> List<T>(
            int? minCount = null,
            int? maxCount = null,
            IValidator<T>? itemValidator = null,
            string? errorMessage = null)
        {
            return new ListValidator<T>(minCount, maxCount, itemValidator, errorMessage);
        }

        #endregion
    }
}
