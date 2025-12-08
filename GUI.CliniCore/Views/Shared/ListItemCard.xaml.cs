using System.Windows.Input;

namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// A reusable ContentView for list item cards with consistent styling.
/// Used in CollectionView/ListView item templates across list pages.
/// Provides a bordered card with main content area and optional action content (badge/button).
/// </summary>
public partial class ListItemCard : ContentView
{
    #region Bindable Properties

    public static readonly BindableProperty MainContentProperty = BindableProperty.Create(
        nameof(MainContent),
        typeof(View),
        typeof(ListItemCard),
        null,
        propertyChanged: OnMainContentChanged);

    public static readonly BindableProperty ActionContentProperty = BindableProperty.Create(
        nameof(ActionContent),
        typeof(View),
        typeof(ListItemCard),
        null,
        propertyChanged: OnActionContentChanged);

    public static readonly BindableProperty TapCommandProperty = BindableProperty.Create(
        nameof(TapCommand),
        typeof(ICommand),
        typeof(ListItemCard),
        null);

    public static readonly BindableProperty TapCommandParameterProperty = BindableProperty.Create(
        nameof(TapCommandParameter),
        typeof(object),
        typeof(ListItemCard),
        null);

    public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(
        nameof(StrokeColor),
        typeof(Color),
        typeof(ListItemCard),
        Colors.LightGray,
        propertyChanged: OnStrokeColorChanged);

    public static readonly BindableProperty CardBackgroundColorProperty = BindableProperty.Create(
        nameof(CardBackgroundColor),
        typeof(Color),
        typeof(ListItemCard),
        Colors.White,
        propertyChanged: OnCardBackgroundColorChanged);

    public static readonly BindableProperty CardCornerRadiusProperty = BindableProperty.Create(
        nameof(CardCornerRadius),
        typeof(double),
        typeof(ListItemCard),
        8.0,
        propertyChanged: OnCardCornerRadiusChanged);

    public static readonly BindableProperty CardPaddingProperty = BindableProperty.Create(
        nameof(CardPadding),
        typeof(Thickness),
        typeof(ListItemCard),
        new Thickness(15),
        propertyChanged: OnCardPaddingChanged);

    public static readonly BindableProperty CardMarginProperty = BindableProperty.Create(
        nameof(CardMargin),
        typeof(Thickness),
        typeof(ListItemCard),
        new Thickness(0, 5),
        propertyChanged: OnCardMarginChanged);

    public static readonly BindableProperty ShowActionContentProperty = BindableProperty.Create(
        nameof(ShowActionContent),
        typeof(bool),
        typeof(ListItemCard),
        true,
        propertyChanged: OnShowActionContentChanged);

    #endregion

    #region Properties

    public View? MainContent
    {
        get => (View?)GetValue(MainContentProperty);
        set => SetValue(MainContentProperty, value);
    }

    public View? ActionContent
    {
        get => (View?)GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public ICommand? TapCommand
    {
        get => (ICommand?)GetValue(TapCommandProperty);
        set => SetValue(TapCommandProperty, value);
    }

    public object? TapCommandParameter
    {
        get => GetValue(TapCommandParameterProperty);
        set => SetValue(TapCommandParameterProperty, value);
    }

    public Color StrokeColor
    {
        get => (Color)GetValue(StrokeColorProperty);
        set => SetValue(StrokeColorProperty, value);
    }

    public Color CardBackgroundColor
    {
        get => (Color)GetValue(CardBackgroundColorProperty);
        set => SetValue(CardBackgroundColorProperty, value);
    }

    public double CardCornerRadius
    {
        get => (double)GetValue(CardCornerRadiusProperty);
        set => SetValue(CardCornerRadiusProperty, value);
    }

    public Thickness CardPadding
    {
        get => (Thickness)GetValue(CardPaddingProperty);
        set => SetValue(CardPaddingProperty, value);
    }

    public Thickness CardMargin
    {
        get => (Thickness)GetValue(CardMarginProperty);
        set => SetValue(CardMarginProperty, value);
    }

    public bool ShowActionContent
    {
        get => (bool)GetValue(ShowActionContentProperty);
        set => SetValue(ShowActionContentProperty, value);
    }

    #endregion

    #region Events

    public event EventHandler? CardTapped;

    #endregion

    public ListItemCard()
    {
        InitializeComponent();
    }

    #region Property Changed Handlers

    private static void OnMainContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListItemCard card)
        {
            card.MainContentArea.Content = newValue as View;
        }
    }

    private static void OnActionContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListItemCard card)
        {
            card.ActionContentArea.Content = newValue as View;
            card.ActionContentArea.IsVisible = card.ShowActionContent && newValue != null;
        }
    }

    private static void OnStrokeColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListItemCard card && newValue is Color color)
        {
            card.CardBorder.Stroke = color;
        }
    }

    private static void OnCardBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListItemCard card && newValue is Color color)
        {
            card.CardBorder.BackgroundColor = color;
        }
    }

    private static void OnCardCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListItemCard card && newValue is double radius)
        {
            card.BorderShape.CornerRadius = new CornerRadius(radius);
        }
    }

    private static void OnCardPaddingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListItemCard card && newValue is Thickness padding)
        {
            card.CardBorder.Padding = padding;
        }
    }

    private static void OnCardMarginChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListItemCard card && newValue is Thickness margin)
        {
            card.CardBorder.Margin = margin;
        }
    }

    private static void OnShowActionContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ListItemCard card && newValue is bool show)
        {
            card.ActionContentArea.IsVisible = show && card.ActionContent != null;
        }
    }

    #endregion

    private void OnCardTapped(object? sender, EventArgs e)
    {
        CardTapped?.Invoke(this, e);
        TapCommand?.Execute(TapCommandParameter);
    }
}
