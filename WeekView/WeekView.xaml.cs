using NWork.Pages;

namespace NWork.WeekView;

enum DragGestureType
{
	Nothing,
	NewEvent,
	MovingEvent,
	ResizingEvent
}

public partial class WeekView : ContentView
{
	private DragGestureType dragType = DragGestureType.Nothing;
	private DateTime? dragStartTime = null;

	public static readonly BindableProperty ViewModelProperty = BindableProperty.Create(
		nameof(ViewModel), 
		typeof(WeekViewModel), 
		typeof(WeekView));

	public WeekViewModel ViewModel
	{
		get => (WeekViewModel)GetValue(ViewModelProperty);
		set => SetValue(ViewModelProperty, value);
	}

	public WeekView()
	{
		InitializeComponent();
		for (int i = 0; i < 8; i++)
		{
			for ( int j = 0; j < 15; j++ )
			{
				var cell = new Border
				{
					Stroke = new SolidColorBrush(Colors.DarkSlateGray),
					StrokeThickness = 0.1
				};
				Grid.SetColumn(cell, i);
				Grid.SetRow(cell, j);
				CalendarGrid.Add(cell);
			}
		}
	}

	private void PrevWeek(object sender, EventArgs e)
	{
		ViewModel.PrevWeek();
	}

	private void NextWeek(object sender, EventArgs e)
	{
		ViewModel.NextWeek();
	}

	private void Today(object sender, EventArgs e)
	{
		ViewModel.GoToday();
	}

	private void IssueLinkTapped(object sender, TappedEventArgs e)
	{
		Launcher.OpenAsync("https://graphisoft.atlassian.net/browse/" + ((Label)sender).Text);
	}

	private DateTime? TimeAtPointer(PointerEventArgs e)
	{
		Point? optpos = e.GetPosition(EventsGrid);
		if (optpos == null)
			return null;
		Point pos = optpos.Value;

		var calendarWidth = EventsGrid.Width - HoursColumn.Width.Value;
		var calendarHeight = EventsGrid.Height - HeaderRow.Height.Value - TotalRow.Height.Value;
		var relativeX = pos.X - HoursColumn.Width.Value;
		var relativeY = pos.Y - HeaderRow.Height.Value;
		if (relativeX < 0 || relativeX > calendarWidth || relativeY < 0 || relativeY > calendarHeight)
			return null;

		DateTime day = ViewModel.Monday.Add(relativeX / calendarWidth * TimeSpan.FromDays(7));
		TimeOnly time = new TimeOnly(7, 0).Add(relativeY / calendarHeight * TimeSpan.FromHours(13));
		return day.Date.Add(time.ToTimeSpan());
	}

	private static DateTime RoundUp(DateTime dt, TimeSpan d)
	{
		var delta = (d.Ticks - (dt.Ticks % d.Ticks)) % d.Ticks;
		return new DateTime(dt.Ticks + delta);
	}

	private static DateTime RoundDown(DateTime dt, TimeSpan d)
	{
		var delta = dt.Ticks % d.Ticks;
		return new DateTime(dt.Ticks - delta);
	}

	private static DateTime RoundToNearestQuarter(DateTime dt)
	{
		TimeSpan d = TimeSpan.FromMinutes(15);
		var delta = dt.Ticks % d.Ticks;

		return delta > d.Ticks / 2 ? RoundUp(dt, d) : RoundDown(dt, d);
	}

	private void Calendar_PointerPressed(object sender, PointerEventArgs e)
    {
		DateTime? dateTime = TimeAtPointer(e);
		if (dateTime == null || dragType != DragGestureType.Nothing)
			return;

		dragType = DragGestureType.NewEvent;
		dragStartTime = RoundToNearestQuarter(dateTime.Value);
		ViewModel.CurrentlyEditedEvent = new Event
		{
			Started = dragStartTime.Value,
			Duration = TimeSpan.FromMinutes(15),
			IsPlaceholder = true
		};
		ViewModel.Events.Add(ViewModel.CurrentlyEditedEvent);		
    }

	private void Calendar_PointerMoved(object sender, PointerEventArgs e)
	{
		DateTime? pDragEndTime = TimeAtPointer(e);
		if (ViewModel.CurrentlyEditedEvent == null || pDragEndTime == null || this.dragStartTime == null || dragType != DragGestureType.NewEvent)
			return;

		DateTime dragStartTime = this.dragStartTime.Value;
		DateTime dragEndTime = RoundToNearestQuarter(pDragEndTime.Value);
		if (dragStartTime > dragEndTime)
			(dragStartTime, dragEndTime) = (dragEndTime, dragStartTime);

		ViewModel.CurrentlyEditedEvent.Started = dragStartTime;
		ViewModel.CurrentlyEditedEvent.Duration = dragEndTime - dragStartTime;
	}

	private async void Calendar_PointerReleased(object sender, PointerEventArgs e)
	{
		DateTime? pDragEndTime = TimeAtPointer(e);
		if (ViewModel.CurrentlyEditedEvent == null || pDragEndTime == null || sender != EventsGrid || dragType != DragGestureType.NewEvent)
			return;
		await Navigation.PushModalAsync(new EditPage(new EditPageViewModel(ViewModel.GetPickerProvider(), ViewModel.CurrentlyEditedEvent!.Started, ViewModel.CurrentlyEditedEvent.Duration)));
		dragType = DragGestureType.Nothing;
	}

	private void Event_PointerEntered(object sender, PointerEventArgs e)
	{

	}

	private void Event_PointerPressed(object sender, PointerEventArgs e)
	{
		if (dragType != DragGestureType.Nothing)
		{
			return;
		}
		dragType = DragGestureType.MovingEvent;
	}

	private void Event_PointerMoved(object sender, PointerEventArgs e)
	{
		
	}

	private void Event_PointerReleased(object sender, PointerEventArgs e)
	{
		if (dragType == DragGestureType.NewEvent)
		{
			return;
		}
		dragType = DragGestureType.Nothing;
	}

	private void Event_PointerExited(object sender, PointerEventArgs e)
	{

	}

	private async void StartCreatingNewWorklog(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new EditPage(new EditPageViewModel(ViewModel.GetPickerProvider())));
    }

    private async void StartEditingWorklog(object sender, EventArgs e)
    {
		Event ev = (Event)((BindableObject) sender).BindingContext;
		if (ev.IsPlaceholder)
			return;

        await Navigation.PushModalAsync(new EditPage(new EditPageViewModel(ViewModel.GetPickerProvider(), new JiraClient.SuggestedIssue()
		{
			summaryText = ev.Description,
			key = ev.Title
		}, ev.Id, ev.Started, ev.Duration)));
    }

	private async void DeleteWorklog(object sender, EventArgs e)
	{
		Event ev = (Event)((BindableObject)sender).BindingContext;
		if (ev.IsPlaceholder)
			return;
		if (await Shell.Current.DisplayAlert("Delete worklog", "Are you sure?", "OK", "Cancel"))
		{
			await ViewModel.DeleteWorklog(new JiraClient.Worklog { id = ev.Id, issueKey = ev.Title });
		}
	}

	private void RefreshCurrentView(object sender, EventArgs e)
	{
		ViewModel.RefreshCurrentView();
	}
}