namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// Represents a single metric/statistic item for display in a SummaryCard.
/// Used to display icon + value + label combinations like "5 Patients" or "12 Appointments".
/// </summary>
public class SummaryItem
{
    /// <summary>
    /// MaterialIcons font glyph for the item (e.g., MaterialIcons.Group).
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// Color of the icon (e.g., green for patients, blue for appointments).
    /// </summary>
    public Color IconColor { get; set; } = Colors.Gray;

    /// <summary>
    /// The bold metric value (e.g., "5", "12").
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// The descriptive label following the value (e.g., "Patients", "Appointments").
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// Optional click command for the item.
    /// </summary>
    public System.Windows.Input.ICommand? TapCommand { get; set; }

    /// <summary>
    /// Creates a new SummaryItem with all properties.
    /// </summary>
    public SummaryItem(string icon, Color iconColor, string value, string label, System.Windows.Input.ICommand? tapCommand = null)
    {
        Icon = icon;
        IconColor = iconColor;
        Value = value;
        Label = label;
        TapCommand = tapCommand;
    }

    /// <summary>
    /// Default constructor for XAML/binding scenarios.
    /// </summary>
    public SummaryItem() { }
}
