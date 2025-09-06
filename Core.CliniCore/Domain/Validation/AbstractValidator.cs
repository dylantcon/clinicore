using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Validation
{
    /// <summary>
    /// Base class providing invariant validator infrastructure
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class AbstractValidator<T> : IValidator<T>
    {
        protected AbstractValidator(string errorMessage)
        {
            ErrorMessage = errorMessage ?? throw new ArgumentNullException(
                nameof(errorMessage));
        }

        public string ErrorMessage { get; protected set; }
        public abstract bool IsValid(T value);
    }
}
