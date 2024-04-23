using CommunityToolkit.Mvvm.ComponentModel;
using NWork.JiraClient;
using NWork.WeekView;
using System.Collections.ObjectModel;

namespace NWork.Pages
{
	public class EditPageViewModel : ObservableObject
	{
		private readonly IPickerProvider client;
		private readonly string? worklogId = null;

		public EditPageViewModel()
		{
			client = null!;

            saveCommand = new Command(async () => {
				Worklog worklog = new()
				{
					issueId = SelectedIssue!.id,
					issueKey = SelectedIssue!.key,
					started = dateTime.ToString("yyyy-MM-dd'T'HH:mm:ss.fff") + dateTime.ToString("zzzz").Replace(":", ""),
					timeSpentSeconds = (int)(EnteredTimespan?.TotalSeconds ?? 0),
				};
				bool success = false;
				if (IsEditMode)
                {
					worklog.id = worklogId!;
                    success = await client.EditWorklog(worklog);
                }
                else
                {
                    success = await client.AddWorklog(worklog);
                }
				if (success)
				{
					SaveFinished?.Invoke();
				}
			}, () => {
                return SelectedIssue != null && EnteredTimespan != null;
            });
        }

		public EditPageViewModel(IPickerProvider client): this()
		{
			this.client = client;
		}

		public EditPageViewModel(IPickerProvider client, DateTime started, TimeSpan timeSpent) : this(client)
		{
			dateTime = started;
			EnteredTimespan = timeSpent;
		}

		public EditPageViewModel(IPickerProvider client, SuggestedIssue issue, string worklogId, DateTime started, TimeSpan timeSpent) : this(client, started, timeSpent)
        {
			this.worklogId = worklogId;
			SelectedIssue = issue;
		}

		public ObservableCollection<SuggestedIssue> SearchResults { get; } = [];
		public SuggestedIssue? SelectedIssue
		{
			get => selectedIssue;
			set
			{
				selectedIssue = value;
				SaveCommand.ChangeCanExecute();
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
				SaveCommand.ChangeCanExecute();
			}
		}

		public Brush InvalidTimespan
		{
			get => EnteredTimespan == null ? Brush.Red : Brush.Transparent;
		}

		public bool IsEditMode => worklogId != null; // otherwise new worklog creating mode

		private SuggestedIssue? selectedIssue = null;
		private TimeSpan? enteredTimespan = TimeSpan.Zero;
		private DateTime dateTime = DateTime.Now;

		public Task<IEnumerable<SuggestedIssue>> GetPickerSuggestions(string query)
		{
			return client.GetPickerSuggestions(query);
		}

		public void RemovePlaceholder()
		{
			client.RemovePlaceholder();
		}

		public delegate void SaveFinishedEvent();
		public event SaveFinishedEvent? SaveFinished;

		private readonly Command saveCommand;
		public Command SaveCommand => saveCommand;
	}
}
