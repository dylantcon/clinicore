using Core.CliniCore.Commands;
using System.Reflection;

namespace GUI.CliniCore.Commands
{
    /// <summary>
    /// Converts MAUI command parameters (object?) to Core.CliniCore CommandParameters
    /// Handles various parameter formats from XAML bindings
    /// </summary>
    public static class CommandParameterConverter
    {
        /// <summary>
        /// Converts a MAUI command parameter to CommandParameters
        /// </summary>
        public static CommandParameters Convert(object? parameter)
        {
            if (parameter == null)
                return new CommandParameters();

            // Handle string parameters
            if (parameter is string stringValue)
            {
                return new CommandParameters().SetParameter("value", stringValue);
            }

            // Handle dictionary parameters
            if (parameter is Dictionary<string, object?> dict)
            {
                return new CommandParameters(dict);
            }

            // Handle tuple for key-value pair
            if (parameter is ValueTuple<string, object?> tuple)
            {
                return new CommandParameters().SetParameter(tuple.Item1, tuple.Item2);
            }

            // Handle ViewModel as parameter (reflection-based)
            if (parameter.GetType().Namespace?.StartsWith("GUI.CliniCore") == true)
            {
                return ConvertFromViewModel(parameter);
            }

            // Default: store as "value" parameter
            return new CommandParameters().SetParameter("value", parameter);
        }

        /// <summary>
        /// Extracts properties from a ViewModel and creates CommandParameters
        /// Useful when CommandParameter="{Binding .}" is used
        /// </summary>
        private static CommandParameters ConvertFromViewModel(object viewModel)
        {
            var commandParams = new CommandParameters();
            var type = viewModel.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in properties)
            {
                // Skip properties we don't want to include
                if (ShouldSkipProperty(prop.Name))
                    continue;

                try
                {
                    var value = prop.GetValue(viewModel);
                    if (value != null)
                    {
                        // Convert property name to lowercase for consistency
                        var key = char.ToLowerInvariant(prop.Name[0]) + prop.Name.Substring(1);
                        commandParams.SetParameter(key, value);
                    }
                }
                catch
                {
                    // Skip properties that can't be read
                }
            }

            return commandParams;
        }

        private static bool ShouldSkipProperty(string propertyName)
        {
            return propertyName == nameof(ViewModels.Base.BaseViewModel.ValidationErrors) ||
                   propertyName == nameof(ViewModels.Base.BaseViewModel.ValidationWarnings) ||
                   propertyName == nameof(ViewModels.Base.BaseViewModel.HasValidationErrors) ||
                   propertyName == nameof(ViewModels.Base.BaseViewModel.HasValidationWarnings) ||
                   propertyName == nameof(ViewModels.Base.BaseViewModel.IsBusy) ||
                   propertyName == nameof(ViewModels.Base.BaseViewModel.Title) ||
                   propertyName.EndsWith("Command"); // Skip command properties
        }
    }
}
