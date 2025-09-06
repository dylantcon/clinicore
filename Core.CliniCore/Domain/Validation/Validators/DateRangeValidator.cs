using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Validation.Validators
{
    public class DateRangeValidator : AbstractValidator<DateTime>
    {
        private readonly DateTime? _minDate;
        private readonly DateTime? _maxDate;

        public DateRangeValidator(DateTime? minDate = null, DateTime? maxDate = null, string? customErrorMessage = null)
            : base(customErrorMessage ?? BuildErrorMessage(minDate, maxDate))
        {
            _minDate = minDate;
            _maxDate = maxDate;
        }

        public override bool IsValid(DateTime value)
        {
            if (value == default) return true;

            if (_minDate.HasValue && value < _minDate) return false;
            if (_maxDate.HasValue && value > _maxDate) return false;

            return true;
        }

        private static string BuildErrorMessage(DateTime? minDate, DateTime? maxDate)
        {
            if (minDate.HasValue && maxDate.HasValue)
                return $"Must be between {minDate:yyyy-MM-dd} and {maxDate:yyyy-MM-dd}";
            if (minDate.HasValue)
                return $"Must be after {minDate:yyyy-MM-dd}";
            if (maxDate.HasValue)
                return $"Must be before {maxDate:yyyy-MM-dd}";
            return "Invalid date range";
        }
    }
}
