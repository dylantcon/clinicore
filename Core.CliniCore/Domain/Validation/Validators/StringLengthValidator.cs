using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Validation.Validators
{
    public class StringLengthValidator : AbstractValidator<string>
    {
        private readonly int? _minLength;
        private readonly int? _maxLength;

        public StringLengthValidator(int? minLength = null, int? maxLength = null, string? customErrorMessage = null)
            : base(customErrorMessage ?? BuildErrorMessage(minLength, maxLength))
        {
            _minLength = minLength;
            _maxLength = maxLength;
        }

        public override bool IsValid(string value)
        {
            if (string.IsNullOrEmpty(value)) return true;

            if (_minLength.HasValue && value.Length < _minLength) return false;
            if (_maxLength.HasValue && value.Length > _maxLength) return false;

            return true;
        }

        private static string BuildErrorMessage(int? minLength, int? maxLength)
        {
            if (minLength.HasValue && maxLength.HasValue)
                return $"Must be between {minLength} and {maxLength} characters";
            if (minLength.HasValue)
                return $"Must be at least {minLength} characters";
            if (maxLength.HasValue)
                return $"Must be at most {maxLength} characters";
            return "Invalid length";
        }
    }
}
