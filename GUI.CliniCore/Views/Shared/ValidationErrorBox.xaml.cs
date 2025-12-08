namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// A reusable ContentView for displaying validation error messages.
/// Displays as a warning-styled frame when HasErrors is true.
/// </summary>
public partial class ValidationErrorBox : ContentView
{
    #region Bindable Properties

    public static readonly BindableProperty HasErrorsProperty = BindableProperty.Create(
        nameof(HasErrors),
        typeof(bool),
        typeof(ValidationErrorBox),
        false,
        propertyChanged: OnHasErrorsChanged);

    public static readonly BindableProperty ErrorMessageProperty = BindableProperty.Create(
        nameof(ErrorMessage),
        typeof(string),
        typeof(ValidationErrorBox),
        string.Empty,
        propertyChanged: OnErrorMessageChanged);

    public static readonly BindableProperty ErrorBackgroundColorProperty = BindableProperty.Create(
        nameof(ErrorBackgroundColor),
        typeof(Color),
        typeof(ValidationErrorBox),
        Color.FromArgb("#FFF3CD"),
        propertyChanged: OnErrorBackgroundColorChanged);

    public static readonly BindableProperty ErrorBorderColorProperty = BindableProperty.Create(
        nameof(ErrorBorderColor),
        typeof(Color),
        typeof(ValidationErrorBox),
        Color.FromArgb("#856404"),
        propertyChanged: OnErrorBorderColorChanged);

    public static readonly BindableProperty ErrorTextColorProperty = BindableProperty.Create(
        nameof(ErrorTextColor),
        typeof(Color),
        typeof(ValidationErrorBox),
        Color.FromArgb("#856404"),
        propertyChanged: OnErrorTextColorChanged);

    #endregion

    #region Properties

    public bool HasErrors
    {
        get => (bool)GetValue(HasErrorsProperty);
        set => SetValue(HasErrorsProperty, value);
    }

    public string ErrorMessage
    {
        get => (string)GetValue(ErrorMessageProperty);
        set => SetValue(ErrorMessageProperty, value);
    }

    public Color ErrorBackgroundColor
    {
        get => (Color)GetValue(ErrorBackgroundColorProperty);
        set => SetValue(ErrorBackgroundColorProperty, value);
    }

    public Color ErrorBorderColor
    {
        get => (Color)GetValue(ErrorBorderColorProperty);
        set => SetValue(ErrorBorderColorProperty, value);
    }

    public Color ErrorTextColor
    {
        get => (Color)GetValue(ErrorTextColorProperty);
        set => SetValue(ErrorTextColorProperty, value);
    }

    #endregion

    public ValidationErrorBox()
    {
        InitializeComponent();
        UpdateVisibility();
    }

    #region Property Changed Handlers

    private static void OnHasErrorsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidationErrorBox box)
        {
            box.UpdateVisibility();
        }
    }

    private static void OnErrorMessageChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidationErrorBox box)
        {
            box.ErrorLabel.Text = newValue as string ?? string.Empty;
        }
    }

    private static void OnErrorBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidationErrorBox box && newValue is Color color)
        {
            box.ErrorFrame.BackgroundColor = color;
        }
    }

    private static void OnErrorBorderColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidationErrorBox box && newValue is Color color)
        {
            box.ErrorFrame.BorderColor = color;
        }
    }

    private static void OnErrorTextColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ValidationErrorBox box && newValue is Color color)
        {
            box.ErrorLabel.TextColor = color;
        }
    }

    #endregion

    private void UpdateVisibility()
    {
        ErrorFrame.IsVisible = HasErrors && !string.IsNullOrEmpty(ErrorMessage);
    }
}
