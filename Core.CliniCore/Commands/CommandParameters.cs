namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Container for passing parameters to commands in a standardized way
    /// </summary>
    public class CommandParameters
    {
        private readonly Dictionary<string, object?> _parameters;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParameters"/> class with no initial parameters.
        /// </summary>
        public CommandParameters()
        {
            _parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandParameters"/> class with the specified initial parameters.
        /// </summary>
        /// <param name="initialParameters">The initial parameter values to populate the collection with.</param>
        public CommandParameters(Dictionary<string, object?> initialParameters)
        {
            _parameters = new Dictionary<string, object?>(
                initialParameters ?? [],
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets a parameter value by key.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <returns>The parameter value, or <c>null</c> if the key is not found.</returns>
        public object? this[string key]
        {
            get => GetParameter(key);
            set => SetParameter(key, value);
        }

        /// <summary>
        /// Sets a parameter value.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>The current <see cref="CommandParameters"/> instance for method chaining.</returns>
        public CommandParameters SetParameter(string key, object? value)
        {
            _parameters[key] = value;
            return this; // Fluent interface
        }

        /// <summary>
        /// Gets a parameter value as an untyped object.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <returns>The parameter value, or <c>null</c> if the key is not present.</returns>
        public object? GetParameter(string key)
        {
            return _parameters.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Gets a typed parameter value.
        /// </summary>
        /// <typeparam name="T">The expected parameter type.</typeparam>
        /// <param name="key">The parameter key.</param>
        /// <returns>The parameter value converted to <typeparamref name="T"/>, or the default value of <typeparamref name="T"/> if not found or not convertible.</returns>
        public T? GetParameter<T>(string key)
        {
            var value = GetParameter(key);
            if (value == null)
                return default;

            if (value is T typedValue)
                return typedValue;

            var targetType = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            // Handle collections of enums separately
            if (IsEnumCollection(underlyingType))
            {
                return TryConvertToEnumCollection<T>(value, underlyingType);
            }

            // Special handling for single enums
            if (underlyingType.IsEnum)
            {
                return TryConvertToEnum<T>(value, underlyingType);
            }

            // Try standard conversion
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        private static bool IsEnumCollection(Type type)
        {
            if (!type.IsGenericType)
                return false;

            var genericDef = type.GetGenericTypeDefinition();
            if (genericDef != typeof(List<>) && genericDef != typeof(IList<>) && genericDef != typeof(IEnumerable<>))
                return false;

            var elementType = type.GetGenericArguments()[0];
            return elementType.IsEnum;
        }

        private static T? TryConvertToEnumCollection<T>(object value, Type targetType)
        {
            try
            {
                var elementType = targetType.GetGenericArguments()[0];

                if (value is System.Collections.IEnumerable enumerable)
                {
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    var list = Activator.CreateInstance(listType);
                    var addMethod = listType.GetMethod("Add");

                    foreach (var item in enumerable)
                    {
                        if (item == null) continue;

                        object? enumValue = ConvertToEnumValue(item, elementType);
                        if (enumValue != null)
                        {
                            addMethod?.Invoke(list, [enumValue]);
                        }
                    }

                    return (T?)list;
                }
            }
            catch
            {
                // Fall through to return default
            }

            return default;
        }

        private static T? TryConvertToEnum<T>(object value, Type enumType)
        {
            try
            {
                object? enumValue = ConvertToEnumValue(value, enumType);
                return enumValue != null ? (T?)enumValue : default;
            }
            catch
            {
                return default;
            }
        }

        private static object? ConvertToEnumValue(object value, Type enumType)
        {
            try
            {
                // If already the right enum type
                if (value.GetType() == enumType)
                    return value;

                // Try to parse from string
                if (value is string stringValue)
                    return Enum.Parse(enumType, stringValue);

                // Try to convert from numeric value
                return Enum.ToObject(enumType, value);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a required parameter value and throws an exception if it is missing or <c>null</c>.
        /// </summary>
        /// <typeparam name="T">The expected parameter type.</typeparam>
        /// <param name="key">The parameter key.</param>
        /// <returns>The parameter value converted to <typeparamref name="T"/>.</returns>
        /// <exception cref="ArgumentException">Thrown when the parameter is missing or <c>null</c>.</exception>
        public T GetRequiredParameter<T>(string key)
        {
            var value = GetParameter<T>(key) ?? 
                throw new ArgumentException($"Required parameter '{key}' is missing or null.");
            return value;
        }

        /// <summary>
        /// Determines whether the specified parameter key exists.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <returns><c>true</c> if the key exists; otherwise, <c>false</c>.</returns>
        public bool HasParameter(string key)
        {
            return _parameters.ContainsKey(key);
        }

        /// <summary>
        /// Determines whether the specified parameter key exists and has a non-<c>null</c> value.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <returns><c>true</c> if the key exists and has a value; otherwise, <c>false</c>.</returns>
        public bool HasValue(string key)
        {
            return _parameters.TryGetValue(key, out var value) && value != null;
        }

        /// <summary>
        /// Removes the parameter with the specified key.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <returns><c>true</c> if the parameter was removed; otherwise, <c>false</c>.</returns>
        public bool RemoveParameter(string key)
        {
            return _parameters.Remove(key);
        }

        /// <summary>
        /// Clears all parameters from the collection.
        /// </summary>
        public void Clear()
        {
            _parameters.Clear();
        }

        /// <summary>
        /// Gets all parameter keys.
        /// </summary>
        /// <returns>An enumerable collection of parameter keys.</returns>
        public IEnumerable<string> GetKeys()
        {
            return _parameters.Keys;
        }

        /// <summary>
        /// Gets the number of parameters in the collection.
        /// </summary>
        public int Count => _parameters.Count;

        /// <summary>
        /// Creates a shallow copy of this <see cref="CommandParameters"/> instance.
        /// </summary>
        /// <returns>A new <see cref="CommandParameters"/> instance containing the same parameter values.</returns>
        public CommandParameters Clone()
        {
            return new CommandParameters(new Dictionary<string, object?>(_parameters));
        }

        /// <summary>
        /// Merges another set of parameters into this instance, overwriting existing values with the same keys.
        /// </summary>
        /// <param name="other">The other parameter collection to merge.</param>
        /// <returns>The current <see cref="CommandParameters"/> instance for method chaining.</returns>
        public CommandParameters Merge(CommandParameters other)
        {
            if (other != null)
            {
                foreach (var key in other.GetKeys())
                {
                    _parameters[key] = other.GetParameter(key);
                }
            }
            return this;
        }

        /// <summary>
        /// Validates that all required parameter keys are present and have non-<c>null</c> values.
        /// </summary>
        /// <param name="requiredKeys">The parameter keys that are required.</param>
        /// <returns><c>true</c> if all required parameters are present; otherwise, <c>false</c>.</returns>
        public bool ValidateRequired(params string[] requiredKeys)
        {
            return requiredKeys.All(key => HasValue(key));
        }

        /// <summary>
        /// Gets validation error messages for any missing required parameters.
        /// </summary>
        /// <param name="requiredKeys">The parameter keys that are required.</param>
        /// <returns>A list of error messages describing missing parameters.</returns>
        public List<string> GetMissingRequired(params string[] requiredKeys)
        {
            return [.. requiredKeys
                .Where(key => !HasValue(key))
                .Select(key => $"Missing required parameter: {key}")];
        }

        /// <summary>
        /// Creates a new, empty <see cref="CommandParameters"/> instance.
        /// </summary>
        /// <returns>A new <see cref="CommandParameters"/> object.</returns>
        public static CommandParameters Create()
        {
            return new CommandParameters();
        }

        /// <summary>
        /// Creates a new <see cref="CommandParameters"/> instance initialized with a single parameter.
        /// </summary>
        /// <param name="key">The parameter key.</param>
        /// <param name="value">The parameter value.</param>
        /// <returns>A new <see cref="CommandParameters"/> object containing the specified key and value.</returns>
        public static CommandParameters Create(string key, object? value)
        {
            return new CommandParameters().SetParameter(key, value);
        }

        /// <summary>
        /// Returns a string that represents the current <see cref="CommandParameters"/>.
        /// </summary>
        /// <returns>A formatted string containing all parameter key-value pairs.</returns>
        public override string ToString()
        {
            var paramStrings = _parameters.Select(kvp => $"{kvp.Key}={kvp.Value ?? "null"}");
            return $"CommandParameters[{string.Join(", ", paramStrings)}]";
        }
    }
}