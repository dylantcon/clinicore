using Core.CliniCore.Domain.Enumerations;

namespace GUI.CliniCore.Helpers;

/// <summary>
/// Helper class for mapping medical specializations to colors.
/// Used in CalendarView to color-code appointments by physician specialization.
/// </summary>
public static class SpecializationColorHelper
{
    private static readonly Dictionary<MedicalSpecialization, Color> SpecializationColors = new()
    {
        { MedicalSpecialization.Emergency, Color.FromArgb("#FF5722") },          // Deep Orange
        { MedicalSpecialization.FamilyMedicine, Color.FromArgb("#8BC34A") },     // Light Green
        { MedicalSpecialization.InternalMedicine, Color.FromArgb("#009688") },   // Teal
        { MedicalSpecialization.Pediatrics, Color.FromArgb("#03A9F4") },         // Light Blue
        { MedicalSpecialization.ObstetricsGynecology, Color.FromArgb("#F48FB1") }, // Pink
        { MedicalSpecialization.Surgery, Color.FromArgb("#795548") },            // Brown
        { MedicalSpecialization.Orthopedics, Color.FromArgb("#FF9800") },        // Orange
        { MedicalSpecialization.Cardiology, Color.FromArgb("#F44336") },         // Red
        { MedicalSpecialization.Neurology, Color.FromArgb("#9C27B0") },          // Purple
        { MedicalSpecialization.Oncology, Color.FromArgb("#673AB7") },           // Deep Purple
        { MedicalSpecialization.Radiology, Color.FromArgb("#2196F3") },          // Blue
        { MedicalSpecialization.Anesthesiology, Color.FromArgb("#00BCD4") },     // Cyan
        { MedicalSpecialization.Psychiatry, Color.FromArgb("#607D8B") },         // Blue Gray
        { MedicalSpecialization.Dermatology, Color.FromArgb("#E91E63") },        // Pink
        { MedicalSpecialization.Ophthalmology, Color.FromArgb("#3F51B5") }       // Indigo
    };

    /// <summary>
    /// Gets the color for a single specialization.
    /// </summary>
    public static Color GetColor(MedicalSpecialization specialization)
    {
        return SpecializationColors.GetValueOrDefault(specialization, Color.FromArgb("#9E9E9E"));
    }

    /// <summary>
    /// Gets an average color for multiple specializations.
    /// </summary>
    public static Color GetAverageColor(IEnumerable<MedicalSpecialization>? specializations)
    {
        if (specializations == null || !specializations.Any())
            return Color.FromArgb("#9E9E9E"); // Default gray

        var colors = specializations
            .Select(s => SpecializationColors.GetValueOrDefault(s, Colors.Gray))
            .ToList();

        var avgR = colors.Average(c => c.Red);
        var avgG = colors.Average(c => c.Green);
        var avgB = colors.Average(c => c.Blue);

        return Color.FromRgba(avgR, avgG, avgB, 1.0);
    }

    /// <summary>
    /// Gets a contrasting text color (black or white) based on background luminance.
    /// Uses ITU-R BT.709 formula for relative luminance.
    /// </summary>
    public static Color GetContrastingTextColor(Color background)
    {
        var luminance = 0.299 * background.Red + 0.587 * background.Green + 0.114 * background.Blue;
        return luminance > 0.5 ? Colors.Black : Colors.White;
    }

    /// <summary>
    /// Gets a color for an appointment status.
    /// </summary>
    public static Color GetStatusColor(AppointmentStatus status)
    {
        return status switch
        {
            AppointmentStatus.Scheduled => Color.FromArgb("#2196F3"),    // Blue
            AppointmentStatus.Completed => Color.FromArgb("#4CAF50"),    // Green
            AppointmentStatus.Cancelled => Color.FromArgb("#9E9E9E"),    // Gray
            AppointmentStatus.NoShow => Color.FromArgb("#F44336"),       // Red
            AppointmentStatus.InProgress => Color.FromArgb("#FF9800"),   // Orange
            _ => Color.FromArgb("#9E9E9E")
        };
    }
}
