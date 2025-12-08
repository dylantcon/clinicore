using System.Collections;
using System.Collections.Specialized;
using System.Windows.Input;
using Core.CliniCore.Scheduling;
using GUI.CliniCore.Helpers;
using Microsoft.Maui.Controls.Shapes;

namespace GUI.CliniCore.Views.Shared;

/// <summary>
/// Calendar view modes for displaying appointments.
/// </summary>
public enum CalendarViewMode
{
    Week,
    Month
}

/// <summary>
/// A reusable ContentView that displays appointments in a calendar grid format.
/// Supports week and month views with color-coded appointment cards.
/// </summary>
public partial class CalendarView : ContentView
{
    private DateTime _displayDate;

    #region Bindable Properties

    public static readonly BindableProperty AppointmentsProperty = BindableProperty.Create(
        nameof(Appointments),
        typeof(IEnumerable),
        typeof(CalendarView),
        null,
        propertyChanged: OnAppointmentsChanged);

    public static readonly BindableProperty ViewModeProperty = BindableProperty.Create(
        nameof(ViewMode),
        typeof(CalendarViewMode),
        typeof(CalendarView),
        CalendarViewMode.Week,
        propertyChanged: OnViewModeChanged);

    public static readonly BindableProperty SelectedDateProperty = BindableProperty.Create(
        nameof(SelectedDate),
        typeof(DateTime),
        typeof(CalendarView),
        DateTime.Today,
        BindingMode.TwoWay,
        propertyChanged: OnSelectedDateChanged);

    public static readonly BindableProperty AppointmentTappedCommandProperty = BindableProperty.Create(
        nameof(AppointmentTappedCommand),
        typeof(ICommand),
        typeof(CalendarView),
        null);

    #endregion

    #region Properties

    public IEnumerable? Appointments
    {
        get => (IEnumerable?)GetValue(AppointmentsProperty);
        set => SetValue(AppointmentsProperty, value);
    }

    public CalendarViewMode ViewMode
    {
        get => (CalendarViewMode)GetValue(ViewModeProperty);
        set => SetValue(ViewModeProperty, value);
    }

    public DateTime SelectedDate
    {
        get => (DateTime)GetValue(SelectedDateProperty);
        set => SetValue(SelectedDateProperty, value);
    }

    public ICommand? AppointmentTappedCommand
    {
        get => (ICommand?)GetValue(AppointmentTappedCommandProperty);
        set => SetValue(AppointmentTappedCommandProperty, value);
    }

    #endregion

    public CalendarView()
    {
        InitializeComponent();
        _displayDate = DateTime.Today;
        UpdateViewModeButtons();
        RebuildCalendar();
    }

    #region Property Changed Handlers

    private static void OnAppointmentsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CalendarView view)
        {
            // Unsubscribe from old collection
            if (oldValue is INotifyCollectionChanged oldCollection)
            {
                oldCollection.CollectionChanged -= view.OnAppointmentsCollectionChanged;
            }

            // Subscribe to new collection
            if (newValue is INotifyCollectionChanged newCollection)
            {
                newCollection.CollectionChanged += view.OnAppointmentsCollectionChanged;
            }

            view.RebuildCalendar();
        }
    }

    private void OnAppointmentsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        RebuildCalendar();
    }

    private static void OnViewModeChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CalendarView view)
        {
            view.UpdateViewModeButtons();
            view.RebuildCalendar();
        }
    }

    private static void OnSelectedDateChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is CalendarView view && newValue is DateTime date)
        {
            view._displayDate = date;
            view.RebuildCalendar();
        }
    }

    #endregion

    #region Event Handlers

    private void OnPreviousPeriod(object? sender, EventArgs e)
    {
        _displayDate = ViewMode == CalendarViewMode.Week
            ? _displayDate.AddDays(-7)
            : _displayDate.AddMonths(-1);
        SelectedDate = _displayDate;
    }

    private void OnNextPeriod(object? sender, EventArgs e)
    {
        _displayDate = ViewMode == CalendarViewMode.Week
            ? _displayDate.AddDays(7)
            : _displayDate.AddMonths(1);
        SelectedDate = _displayDate;
    }

    private void OnWeekModeSelected(object? sender, EventArgs e)
    {
        ViewMode = CalendarViewMode.Week;
    }

    private void OnMonthModeSelected(object? sender, EventArgs e)
    {
        ViewMode = CalendarViewMode.Month;
    }

    #endregion

    #region Calendar Building

    private void UpdateViewModeButtons()
    {
        if (ViewMode == CalendarViewMode.Week)
        {
            WeekButton.BackgroundColor = Color.FromArgb("#2196F3");
            WeekButton.TextColor = Colors.White;
            MonthButton.BackgroundColor = Color.FromArgb("#E0E0E0");
            MonthButton.TextColor = Color.FromArgb("#333");
        }
        else
        {
            MonthButton.BackgroundColor = Color.FromArgb("#2196F3");
            MonthButton.TextColor = Colors.White;
            WeekButton.BackgroundColor = Color.FromArgb("#E0E0E0");
            WeekButton.TextColor = Color.FromArgb("#333");
        }
    }

    private void RebuildCalendar()
    {
        CalendarGrid.Children.Clear();
        CalendarGrid.RowDefinitions.Clear();

        // Update period label
        UpdatePeriodLabel();

        // Get date range based on view mode
        var (startDate, endDate, rowCount) = GetDateRange();

        // Add row definitions
        for (int i = 0; i < rowCount; i++)
        {
            CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
        }

        // Get appointments in range
        var appointmentsInRange = GetAppointmentsInRange(startDate, endDate);

        // Build calendar cells
        var currentDate = startDate;
        for (int row = 0; row < rowCount; row++)
        {
            for (int col = 0; col < 7; col++)
            {
                var cell = CreateDayCell(currentDate, appointmentsInRange);
                Grid.SetRow(cell, row);
                Grid.SetColumn(cell, col);
                CalendarGrid.Children.Add(cell);
                currentDate = currentDate.AddDays(1);
            }
        }
    }

    private void UpdatePeriodLabel()
    {
        if (ViewMode == CalendarViewMode.Week)
        {
            var weekStart = GetWeekStart(_displayDate);
            var weekEnd = weekStart.AddDays(6);
            PeriodLabel.Text = weekStart.Month == weekEnd.Month
                ? $"{weekStart:MMM d} - {weekEnd:d}, {weekStart:yyyy}"
                : $"{weekStart:MMM d} - {weekEnd:MMM d}, {weekStart:yyyy}";
        }
        else
        {
            PeriodLabel.Text = _displayDate.ToString("MMMM yyyy");
        }
    }

    private (DateTime startDate, DateTime endDate, int rowCount) GetDateRange()
    {
        if (ViewMode == CalendarViewMode.Week)
        {
            var weekStart = GetWeekStart(_displayDate);
            return (weekStart, weekStart.AddDays(7), 1);
        }
        else
        {
            var monthStart = new DateTime(_displayDate.Year, _displayDate.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);
            var calendarStart = GetWeekStart(monthStart);
            var daysToShow = (int)(monthEnd - calendarStart).TotalDays + 1;
            var rowCount = (daysToShow + 6) / 7; // Round up to complete weeks
            return (calendarStart, calendarStart.AddDays(rowCount * 7), rowCount);
        }
    }

    private static DateTime GetWeekStart(DateTime date)
    {
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Sunday)) % 7;
        return date.AddDays(-diff).Date;
    }

    private List<AppointmentTimeInterval> GetAppointmentsInRange(DateTime startDate, DateTime endDate)
    {
        if (Appointments == null) return new List<AppointmentTimeInterval>();

        return Appointments
            .OfType<AppointmentTimeInterval>()
            .Where(a => a.Start.Date >= startDate.Date && a.Start.Date < endDate.Date)
            .OrderBy(a => a.Start)
            .ToList();
    }

    private View CreateDayCell(DateTime date, List<AppointmentTimeInterval> allAppointments)
    {
        var isToday = date.Date == DateTime.Today;
        var isCurrentMonth = date.Month == _displayDate.Month;
        var dayAppointments = allAppointments.Where(a => a.Start.Date == date.Date).ToList();

        var border = new Border
        {
            BackgroundColor = isToday ? Color.FromArgb("#E3F2FD") : Colors.White,
            Stroke = isToday ? Color.FromArgb("#2196F3") : Colors.LightGray,
            StrokeThickness = isToday ? 2 : 1,
            Padding = new Thickness(4),
            MinimumHeightRequest = 60
        };
        border.StrokeShape = new RoundRectangle { CornerRadius = 4 };

        var stack = new VerticalStackLayout { Spacing = 2 };

        // Day number
        var dayLabel = new Label
        {
            Text = date.Day.ToString(),
            FontSize = 12,
            FontAttributes = isToday ? FontAttributes.Bold : FontAttributes.None,
            TextColor = isCurrentMonth ? (isToday ? Color.FromArgb("#2196F3") : Colors.Black) : Colors.LightGray,
            HorizontalOptions = LayoutOptions.End
        };
        stack.Children.Add(dayLabel);

        // Appointments (show max 3)
        var displayCount = Math.Min(dayAppointments.Count, 3);
        for (int i = 0; i < displayCount; i++)
        {
            var apt = dayAppointments[i];
            var aptCard = CreateAppointmentCard(apt);
            stack.Children.Add(aptCard);
        }

        // Show "+N more" if there are more appointments
        if (dayAppointments.Count > 3)
        {
            var moreLabel = new Label
            {
                Text = $"+{dayAppointments.Count - 3} more",
                FontSize = 10,
                TextColor = Colors.Gray,
                HorizontalOptions = LayoutOptions.Start
            };
            stack.Children.Add(moreLabel);
        }

        border.Content = stack;

        // Add tap gesture for day selection
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) => SelectedDate = date;
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }

    private View CreateAppointmentCard(AppointmentTimeInterval appointment)
    {
        var statusColor = SpecializationColorHelper.GetStatusColor(appointment.Status);
        var textColor = SpecializationColorHelper.GetContrastingTextColor(statusColor);

        var border = new Border
        {
            BackgroundColor = statusColor,
            Padding = new Thickness(4, 2),
            Margin = new Thickness(0, 1)
        };
        border.StrokeShape = new RoundRectangle { CornerRadius = 4 };
        border.StrokeThickness = 0;

        var label = new Label
        {
            Text = $"{appointment.Start:HH:mm}",
            FontSize = 10,
            TextColor = textColor,
            LineBreakMode = LineBreakMode.TailTruncation
        };

        border.Content = label;

        // Add tap gesture for appointment
        var tapGesture = new TapGestureRecognizer();
        tapGesture.Tapped += (s, e) =>
        {
            AppointmentTappedCommand?.Execute(appointment);
        };
        border.GestureRecognizers.Add(tapGesture);

        return border;
    }

    #endregion
}
