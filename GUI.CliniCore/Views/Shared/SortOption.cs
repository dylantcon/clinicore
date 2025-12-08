using System.Collections;

namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// Abstract base class for sort options. Used for XAML binding with Picker.ItemsSource.
/// </summary>
public abstract class SortOptionBase
{
    /// <summary>
    /// Display name shown in the sort picker.
    /// </summary>
    public abstract string DisplayName { get; }

    /// <summary>
    /// Applies sorting to the collection.
    /// </summary>
    /// <param name="items">The items to sort</param>
    /// <param name="ascending">True for ascending, false for descending</param>
    /// <returns>The sorted collection</returns>
    public abstract IEnumerable<object> Apply(IEnumerable items, bool ascending);

    public override string ToString() => DisplayName;
}

/// <summary>
/// Generic sort option using compile-time safe delegate-based key selector.
/// Provides type-safe sorting without reflection.
/// </summary>
/// <typeparam name="T">The type of items being sorted</typeparam>
public class SortOption<T> : SortOptionBase
{
    private readonly string _displayName;
    private readonly Func<T, object> _keySelector;

    /// <summary>
    /// Creates a new sort option.
    /// </summary>
    /// <param name="displayName">Display name shown in the sort picker</param>
    /// <param name="keySelector">Delegate to extract the sort key from an item</param>
    public SortOption(string displayName, Func<T, object> keySelector)
    {
        _displayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
        _keySelector = keySelector ?? throw new ArgumentNullException(nameof(keySelector));
    }

    /// <summary>
    /// Display name shown in the sort picker.
    /// </summary>
    public override string DisplayName => _displayName;

    /// <summary>
    /// The delegate used to extract the sort key.
    /// </summary>
    public Func<T, object> KeySelector => _keySelector;

    /// <summary>
    /// Applies sorting to the collection using the delegate-based key selector.
    /// </summary>
    public override IEnumerable<object> Apply(IEnumerable items, bool ascending)
    {
        if (items == null) return Enumerable.Empty<object>();

        var typed = items.OfType<T>().ToList();
        if (!typed.Any()) return items.Cast<object>();

        var sorted = ascending
            ? typed.OrderBy(_keySelector)
            : typed.OrderByDescending(_keySelector);

        return sorted.Cast<object>();
    }
}
