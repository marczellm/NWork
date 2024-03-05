using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace NWork.WeekView
{
	public class Event
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public DateTime Started { get; set; }
		public TimeSpan Duration { get; set; }
	}

	public abstract class WeekViewModel: INotifyPropertyChanged
	{
		private DateTime startDate;

		public DateTime StartDate
		{
			get => startDate;
			set
			{
				startDate = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(EndDate));
			}
		}
		public DateTime EndDate { get {  return StartDate + TimeSpan.FromDays(4); } }

		protected WeekViewModel()
		{
			GoToday();
		}

		public void PrevWeek()
		{
			StartDate = StartDate.AddDays(-7);
		}

		public void NextWeek()
		{
			StartDate = StartDate.AddDays(7);
		}

		public void GoToday()
		{
			int diff = (7 + (DateTime.UtcNow.DayOfWeek - DayOfWeek.Monday)) % 7;
			StartDate = DateTime.UtcNow.AddDays(-1 * diff).Date;
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
