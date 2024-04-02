using CommunityToolkit.Mvvm.ComponentModel;
using NWork.JiraClient;
using NWork.WeekView;
using System.Collections.ObjectModel;

namespace NWork.Pages
{
	public class EditPageViewModel : ObservableObject
	{
		private readonly IPickerProvider client;

		public EditPageViewModel()
		{
			client = null!;
		}

		public EditPageViewModel(IPickerProvider client)
		{
			this.client = client;
		}

		public EditPageViewModel(IPickerProvider client, SuggestedIssue issue, string worklogId, DateTime started, TimeSpan timeSpent)
		{
			this.client = client;
			SelectedIssue = issue;
			dateTime = started;
			EnteredTimespan = timeSpent;
		}

		public ObservableCollection<SuggestedIssue> SearchResults { get; } = [];
		public SuggestedIssue? SelectedIssue
		{
			get => selectedIssue;
			set
			{
				selectedIssue = value;
				OnPropertyChanged(nameof(SaveEnabled));
			}
		}

		public TimeSpan Time
		{
			get => dateTime.TimeOfDay;
			set
			{
				dateTime = dateTime.Date + value;
			}
		}

		public DateTime Date
		{
			get => dateTime;
			set
			{
				dateTime = value + Time;
			}
		}

		public TimeSpan? EnteredTimespan
		{
			get => enteredTimespan;
			set
			{
				enteredTimespan = value;
				OnPropertyChanged(nameof(InvalidTimespan));
			}
		}

		public Brush InvalidTimespan
		{
			get => EnteredTimespan == null ? Brush.Red : Brush.Transparent;
		}

		public bool SaveEnabled => SelectedIssue != null;

		private SuggestedIssue? selectedIssue = null;
		private TimeSpan? enteredTimespan = TimeSpan.Zero;
		private DateTime dateTime;

		public Task<IEnumerable<SuggestedIssue>> GetPickerSuggestions(string query)
		{
			return client.GetPickerSuggestions(query);
		}
	}
}
