namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// A reusable ContentView for styled form entry fields.
/// Combines a label and entry with consistent styling.
/// </summary>
public partial class StyledEntryField : ContentView
{
    #region Bindable Properties

    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label),
        typeof(string),
        typeof(StyledEntryField),
        string.Empty,
        propertyChanged: OnLabelChanged);

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(StyledEntryField),
        string.Empty,
        BindingMode.TwoWay,
        propertyChanged: OnTextPropertyChanged);

    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(
        nameof(Placeholder),
        typeof(string),
        typeof(StyledEntryField),
        string.Empty,
        propertyChanged: OnPlaceholderChanged);

    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(StyledEntryField),
        false,
        propertyChanged: OnIsRequiredChanged);

    public static readonly BindableProperty KeyboardProperty = BindableProperty.Create(
        nameof(Keyboard),
        typeof(Keyboard),
        typeof(StyledEntryField),
        Keyboard.Default,
        propertyChanged: OnKeyboardChanged);

    public static readonly BindableProperty MaxLengthProperty = BindableProperty.Create(
        nameof(MaxLength),
        typeof(int),
        typeof(StyledEntryField),
        int.MaxValue,
        propertyChanged: OnMaxLengthChanged);

    public static readonly BindableProperty IsPasswordProperty = BindableProperty.Create(
        nameof(IsPassword),
        typeof(bool),
        typeof(StyledEntryField),
        false,
        propertyChanged: OnIsPasswordChanged);

    public static readonly BindableProperty IsReadOnlyProperty = BindableProperty.Create(
        nameof(IsReadOnly),
        typeof(bool),
        typeof(StyledEntryField),
        false,
        propertyChanged: OnIsReadOnlyChanged);

    #endregion

    #region Properties

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    public Keyboard Keyboard
    {
        get => (Keyboard)GetValue(KeyboardProperty);
        set => SetValue(KeyboardProperty, value);
    }

    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    public bool IsPassword
    {
        get => (bool)GetValue(IsPasswordProperty);
        set => SetValue(IsPasswordProperty, value);
    }

    public bool IsReadOnly
    {
        get => (bool)GetValue(IsReadOnlyProperty);
        set => SetValue(IsReadOnlyProperty, value);
    }

    #endregion

    private bool _isUpdatingText;

    public StyledEntryField()
    {
        InitializeComponent();
        UpdateLabel();
    }

    #region Property Changed Handlers

    private static void OnLabelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledEntryField field)
        {
            field.UpdateLabel();
        }
    }

    private static void OnTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledEntryField field && !field._isUpdatingText)
        {
            field._isUpdatingText = true;
            field.FieldEntry.Text = newValue as string ?? string.Empty;
            field._isUpdatingText = false;
        }
    }

    private static void OnPlaceholderChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledEntryField field)
        {
            field.FieldEntry.Placeholder = newValue as string ?? string.Empty;
        }
    }

    private static void OnIsRequiredChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledEntryField field)
        {
            field.UpdateLabel();
        }
    }

    private static void OnKeyboardChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledEntryField field && newValue is Keyboard keyboard)
        {
            field.FieldEntry.Keyboard = keyboard;
        }
    }

    private static void OnMaxLengthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledEntryField field && newValue is int maxLength)
        {
            field.FieldEntry.MaxLength = maxLength;
        }
    }

    private static void OnIsPasswordChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledEntryField field && newValue is bool isPassword)
        {
            field.FieldEntry.IsPassword = isPassword;
        }
    }

    private static void OnIsReadOnlyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledEntryField field && newValue is bool isReadOnly)
        {
            field.FieldEntry.IsReadOnly = isReadOnly;
        }
    }

    #endregion

    private void UpdateLabel()
    {
        var labelText = Label ?? string.Empty;
        if (IsRequired && !string.IsNullOrEmpty(labelText))
        {
            labelText += " *";
        }
        FieldLabel.Text = labelText;
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (!_isUpdatingText)
        {
            _isUpdatingText = true;
            Text = e.NewTextValue;
            _isUpdatingText = false;
        }
    }
}
