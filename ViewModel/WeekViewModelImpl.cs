using NWork.JiraClient;
using NWork.WeekView;

namespace NWork.ViewModel
{
	public class WeekViewModelImpl: WeekViewModel, IPickerProvider
	{
		private readonly JiraClient.JiraClient client;
		public WeekViewModelImpl(JiraClient.JiraClient client)
		{
			this.client = client;
			client.LoggedIn += OnLoggedIn;
			client.LoggedOut += OnLoggedOut;
			PropertyChanged += WeekViewModelImpl_PropertyChanged;
		}

		private void OnLoggedIn(UserInfo user)
		{
			GoToday();
		}

		private void OnLoggedOut(object? sender, EventArgs e)
		{
			Events = [];
		}


		private async void WeekViewModelImpl_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Monday))
			{
				ShowSpinner = true;
				Events = [];
				var worklogs = await client.GetWorklogsBetween(Monday, Sunday + TimeSpan.FromHours(23));
				Events = worklogs.Select(worklog => new Event()
				{
					Title = worklog.issueKey,
					Started = DateTime.Parse(worklog.started),
					Duration = TimeSpan.FromSeconds(worklog.timeSpentSeconds),
					Description = worklog.issueTitle
				});
				ShowSpinner = false;
			}
		}

        Task<IEnumerable<SuggestedIssue>> IPickerProvider.GetPickerSuggestions(string query)
		{
			return client.GetPickerSuggestions(query);
		}

		public override IPickerProvider GetPickerProvider()
		{
			return this;
		}
	}
}
