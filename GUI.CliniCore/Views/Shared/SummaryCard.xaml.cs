using System.Collections;
using System.Collections.Specialized;

namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// A reusable ContentView for displaying summary/statistics cards.
/// Used in detail pages to show metrics like patient counts, appointment counts, etc.
/// </summary>
public partial class SummaryCard : ContentView
{
    #region Bindable Properties

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(SummaryCard),
        string.Empty,
        propertyChanged: OnTitleChanged);

    public static readonly BindableProperty ItemsProperty = BindableProperty.Create(
        nameof(Items),
        typeof(IEnumerable),
        typeof(SummaryCard),
        null,
        propertyChanged: OnItemsChanged);

    public static readonly BindableProperty StrokeColorProperty = BindableProperty.Create(
        nameof(StrokeColor),
        typeof(Color),
        typeof(SummaryCard),
        Colors.LightGray,
        propertyChanged: OnStrokeColorChanged);

    public static readonly BindableProperty CardCornerRadiusProperty = BindableProperty.Create(
        nameof(CardCornerRadius),
        typeof(double),
        typeof(SummaryCard),
        8.0,
        propertyChanged: OnCardCornerRadiusChanged);

    public static readonly BindableProperty ActionContentProperty = BindableProperty.Create(
        nameof(ActionContent),
        typeof(View),
        typeof(SummaryCard),
        null,
        propertyChanged: OnActionContentChanged);

    public static readonly BindableProperty ShowSeparatorProperty = BindableProperty.Create(
        nameof(ShowSeparator),
        typeof(bool),
        typeof(SummaryCard),
        true);

    #endregion

    #region Properties

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public IEnumerable Items
    {
        get => (IEnumerable)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public Color StrokeColor
    {
        get => (Color)GetValue(StrokeColorProperty);
        set => SetValue(StrokeColorProperty, value);
    }

    public double CardCornerRadius
    {
        get => (double)GetValue(CardCornerRadiusProperty);
        set => SetValue(CardCornerRadiusProperty, value);
    }

    public View? ActionContent
    {
        get => (View?)GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public bool ShowSeparator
    {
        get => (bool)GetValue(ShowSeparatorProperty);
        set => SetValue(ShowSeparatorProperty, value);
    }

    #endregion

    public SummaryCard()
    {
        InitializeComponent();
    }

    #region Property Changed Handlers

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SummaryCard card)
        {
            var title = newValue as string ?? string.Empty;
            card.TitleLabel.Text = title;
            card.TitleLabel.IsVisible = !string.IsNullOrEmpty(title);
        }
    }

    private static void OnItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SummaryCard card)
        {
            // Unsubscribe from old collection
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= card.OnItemsCollectionChanged;
            }

            // Subscribe to new collection
            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += card.OnItemsCollectionChanged;
            }

            card.RebuildItems();
        }
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildItems();
    }

    private static void OnStrokeColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SummaryCard card && newValue is Color color)
        {
            card.CardBorder.Stroke = color;
        }
    }

    private static void OnCardCornerRadiusChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SummaryCard card && newValue is double radius)
        {
            card.BorderShape.CornerRadius = new CornerRadius(radius);
        }
    }

    private static void OnActionContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SummaryCard card)
        {
            card.ActionContentArea.Content = newValue as View;
            card.ActionContentArea.IsVisible = newValue != null;
        }
    }

    #endregion

    #region Item Building

    private void RebuildItems()
    {
        ItemsContainer.Children.Clear();

        if (Items == null) return;

        var itemsList = Items.Cast<object>().ToList();
        for (int i = 0; i < itemsList.Count; i++)
        {
            if (itemsList[i] is SummaryItem item)
            {
                ItemsContainer.Children.Add(CreateItemView(item));

                // Add separator between items if ShowSeparator is true (but not after last item)
                if (ShowSeparator && i < itemsList.Count - 1)
                {
                    ItemsContainer.Children.Add(new BoxView
                    {
                        HeightRequest = 1,
                        Color = Colors.LightGray,
                        Margin = new Thickness(0, 2)
                    });
                }
            }
        }
    }

    private static View CreateItemView(SummaryItem item)
    {
        var iconLabel = new Label
        {
            Text = item.Icon,
            FontFamily = "MaterialIconsRegular",
            FontSize = 20,
            TextColor = item.IconColor,
            VerticalOptions = LayoutOptions.Center
        };

        var valueSpan = new Span
        {
            Text = item.Value,
            FontAttributes = FontAttributes.Bold
        };

        var labelSpan = new Span
        {
            Text = " " + item.Label
        };

        var formattedString = new FormattedString();
        formattedString.Spans.Add(valueSpan);
        formattedString.Spans.Add(labelSpan);

        var textLabel = new Label
        {
            FormattedText = formattedString,
            FontSize = 14,
            TextColor = Color.FromArgb("#333"),
            VerticalOptions = LayoutOptions.Center
        };

        var stack = new HorizontalStackLayout
        {
            Spacing = 10,
            Children = { iconLabel, textLabel }
        };

        // Add tap gesture if command is provided
        if (item.TapCommand != null)
        {
            var tapGesture = new TapGestureRecognizer { Command = item.TapCommand };
            stack.GestureRecognizers.Add(tapGesture);
        }

        return stack;
    }

    #endregion
}
