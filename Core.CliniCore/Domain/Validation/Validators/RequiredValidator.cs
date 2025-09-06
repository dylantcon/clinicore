using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Validation.Validators
{
    public class RequiredValidator<T> : AbstractValidator<T>
    {
        public RequiredValidator(string? customErrorMessage = null)
            : base(customErrorMessage ?? "This field is required") { }

        public override bool IsValid(T value)
        {
            if (value == null) return false;

            // special handling for strings
            if (typeof(T) == typeof(string))
                return !string.IsNullOrWhiteSpace(value as string);

            // special handling for collections
            if (value is System.Collections.ICollection collection)
                return collection.Count > 0;

            // for value types, check against default
            return !EqualityComparer<T>.Default.Equals(value, default);
        }
    }
}
