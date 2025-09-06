using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Validation
{
    public class CompositeValidator<T> : IValidator<T>
    {
        private readonly List<IValidator<T>> _validators;
        private string _lastErrorMessage = string.Empty;

        public CompositeValidator(params IValidator<T>[] validators)
        {
            _validators = validators?.ToList() ?? throw new ArgumentNullException(
                nameof(validators));
        }

        public string ErrorMessage => _lastErrorMessage;

        public bool IsValid(T value)
        {
            foreach (var validator in _validators)
            {
                if (!validator.IsValid(value))
                {
                    _lastErrorMessage = validator.ErrorMessage;
                    return false;
                }
            }
            _lastErrorMessage = string.Empty;
            return true;
        }

        public CompositeValidator<T> And(IValidator<T> validator)
        {
            _validators.Add(validator);
            return this;
        }
    }
}
