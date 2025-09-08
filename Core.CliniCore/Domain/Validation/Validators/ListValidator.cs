using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Validation.Validators
{
    public class ListValidator<T> : AbstractValidator<List<T>>
    {
        private readonly int? _minCount;
        private readonly int? _maxCount;
        private readonly IValidator<T>? _itemValidator;
        private readonly string _lastItemError = string.Empty;

        public ListValidator(
            int? minCount = null,
            int? maxCount = null,
            IValidator<T>? itemValidator = null,
            string? customErrorMessage = null)
            : base(customErrorMessage ?? BuildDefaultErrorMessage(minCount, maxCount))
        {
            _minCount = minCount;
            _maxCount = maxCount;
            _itemValidator = itemValidator;
        }

        public override bool IsValid(List<T> value)
        {
            if (value == null)
            {
                ErrorMessage = "List cannot be null";
                return false;
            }

            if (_minCount.HasValue && value.Count < _minCount)
            {
                ErrorMessage = $"Must have at least {_minCount} item(s)";
                return false;
            }

            if (_maxCount.HasValue && value.Count > _maxCount)
            {
                ErrorMessage = $"Must have at most {_maxCount} item(s)";
                return false;
            }

            if (_itemValidator != null)
            {
                for (int i = 0; i < value.Count; i++)
                {
                    if (!_itemValidator.IsValid(value[i]))
                    {
                        ErrorMessage = $"Item {i + 1}: {_itemValidator.ErrorMessage}";
                        return false;
                    }
                }
            }

            return true;
        }

        private static string BuildDefaultErrorMessage(int? min, int? max)
        {
            if (min.HasValue && max.HasValue)
                return $"List must contain {min}-{max} items";
            if (min.HasValue)
                return $"List must contain at least {min} item(s)";
            if (max.HasValue)
                return $"List must contain at most {max} item(s)";
            return "Invalid list (!)";
        }
    }
}
