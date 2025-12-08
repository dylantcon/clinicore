using System.Collections;

namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// A styled picker component with drop shadow, darker clickable region,
/// and italicized placeholder text for improved UX.
/// </summary>
public partial class StyledPicker : ContentView
{
    #region Bindable Properties

    public static readonly BindableProperty LabelTextProperty = BindableProperty.Create(
        nameof(LabelText),
        typeof(string),
        typeof(StyledPicker),
        string.Empty);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(StyledPicker),
        "Select an option...");

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IList),
        typeof(StyledPicker),
        null);

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem),
        typeof(object),
        typeof(StyledPicker),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedItemChanged);

    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex),
        typeof(int),
        typeof(StyledPicker),
        -1,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedIndexChanged);

    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(StyledPicker),
        false,
        propertyChanged: OnIsRequiredChanged);

    public static readonly BindableProperty ItemDisplayPathProperty = BindableProperty.Create(
        nameof(ItemDisplayPath),
        typeof(string),
        typeof(StyledPicker),
        null,
        propertyChanged: OnItemDisplayPathChanged);

    #endregion

    #region Properties

    public string LabelText
    {
        get => (string)GetValue(LabelTextProperty);
        set => SetValue(LabelTextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public IList? ItemsSource
    {
        get => (IList?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public object? SelectedItem
    {
        get => GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    public string? ItemDisplayPath
    {
        get => (string?)GetValue(ItemDisplayPathProperty);
        set => SetValue(ItemDisplayPathProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler<EventArgs>? SelectionChanged;

    #endregion

    public StyledPicker()
    {
        InitializeComponent();
        UpdatePlaceholderVisibility();
        // Do NOT set FieldLabel.Text here -- leave the binding to LabelText intact
    }

    /// <summary>
    /// Opens the picker when the field label is tapped (like HTML label-for behavior).
    /// </summary>
    private void OnLabelTapped(object? sender, TappedEventArgs e)
    {
        InnerPicker.Focus();
    }

    private void OnSelectionChanged(object? sender, EventArgs e)
    {
        UpdatePlaceholderVisibility();
        SelectionChanged?.Invoke(this, e);
    }

    private void UpdatePlaceholderVisibility()
    {
        // Show placeholder when nothing is selected
        PlaceholderLabel.IsVisible = SelectedIndex < 0 && SelectedItem == null;
    }

    private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledPicker picker)
        {
            picker.UpdatePlaceholderVisibility();
        }
    }

    private static void OnSelectedIndexChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledPicker picker)
        {
            picker.UpdatePlaceholderVisibility();
        }
    }

    private static void OnIsRequiredChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledPicker picker)
        {
            // Modify the underlying LabelText so the XAML binding updates the displayed text
            var isRequired = newValue is bool b && b;
            var text = picker.LabelText ?? string.Empty;
            if (isRequired)
            {
                if (!text.EndsWith(" *"))
                    picker.LabelText = text + " *";
            }
            else
            {
                if (text.EndsWith(" *"))
                    picker.LabelText = text.Substring(0, text.Length - 2);
            }
        }
    }

    private static void OnItemDisplayPathChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledPicker picker && newValue is string path && !string.IsNullOrEmpty(path))
        {
            picker.InnerPicker.ItemDisplayBinding = new Binding(path);
        }
    }
}
