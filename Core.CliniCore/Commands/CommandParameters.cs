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
                initialParameters ?? new Dictionary<string, object?>(),
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

            // Special handling for enums
            var targetType = typeof(T);
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;
            
            if (underlyingType.IsEnum)
            {
                try
                {
                    // If value is already an enum of the right type (boxed as object)
                    if (value.GetType() == underlyingType)
                        return (T)value;
                    
                    // Try to parse from string
                    if (value is string stringValue)
                        return (T)Enum.Parse(underlyingType, stringValue);
                    
                    // Try to convert from numeric value
                    return (T)Enum.ToObject(underlyingType, value);
                }
                catch
                {
                    return default;
                }
            }

            // Try to convert
            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch
            {
                return default;
            }
        }

        /// <summary>
        /// Gets a required parameter value (throws if not found or null)
        /// </summary>
        public T GetRequiredParameter<T>(string key)
        {
            var value = GetParameter<T>(key);
            if (value == null)
            {
                throw new ArgumentException($"Required parameter '{key}' is missing or null.");
            }
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
            return requiredKeys
                .Where(key => !HasValue(key))
                .Select(key => $"Missing required parameter: {key}")
                .ToList();
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