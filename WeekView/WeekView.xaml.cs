using Microsoft.UI.Xaml.Controls;

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
}