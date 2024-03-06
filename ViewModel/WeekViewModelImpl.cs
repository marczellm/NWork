using NWork.JiraClient;
using NWork.WeekView;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NWork.ViewModel
{
	public class WeekViewModelImpl: WeekViewModel
	{
		private readonly JiraClient.JiraClient client;
		public WeekViewModelImpl(JiraClient.JiraClient client)
		{
			this.client = client;
			PropertyChanged += WeekViewModelImpl_PropertyChanged;
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
	}
}
