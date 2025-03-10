﻿using CommunityToolkit.Mvvm.ComponentModel;
using NWork.JiraClient;
using System.Collections.ObjectModel;

namespace NWork.WeekView
{
	public class Event: ObservableObject
	{
		private TimeSpan duration;
		private DateTime started;

		public string Id { get; set; } = string.Empty;
		public string Title { get; set; } = string.Empty;
		public string Description { get; set; } = string.Empty;
		public DateTime Started
		{
			get => started; 
			set
			{
				OnPropertyChanged();
				OnPropertyChanged(nameof(Column));
				OnPropertyChanged(nameof(Row));
				OnPropertyChanged(nameof(Tooltip));
				started = value;
			}
		}
		public TimeSpan Duration
		{
			get => duration; 
			set
			{
				OnPropertyChanged();
				OnPropertyChanged(nameof(RowSpan));
				OnPropertyChanged(nameof(Tooltip));
				duration = value;
			}
		}

		public bool IsPlaceholder { get; set; } = false; // dragging the mouse will create a temporary rect that looks different

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

	public interface IPickerProvider
	{
		Task<IEnumerable<SuggestedIssue>> GetPickerSuggestions(string query);
		Task<bool> AddWorklog(Worklog worklog);
		Task<bool> EditWorklog(Worklog worklog);
		void RemovePlaceholder();
	}

	public abstract partial class WeekViewModel: ObservableObject
	{
		private DateTime startDate;

		[ObservableProperty]
		private bool showSpinner = false;

		internal Event? CurrentlyEditedEvent = null;

		protected WeekViewModel() 
		{
			Events.CollectionChanged += (sender, e) =>
			{
				OnPropertyChanged(nameof(MondayTotal));
				OnPropertyChanged(nameof(TuesdayTotal));
				OnPropertyChanged(nameof(WednesdayTotal));
				OnPropertyChanged(nameof(ThursdayTotal));
				OnPropertyChanged(nameof(FridayTotal));
				OnPropertyChanged(nameof(SaturdayTotal));
				OnPropertyChanged(nameof(SundayTotal));
				OnPropertyChanged(nameof(WeekTotal));
				OnPropertyChanged(nameof(WeekTotalHours));
			};
			GoToday();
		}

		public DateTime Monday
		{
			get => startDate;
			set
			{
				startDate = value;
				OnPropertyChanged();
				OnPropertyChanged(nameof(Tuesday));
				OnPropertyChanged(nameof(Wednesday));
				OnPropertyChanged(nameof(Thursday));
				OnPropertyChanged(nameof(Friday));
				OnPropertyChanged(nameof(Saturday));
				OnPropertyChanged(nameof(Sunday));
				OnPropertyChanged(nameof(IsMondayToday));
                OnPropertyChanged(nameof(IsTuesdayToday));
                OnPropertyChanged(nameof(IsWednesdayToday));
                OnPropertyChanged(nameof(IsThursdayToday));
                OnPropertyChanged(nameof(IsFridayToday));
                OnPropertyChanged(nameof(IsSaturdayToday));
                OnPropertyChanged(nameof(IsSundayToday));
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

		public uint WeekTotalHours 
		{
			get
			{
				return (uint) Math.Floor(WeekTotal.TotalHours);
			}
		}

		public ObservableCollection<Event> Events { get; set; } = [];

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

		public FontAttributes IsMondayToday => IsToday(Monday);
		public FontAttributes IsTuesdayToday => IsToday(Tuesday);
		public FontAttributes IsWednesdayToday => IsToday(Wednesday);
		public FontAttributes IsThursdayToday => IsToday(Thursday);
		public FontAttributes IsFridayToday => IsToday(Friday);
		public FontAttributes IsSaturdayToday => IsToday(Saturday);
		public FontAttributes IsSundayToday => IsToday(Sunday);

        private static FontAttributes IsToday(DateTime date)
        {
            var ret = new FontAttributes();
            if (date == DateTime.Today)
            {
                ret = FontAttributes.Bold;
            }
            return ret;
        }

		public abstract IPickerProvider GetPickerProvider();

        public abstract Task<bool> DeleteWorklog(Worklog worklog);

		public abstract void RefreshCurrentView();

		public abstract void LaunchIssueInBrowser(string issueKey);
    }
}
