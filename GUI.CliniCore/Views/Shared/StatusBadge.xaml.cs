namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// A reusable ContentView for color-coded status badges.
/// Used throughout list pages to display status information (e.g., "Scheduled", "Completed", "Cancelled").
/// </summary>
public partial class StatusBadge : ContentView
{
    #region Bindable Properties

    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        nameof(Text),
        typeof(string),
        typeof(StatusBadge),
        string.Empty,
        propertyChanged: OnTextChanged);

    public static readonly BindableProperty BadgeColorProperty = BindableProperty.Create(
        nameof(BadgeColor),
        typeof(Color),
        typeof(StatusBadge),
        Color.FromArgb("#9E9E9E"),  // Default gray
        propertyChanged: OnBadgeColorChanged);

    public static readonly BindableProperty BadgeTextColorProperty = BindableProperty.Create(
        nameof(BadgeTextColor),
        typeof(Color),
        typeof(StatusBadge),
        Colors.White,
        propertyChanged: OnBadgeTextColorChanged);

    public static readonly BindableProperty BadgeCornerRadiusProperty = BindableProperty.Create(
        nameof(BadgeCornerRadius),
        typeof(double),
        typeof(StatusBadge),
        10.0,
        propertyChanged: OnBadgeCornerRadiusChanged);

    public static readonly BindableProperty PaddingHorizontalProperty = BindableProperty.Create(
        nameof(PaddingHorizontal),
        typeof(double),
        typeof(StatusBadge),
        10.0,
        propertyChanged: OnPaddingChanged);

    public static readonly BindableProperty PaddingVerticalProperty = BindableProperty.Create(
        nameof(PaddingVertical),
        typeof(double),
        typeof(StatusBadge),
        4.0,
        propertyChanged: OnPaddingChanged);

    #endregion

    #region Properties

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public Color BadgeColor
    {
        get => (Color)GetValue(BadgeColorProperty);
        set => SetValue(BadgeColorProperty, value);
    }

    public Color BadgeTextColor
    {
        get => (Color)GetValue(BadgeTextColorProperty);
        set => SetValue(BadgeTextColorProperty, value);
    }

    public double BadgeCornerRadius
    {
        get => (double)GetValue(BadgeCornerRadiusProperty);
        set => SetValue(BadgeCornerRadiusProperty, value);
    }

    public double PaddingHorizontal
    {
        get => (double)GetValue(PaddingHorizontalProperty);
        set => SetValue(PaddingHorizontalProperty, value);
    }

    public double PaddingVertical
    {
        get => (double)GetValue(PaddingVerticalProperty);
        set => SetValue(PaddingVerticalProperty, value);
    }

    #endregion

    public StatusBadge()
    {
        InitializeComponent();
        ApplyDefaults();
    }

    private void ApplyDefaults()
    {
        BadgeBorder.BackgroundColor = BadgeColor;
        BadgeLabel.TextColor = BadgeTextColor;
        BadgeLabel.Text = Text;
        BorderShape.CornerRadius = new CornerRadius(BadgeCornerRadius);
        BadgeBorder.Padding = new Thickness(PaddingHorizontal, PaddingVertical);
    }

    #region Property Changed Handlers

    private static void OnTextChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatusBadge badge)
        {
            badge.BadgeLabel.Text = newValue as string ?? string.Empty;
        }
    }

    private static void OnBadgeColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatusBadge badge && newValue is Color color)
        {
            badge.BadgeBorder.BackgroundColor = color;
        }
    }

    private static void OnBadgeTextColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatusBadge badge && newValue is Color color)
        {
            badge.BadgeLabel.TextColor = color;
        }
    }

    private static void OnBadgeCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatusBadge badge && newValue is double radius)
        {
            badge.BorderShape.CornerRadius = new CornerRadius(radius);
        }
    }

    private static void OnPaddingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatusBadge badge)
        {
            badge.BadgeBorder.Padding = new Thickness(badge.PaddingHorizontal, badge.PaddingVertical);
        }
    }

    #endregion
}
