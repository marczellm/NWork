using NWork.Pages;
using System.Diagnostics;

namespace NWork.WeekView;

public partial class WeekView : ContentView
{
	private bool dragInProgress = false;

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

	private TimeOnly? TimeAtPointer(PointerEventArgs e)
	{
        Point? optpos = e.GetPosition(EventsGrid);
        if (optpos == null)
            return null;
        Point pos = optpos.Value;

        var calendarHeight = EventsGrid.Height - HeaderRow.Height.Value - TotalRow.Height.Value;
        var relativeY = pos.Y - HeaderRow.Height.Value;
        if (relativeY < 0 || relativeY > calendarHeight)
            return null;

        return new TimeOnly(7, 0).Add((relativeY / calendarHeight) * TimeSpan.FromHours(13));
    }

    private void PointerGestureRecognizer_PointerPressed(object sender, PointerEventArgs e)
    {
		dragInProgress = true;
    }

    private void PointerGestureRecognizer_PointerMoved(object sender, PointerEventArgs e)
    {
		if (!dragInProgress)
			return;

		Debug.WriteLine(TimeAtPointer(e));
    }

    private void PointerGestureRecognizer_PointerReleased(object sender, PointerEventArgs e)
    {
		dragInProgress = false;
    }

    private async void StartCreatingNewWorklog(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new EditPage(ViewModel));
    }

    private async void StartEditingWorklog(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new EditPage(ViewModel));
    }

	private async void DeleteWorklog(object sender, EventArgs e)
	{
		await Shell.Current.DisplayAlert("Delete worklog", "Are you sure?", "OK", "Cancel");
	}
}