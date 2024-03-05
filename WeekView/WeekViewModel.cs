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
		private bool showSpinner = false;

		public DateTime Monday
		{
			get => startDate;
			set
			{
				startDate = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(Tuesday));
				RaisePropertyChanged(nameof(Wednesday));
				RaisePropertyChanged(nameof(Thursday));
				RaisePropertyChanged(nameof(Friday));
				RaisePropertyChanged(nameof(Saturday));
				RaisePropertyChanged(nameof(Sunday));
			}
		}
		public DateTime Tuesday { get {  return Monday + TimeSpan.FromDays(1); } }
		public DateTime Wednesday { get { return Monday + TimeSpan.FromDays(2); } }
		public DateTime Thursday { get { return Monday + TimeSpan.FromDays(3); } }
		public DateTime Friday { get { return Monday + TimeSpan.FromDays(4); } }
		public DateTime Saturday { get { return Monday + TimeSpan.FromDays(5); } }
		public DateTime Sunday { get { return Monday + TimeSpan.FromDays(6); } }

		public bool ShowSpinner
		{
			get => showSpinner; 
			set
			{
				showSpinner = value;
				RaisePropertyChanged();
			}
		}

		protected WeekViewModel()
		{
			GoToday();
		}

		public void PrevWeek()
		{
			Monday = Monday.AddDays(-7);
		}

		public void NextWeek()
		{
			Monday = Monday.AddDays(7);
		}

		public void GoToday()
		{
			int diff = (7 + (DateTime.UtcNow.DayOfWeek - DayOfWeek.Monday)) % 7;
			Monday = DateTime.UtcNow.AddDays(-1 * diff).Date;
		}

		public event PropertyChangedEventHandler? PropertyChanged;
		protected void RaisePropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
