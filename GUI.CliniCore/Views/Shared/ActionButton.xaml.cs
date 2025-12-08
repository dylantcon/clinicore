using System.Windows.Input;

namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// Style variants for ActionButton.
/// </summary>
public enum ButtonStyleType
{
    Primary,    // Blue - main actions
    Secondary,  // Gray - cancel/back
    Danger,     // Red - destructive actions
    Success     // Green - completion/confirmation
}

/// <summary>
/// A reusable ContentView for styled action buttons with consistent appearance.
/// Supports multiple style variants (Primary, Secondary, Danger, Success).
/// </summary>
public partial class ActionButton : ContentView
{
    #region Style Colors

    private static readonly Color PrimaryBackground = Color.FromArgb("#2196F3");
    private static readonly Color PrimaryText = Colors.White;

    private static readonly Color SecondaryBackground = Color.FromArgb("#757575");
    private static readonly Color SecondaryText = Colors.White;

    private static readonly Color DangerBackground = Color.FromArgb("#F44336");
    private static readonly Color DangerText = Colors.White;

    private static readonly Color SuccessBackground = Color.FromArgb("#4CAF50");
    private static readonly Color SuccessText = Colors.White;

    #endregion

    #region Bindable Properties

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(ActionButton),
        string.Empty,
        propertyChanged: OnTextChanged);

    public static readonly BindableProperty CommandProperty = BindableProperty.Create(
        nameof(Command),
        typeof(ICommand),
        typeof(ActionButton),
        null,
        propertyChanged: OnCommandChanged);

    public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(
        nameof(CommandParameter),
        typeof(object),
        typeof(ActionButton),
        null,
        propertyChanged: OnCommandParameterChanged);

    public static readonly BindableProperty ButtonStyleTypeProperty = BindableProperty.Create(
        nameof(ButtonStyleType),
        typeof(ButtonStyleType),
        typeof(ActionButton),
        ButtonStyleType.Primary,
        propertyChanged: OnButtonStyleTypeChanged);

    public static readonly BindableProperty IsButtonEnabledProperty = BindableProperty.Create(
        nameof(IsButtonEnabled),
        typeof(bool),
        typeof(ActionButton),
        true,
        propertyChanged: OnIsButtonEnabledChanged);

    public static readonly BindableProperty ButtonWidthProperty = BindableProperty.Create(
        nameof(ButtonWidth),
        typeof(double),
        typeof(ActionButton),
        -1.0,
        propertyChanged: OnButtonWidthChanged);

    #endregion

    #region Properties

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public ICommand? Command
    {
        get => (ICommand?)GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }

    public object? CommandParameter
    {
        get => GetValue(CommandParameterProperty);
        set => SetValue(CommandParameterProperty, value);
    }

    public ButtonStyleType ButtonStyleType
    {
        get => (ButtonStyleType)GetValue(ButtonStyleTypeProperty);
        set => SetValue(ButtonStyleTypeProperty, value);
    }

    public bool IsButtonEnabled
    {
        get => (bool)GetValue(IsButtonEnabledProperty);
        set => SetValue(IsButtonEnabledProperty, value);
    }

    public double ButtonWidth
    {
        get => (double)GetValue(ButtonWidthProperty);
        set => SetValue(ButtonWidthProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler? Clicked;

    #endregion

    public ActionButton()
    {
        InitializeComponent();
        ApplyStyle();
    }

    #region Property Changed Handlers

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ActionButton button)
        {
            button.InnerButton.Text = newValue as string ?? string.Empty;
        }
    }

    private static void OnCommandChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ActionButton button)
        {
            button.InnerButton.Command = newValue as ICommand;
        }
    }

    private static void OnCommandParameterChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ActionButton button)
        {
            button.InnerButton.CommandParameter = newValue;
        }
    }

    private static void OnButtonStyleTypeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ActionButton button)
        {
            button.ApplyStyle();
        }
    }

    private static void OnIsButtonEnabledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ActionButton button && newValue is bool isEnabled)
        {
            button.InnerButton.IsEnabled = isEnabled;
        }
    }

    private static void OnButtonWidthChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ActionButton button && newValue is double width)
        {
            button.InnerButton.WidthRequest = width > 0 ? width : -1;
        }
    }

    #endregion

    private void ApplyStyle()
    {
        var (backgroundColor, textColor) = ButtonStyleType switch
        {
            ButtonStyleType.Primary => (PrimaryBackground, PrimaryText),
            ButtonStyleType.Secondary => (SecondaryBackground, SecondaryText),
            ButtonStyleType.Danger => (DangerBackground, DangerText),
            ButtonStyleType.Success => (SuccessBackground, SuccessText),
            _ => (PrimaryBackground, PrimaryText)
        };

        InnerButton.BackgroundColor = backgroundColor;
        InnerButton.TextColor = textColor;
    }

    private void OnButtonClicked(object? sender, EventArgs e)
    {
        Clicked?.Invoke(this, e);
    }
}
