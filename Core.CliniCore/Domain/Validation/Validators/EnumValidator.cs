using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Validation.Validators
{
    public class EnumValidator<T> : AbstractValidator<T>
    {
        private readonly HashSet<T> _validValues;
        private readonly bool _allowNull;

        public EnumValidator(IEnumerable<T> validValues, string? errorMessage = null, bool allowNull = true)
            : base(errorMessage ?? BuildDefaultErrorMessage(validValues))
        {
            _validValues = [.. validValues ?? throw new ArgumentNullException(nameof(validValues))];
            _allowNull = allowNull;

            if (_validValues.Count == 0)
                throw new ArgumentException("At least one valid value must be provided");
        }

        public override bool IsValid(T value)
        {
            if (value == null) return _allowNull;
            return _validValues.Contains(value);
        }

        private static string BuildDefaultErrorMessage(IEnumerable<T> validValues)
        {
            var values = validValues.Take(5).Select(v => v?.ToString() ?? "null");
            var valueString = string.Join(", ", values);
            var hasMore = validValues.Count() > 5;

            return hasMore
                ? $"Must be one of: {valueString}... (and {validValues.Count() - 5} others)"
                : $"Must be one of: {valueString}";
        }
    }
}
