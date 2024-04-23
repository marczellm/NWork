using NWork.JiraClient;
using NWork.WeekView;
using zoft.MauiExtensions.Core.Extensions;

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
			Events.Clear();
		}


		private void WeekViewModelImpl_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(Monday))
            {
                Events.Clear();
                RefreshCurrentView();
			}
		}

		public async override void RefreshCurrentView ()
		{
            ShowSpinner = true;
            var worklogs = await client.GetWorklogsBetween(Monday, Sunday + TimeSpan.FromHours(23));
			Events.Clear();
            Events.AddRange(worklogs.Select(worklog => new Event
            {
                Id = worklog.id,
                Title = worklog.issueKey,
                Started = DateTime.Parse(worklog.started),
                Duration = TimeSpan.FromSeconds(worklog.timeSpentSeconds),
                Description = worklog.issueTitle
            }));
            ShowSpinner = false;
        }

        Task<IEnumerable<SuggestedIssue>> IPickerProvider.GetPickerSuggestions(string query)
		{
			return client.GetPickerSuggestions(query);
		}

		public override IPickerProvider GetPickerProvider()
		{
			return this;
		}

		public async Task<bool> AddWorklog(Worklog worklog)
		{
			var ret = await client.AddWorklog(worklog);
			RemovePlaceholder();
			if (ret)
			{
				RefreshCurrentView();
			}
			return ret;
		}

		public async Task<bool> EditWorklog(Worklog worklog)
		{
			var ret = await client.EditWorklog(worklog);
			if (ret)
			{
				RefreshCurrentView();
			}
			return ret;
		}

        public override async Task<bool> DeleteWorklog(Worklog worklog)
        {
            var ret = await client.DeleteWorklog(worklog);
			if (ret)
			{
				RefreshCurrentView();
			}
			return ret;
        }

		public void RemovePlaceholder()
		{
			if (CurrentlyEditedEvent != null)
			{
				Events.Remove(CurrentlyEditedEvent);
				CurrentlyEditedEvent = null;
			}
		}
    }
}
