using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Domain.Validation
{
    /// <summary>
    /// Contract for all validators
    /// </summary>
    /// <typeparam name="T">Type being validated</typeparam>
    public interface IValidator<T>
    {
        bool IsValid(T value);
        string ErrorMessage { get; }
    }
}
