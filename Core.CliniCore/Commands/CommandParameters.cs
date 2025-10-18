using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.CliniCore.Commands
{
    /// <summary>
    /// Container for passing parameters to commands in a standardized way
    /// </summary>
    public class CommandParameters
    {
        private readonly Dictionary<string, object?> _parameters;

        public CommandParameters()
        {
            _parameters = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }

        public CommandParameters(Dictionary<string, object?> initialParameters)
        {
            _parameters = new Dictionary<string, object?>(
                initialParameters ?? [],
                StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets or sets a parameter value
        /// </summary>
        public object? this[string key]
        {
            get => GetParameter(key);
            set => SetParameter(key, value);
        }

        /// <summary>
        /// Sets a parameter value
        /// </summary>
        public CommandParameters SetParameter(string key, object? value)
        {
            _parameters[key] = value;
            return this; // Fluent interface
        }

        /// <summary>
        /// Gets a parameter value
        /// </summary>
        public object? GetParameter(string key)
        {
            return _parameters.TryGetValue(key, out var value) ? value : null;
        }

        /// <summary>
        /// Gets a typed parameter value
        /// </summary>
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
        /// Gets a required parameter value (throws if not found or null)
        /// </summary>
        public T GetRequiredParameter<T>(string key)
        {
            var value = GetParameter<T>(key) ?? 
                throw new ArgumentException($"Required parameter '{key}' is missing or null.");
            return value;
        }

        /// <summary>
        /// Checks if a parameter exists
        /// </summary>
        public bool HasParameter(string key)
        {
            return _parameters.ContainsKey(key);
        }

        /// <summary>
        /// Checks if a parameter exists and is not null
        /// </summary>
        public bool HasValue(string key)
        {
            return _parameters.TryGetValue(key, out var value) && value != null;
        }

        /// <summary>
        /// Removes a parameter
        /// </summary>
        public bool RemoveParameter(string key)
        {
            return _parameters.Remove(key);
        }

        /// <summary>
        /// Clears all parameters
        /// </summary>
        public void Clear()
        {
            _parameters.Clear();
        }

        /// <summary>
        /// Gets all parameter keys
        /// </summary>
        public IEnumerable<string> GetKeys()
        {
            return _parameters.Keys;
        }

        /// <summary>
        /// Gets the count of parameters
        /// </summary>
        public int Count => _parameters.Count;

        /// <summary>
        /// Creates a copy of these parameters
        /// </summary>
        public CommandParameters Clone()
        {
            return new CommandParameters(new Dictionary<string, object?>(_parameters));
        }

        /// <summary>
        /// Merges another set of parameters into this one
        /// </summary>
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
        /// Validates that required parameters are present
        /// </summary>
        public bool ValidateRequired(params string[] requiredKeys)
        {
            return requiredKeys.All(key => HasValue(key));
        }

        /// <summary>
        /// Gets validation errors for missing required parameters
        /// </summary>
        public List<string> GetMissingRequired(params string[] requiredKeys)
        {
            return [.. requiredKeys
                .Where(key => !HasValue(key))
                .Select(key => $"Missing required parameter: {key}")];
        }

        /// <summary>
        /// Builder method for fluent interface
        /// </summary>
        public static CommandParameters Create()
        {
            return new CommandParameters();
        }

        /// <summary>
        /// Builder method with initial parameter
        /// </summary>
        public static CommandParameters Create(string key, object? value)
        {
            return new CommandParameters().SetParameter(key, value);
        }

        public override string ToString()
        {
            var paramStrings = _parameters.Select(kvp => $"{kvp.Key}={kvp.Value ?? "null"}");
            return $"CommandParameters[{string.Join(", ", paramStrings)}]";
        }
    }
}