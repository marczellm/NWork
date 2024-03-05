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
		private JiraClient.JiraClient client;
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
				await client.GetWorklogsBetween(Monday, Sunday);
				ShowSpinner = false;
			}
		}
	}
}
