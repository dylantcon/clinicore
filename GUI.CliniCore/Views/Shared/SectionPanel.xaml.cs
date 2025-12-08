namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// A reusable ContentView for grouping related form elements in a styled panel.
/// Provides consistent section styling with title and content area.
/// </summary>
[ContentProperty(nameof(SectionContent))]
public partial class SectionPanel : ContentView
{
    #region Bindable Properties

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(SectionPanel),
        string.Empty,
        propertyChanged: OnTitleChanged);

    public static readonly BindableProperty SectionContentProperty = BindableProperty.Create(
        nameof(SectionContent),
        typeof(View),
        typeof(SectionPanel),
        null,
        propertyChanged: OnSectionContentChanged);

    public static readonly BindableProperty SectionBackgroundColorProperty = BindableProperty.Create(
        nameof(SectionBackgroundColor),
        typeof(Color),
        typeof(SectionPanel),
        Color.FromArgb("#F5F5F5"),
        propertyChanged: OnSectionBackgroundColorChanged);

    public static readonly BindableProperty TitleColorProperty = BindableProperty.Create(
        nameof(TitleColor),
        typeof(Color),
        typeof(SectionPanel),
        null,
        propertyChanged: OnTitleColorChanged);

    public static readonly BindableProperty IsTitleVisibleProperty = BindableProperty.Create(
        nameof(IsTitleVisible),
        typeof(bool),
        typeof(SectionPanel),
        true,
        propertyChanged: OnIsTitleVisibleChanged);

    public static readonly BindableProperty SectionPaddingProperty = BindableProperty.Create(
        nameof(SectionPadding),
        typeof(Thickness),
        typeof(SectionPanel),
        new Thickness(15),
        propertyChanged: OnSectionPaddingChanged);

    #endregion

    #region Properties

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public View? SectionContent
    {
        get => (View?)GetValue(SectionContentProperty);
        set => SetValue(SectionContentProperty, value);
    }

    public Color SectionBackgroundColor
    {
        get => (Color)GetValue(SectionBackgroundColorProperty);
        set => SetValue(SectionBackgroundColorProperty, value);
    }

    public Color? TitleColor
    {
        get => (Color?)GetValue(TitleColorProperty);
        set => SetValue(TitleColorProperty, value);
    }

    public bool IsTitleVisible
    {
        get => (bool)GetValue(IsTitleVisibleProperty);
        set => SetValue(IsTitleVisibleProperty, value);
    }

    public Thickness SectionPadding
    {
        get => (Thickness)GetValue(SectionPaddingProperty);
        set => SetValue(SectionPaddingProperty, value);
    }

    #endregion

    public SectionPanel()
    {
        InitializeComponent();
    }

    #region Property Changed Handlers

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionPanel panel)
        {
            panel.TitleLabel.Text = newValue as string ?? string.Empty;
            panel.TitleLabel.IsVisible = panel.IsTitleVisible && !string.IsNullOrEmpty(panel.Title);
        }
    }

    private static void OnSectionContentChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionPanel panel && newValue is View view)
        {
            panel.ContentContainer.Content = view;
        }
    }

    private static void OnSectionBackgroundColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionPanel panel && newValue is Color color)
        {
            panel.SectionFrame.BackgroundColor = color;
        }
    }

    private static void OnTitleColorChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionPanel panel && newValue is Color color)
        {
            panel.TitleLabel.TextColor = color;
        }
    }

    private static void OnIsTitleVisibleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionPanel panel && newValue is bool isVisible)
        {
            panel.TitleLabel.IsVisible = isVisible && !string.IsNullOrEmpty(panel.Title);
        }
    }

    private static void OnSectionPaddingChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is SectionPanel panel && newValue is Thickness padding)
        {
            panel.SectionFrame.Padding = padding;
        }
    }

    #endregion
}
