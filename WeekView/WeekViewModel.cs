using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NWork.WeekView
{
	public class Event
	{
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public DateTime Started { get; set; }
		public TimeSpan Duration { get; set; }

		public uint Column
		{
			get
			{
				if (Started.DayOfWeek == DayOfWeek.Sunday)
				{
					return 7;
				}
				else
				{
					return (uint)Started.DayOfWeek;
				}
			}
		}

		public uint Row 
		{ 
			get
			{
				var baseDate = Started.Date + TimeSpan.FromHours(7);
				return (uint)Math.Round(Started.Subtract(baseDate).TotalMinutes / 15) + 1;
			} 
		}

		public uint RowSpan
		{
			get
			{
				return (uint)Math.Round(Duration.TotalMinutes / 15);
			}
		}

		public string Tooltip
		{
			get
			{
				return $"{Description}\nStarted: {Started.TimeOfDay}\nEnded: {(Started + Duration).TimeOfDay}\nTime spent: {Duration}";
			}
		}
	}

	public abstract class WeekViewModel: INotifyPropertyChanged
	{
		private DateTime startDate;
		private bool showSpinner = false;
		private IEnumerable<Event> events = [];

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

		public TimeSpan MondayTotal
		{
			get
			{
				return Events.Where(ev => ev.Started < Tuesday).Aggregate(TimeSpan.Zero, (partialSum, ev) => partialSum + ev.Duration);
			}
		}

		public TimeSpan TuesdayTotal
		{
			get
			{
				return Events.Where(ev => Tuesday < ev.Started && ev.Started < Wednesday).Aggregate(TimeSpan.Zero, (partialSum, ev) => partialSum + ev.Duration);
			}
		}

		public TimeSpan WednesdayTotal
		{
			get
			{
				return Events.Where(ev => Wednesday < ev.Started && ev.Started < Thursday).Aggregate(TimeSpan.Zero, (partialSum, ev) => partialSum + ev.Duration);
			}
		}

		public TimeSpan ThursdayTotal
		{
			get
			{
				return Events.Where(ev => Thursday < ev.Started && ev.Started < Friday).Aggregate(TimeSpan.Zero, (partialSum, ev) => partialSum + ev.Duration);
			}
		}

		public TimeSpan FridayTotal
		{
			get
			{
				return Events.Where(ev => Friday < ev.Started && ev.Started < Saturday).Aggregate(TimeSpan.Zero, (partialSum, ev) => partialSum + ev.Duration);
			}
		}

		public TimeSpan SaturdayTotal
		{
			get
			{
				return Events.Where(ev => Saturday < ev.Started && ev.Started < Sunday).Aggregate(TimeSpan.Zero, (partialSum, ev) => partialSum + ev.Duration);
			}
		}

		public TimeSpan SundayTotal
		{
			get
			{
				return Events.Where(ev => Sunday < ev.Started).Aggregate(TimeSpan.Zero, (partialSum, ev) => partialSum + ev.Duration);
			}
		}

		public TimeSpan WeekTotal
		{
			get
			{
				return Events.Aggregate(TimeSpan.Zero, (partialSum, ev) => partialSum + ev.Duration);
			}
		}

		public IEnumerable<Event> Events
		{
			get => events; 
			set
			{
				events = value;
				RaisePropertyChanged();
				RaisePropertyChanged(nameof(MondayTotal));
				RaisePropertyChanged(nameof(TuesdayTotal));
				RaisePropertyChanged(nameof(WednesdayTotal));
				RaisePropertyChanged(nameof(ThursdayTotal));
				RaisePropertyChanged(nameof(FridayTotal));
				RaisePropertyChanged(nameof(Saturday));
				RaisePropertyChanged(nameof(SundayTotal));
				RaisePropertyChanged(nameof(WeekTotal));
			}
		}

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
