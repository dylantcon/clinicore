using System.Collections;

namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// A reusable ContentView for styled form picker fields.
/// Combines a label and picker with consistent styling.
/// </summary>
public partial class StyledPickerField : ContentView
{
    #region Bindable Properties

    public static readonly BindableProperty LabelProperty = BindableProperty.Create(
        nameof(Label),
        typeof(string),
        typeof(StyledPickerField),
        string.Empty,
        propertyChanged: OnLabelChanged);

    public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(
        nameof(ItemsSource),
        typeof(IList),
        typeof(StyledPickerField),
        null,
        propertyChanged: OnItemsSourceChanged);

    public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(
        nameof(SelectedItem),
        typeof(object),
        typeof(StyledPickerField),
        null,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedItemPropertyChanged);

    public static readonly BindableProperty SelectedIndexProperty = BindableProperty.Create(
        nameof(SelectedIndex),
        typeof(int),
        typeof(StyledPickerField),
        -1,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedIndexPropertyChanged);

    public static readonly BindableProperty TitleProperty = BindableProperty.Create(
        nameof(Title),
        typeof(string),
        typeof(StyledPickerField),
        string.Empty,
        propertyChanged: OnTitleChanged);

    public static readonly BindableProperty IsRequiredProperty = BindableProperty.Create(
        nameof(IsRequired),
        typeof(bool),
        typeof(StyledPickerField),
        false,
        propertyChanged: OnIsRequiredChanged);

    public static readonly BindableProperty ItemDisplayBindingPathProperty = BindableProperty.Create(
        nameof(ItemDisplayBindingPath),
        typeof(string),
        typeof(StyledPickerField),
        null,
        propertyChanged: OnItemDisplayBindingPathChanged);

    #endregion

    #region Properties

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
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

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public bool IsRequired
    {
        get => (bool)GetValue(IsRequiredProperty);
        set => SetValue(IsRequiredProperty, value);
    }

    public string? ItemDisplayBindingPath
    {
        get => (string?)GetValue(ItemDisplayBindingPathProperty);
        set => SetValue(ItemDisplayBindingPathProperty, value);
    }

    #endregion

    private bool _isUpdatingSelection;

    public StyledPickerField()
    {
        InitializeComponent();
        UpdateLabel();
    }

    #region Property Changed Handlers

    private static void OnLabelChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledPickerField field)
        {
            field.UpdateLabel();
        }
    }

    private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledPickerField field && newValue is IList items)
        {
            field.FieldPicker.ItemsSource = items;
        }
    }

    private static void OnSelectedItemPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledPickerField field && !field._isUpdatingSelection)
        {
            field._isUpdatingSelection = true;
            field.FieldPicker.SelectedItem = newValue;
            field._isUpdatingSelection = false;
        }
    }

    private static void OnSelectedIndexPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledPickerField field && !field._isUpdatingSelection && newValue is int index)
        {
            field._isUpdatingSelection = true;
            field.FieldPicker.SelectedIndex = index;
            field._isUpdatingSelection = false;
        }
    }

    private static void OnTitleChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledPickerField field)
        {
            field.FieldPicker.Title = newValue as string ?? string.Empty;
        }
    }

    private static void OnIsRequiredChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledPickerField field)
        {
            field.UpdateLabel();
        }
    }

    private static void OnItemDisplayBindingPathChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StyledPickerField field && newValue is string path && !string.IsNullOrEmpty(path))
        {
            field.FieldPicker.ItemDisplayBinding = new Binding(path);
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

    private void OnSelectedIndexChanged(object? sender, EventArgs e)
    {
        if (!_isUpdatingSelection)
        {
            _isUpdatingSelection = true;
            SelectedItem = FieldPicker.SelectedItem;
            SelectedIndex = FieldPicker.SelectedIndex;
            _isUpdatingSelection = false;
        }
    }
}
