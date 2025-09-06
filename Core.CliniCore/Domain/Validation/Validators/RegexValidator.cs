using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace Core.CliniCore.Domain.Validation.Validators
{
    public class RegexValidator : AbstractValidator<string>
    {
        private readonly Regex _regex;

        public RegexValidator(string pattern, string errorMessage, RegexOptions options = RegexOptions.None)
            : base(errorMessage)
        {
            _regex = new Regex(pattern, options);
        }

        public override bool IsValid(string value)
        {
            if (string.IsNullOrEmpty(value)) return true;
            return _regex.IsMatch(value);
        }
    }
}
