using Microsoft.Maui.Controls;

namespace NWork.WeekView;

public partial class WeekView : ContentView
{
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
}