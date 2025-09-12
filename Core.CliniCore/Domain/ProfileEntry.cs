namespace Core.CliniCore.Domain
{
    public abstract class ProfileEntry
    {
        protected ProfileEntry(string key, string displayName)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(displayName);

            Key = key;
            DisplayName = displayName;
        } 

        public string Key { get; protected set; }
        public string DisplayName { get; set; }
        public bool IsRequired { get; set; }
        public abstract Type ValueType { get; }
        public abstract bool IsValid { get; }
        public abstract string ErrorMessage { get; }
    }

    public class ProfileEntry<T> : ProfileEntry
    {
        private T _value = default!;
        private readonly Func<T, bool> _validator;
        private readonly string _validationErrorMessage;

        public ProfileEntry(string key, string displayName, bool isRequired = false,
            Func<T, bool>? validator = null, string? errorMessage = null)
            : base(key, displayName)
        {
            this.IsRequired = isRequired;
            // null coalescing to a validator that always returns true, this
            //  allows for entries that don't require validation (null object pattern)
            _validator = validator ?? (_ => true);
            _validationErrorMessage = errorMessage ?? $"ProfileEntry {DisplayName} null error message";
        }

        public T Value
        {
            get => _value;
            set
            {
                if (_validator(value))
                    _value = value;
                else
                    throw new ArgumentException(_validationErrorMessage);
            }
        }
        
        public override Type ValueType => typeof(T);

        public override bool IsValid => 
            _validator(_value);

        public override string ErrorMessage => 
            IsValid ? string.Empty : _validationErrorMessage;
    }
}