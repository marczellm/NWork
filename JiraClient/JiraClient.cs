using System.Net.Http.Headers;
using System.Text;
using zoft.MauiExtensions.Core.Extensions;

namespace NWork.JiraClient
{
	public class User
	{
		public string displayName { get; set; } = string.Empty;
		public string accountId { get; set; } = string.Empty;
	}

	public class UserInfo : User
	{
		public Dictionary<string, string> avatarUrls { get; set; } = [];
	}

	public class WorklogResult
	{
		public uint startAt { get; set; }
		public uint maxResults { get; set; }
		public uint total { get; set; }
		public IEnumerable<Worklog2> worklogs { get; set; } = [];
	}

	public class IssueFields
	{
		public string summary { get; set; } = string.Empty;
		public WorklogResult? worklog { get; set; }
	}

	public class Issue
	{
		public string key { get; set; } = string.Empty;
		public IssueFields? fields { get; set; }
	}

	public class SuggestedIssue: Issue
	{
		public string id {  get; set; }	= string.Empty;
		public string summaryText { get; set; } = string.Empty;
		public string FullText => key + " " + summaryText;
	}

	class IssuePickerSuggestionSection
	{
        public IEnumerable<SuggestedIssue> issues { get; set; } = [];
    }

	class IssuePickerSuggestions
	{
        public IEnumerable<IssuePickerSuggestionSection> sections { get; set; } = [];
    }

	class SearchResult
	{
		public IEnumerable<Issue> issues { get; set; } = [];
	}

	public class Worklog
	{
        public User? author { get; set; }
        public string id { get; set; } = string.Empty;
        public string issueId { get; set; } = string.Empty;
        public string started { get; set; } = string.Empty;
        public int timeSpentSeconds { get; set; }

        public string issueKey { get; set; } = string.Empty;
        public string issueTitle { get; set; } = string.Empty;
    }

	public class Worklog2: Worklog
	{
		public DateTime startDate { get { return DateTime.Parse(started); } }
		public DateTime endDate { get { return new DateTimeOffset(startDate).Add(TimeSpan.FromSeconds(timeSpentSeconds)).UtcDateTime;  } }
	}

	public class JiraClient
	{
		private readonly HttpClient client = new()
		{
			BaseAddress = new Uri("https://graphisoft.atlassian.net/rest/api/3/")
		};

		public JiraClient()
		{
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
		}

		public async Task<bool> Login(string username, string apitoken)
		{
			var buffer = Encoding.UTF8.GetBytes(username + ":" + apitoken);
			string base64token = Convert.ToBase64String(buffer);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64token);

			var user = await GetUser();
			if (user != null)
			{
				Preferences.Default.Set("username", username);
				Preferences.Default.Set("apitoken", apitoken);
				LoggedIn?.Invoke(user);
				return true;
			}
			return false;
		}

		public void Logout()
		{
			client.DefaultRequestHeaders.Authorization = null;
			LoggedOut?.Invoke(this, new());
		}

		public delegate void LoginSuccessHandler(UserInfo user);
		public event LoginSuccessHandler? LoggedIn;
		public event EventHandler? LoggedOut;

		// Returns null on error
		public async Task<UserInfo?> GetUser()
		{
			var response = await client.GetAsync("myself");
			try
			{
				response.EnsureSuccessStatusCode();
			}
			catch (HttpRequestException)
			{
				return null;
			}
			return await response.Content.ReadAsAsync<UserInfo>();
		}

		async Task<WorklogResult> GetIssueWorklogs(string idOrKey)
		{
			var response = await client.GetAsync($"issue/{idOrKey}/worklog");
			return await response.Content.ReadAsAsync<WorklogResult>();
		}

		public async Task<IEnumerable<Worklog2>> GetWorklogsBetween(DateTime start, DateTime end)
		{
			var url = $"search?jql=worklogAuthor in (currentUser()) and worklogDate >= '{start:yyyy-MM-dd}' and worklogDate <= '{end:yyyy-MM-dd}'&fields=summary,worklog&maxResults=200";
			var response = await client.GetAsync(url);
			var result = await response.Content.ReadAsAsync<SearchResult>();
			foreach (var issue in result.issues)
			{
				var worklogResult = issue.fields!.worklog!;
				if (worklogResult.total > worklogResult.maxResults)
				{
					worklogResult = await GetIssueWorklogs(issue.key);
				}
				foreach (var worklog in worklogResult.worklogs)
				{					
					worklog.issueKey = issue.key;
					worklog.issueTitle = issue.fields!.summary;
				}
				issue.fields!.worklog = worklogResult;
			}
			var ret = result.issues.SelectMany(issue => issue.fields!.worklog!.worklogs).Where(worklog => worklog.startDate >= start && worklog.endDate <= end);
			return ret;
		}

		public async Task<SuggestedIssue?> GetIssue(string idOrKey)
		{
			var response = await client.GetAsync("issue/" + idOrKey);
			if (response.IsSuccessStatusCode)
			{
				var ret = await response.Content.ReadAsAsync<SuggestedIssue>();
				if (ret.summaryText.IsNullOrEmpty() && ret.fields != null)
				{
					ret.summaryText = ret.fields.summary;
				}
				return ret;
			}
			else return null;
		}

		public async Task<IEnumerable<SuggestedIssue>> GetPickerSuggestions(string query)
		{
            var response = await client.GetAsync("issue/picker?currentJQL=&query=" + query);
			var result = await response.Content.ReadAsAsync<IssuePickerSuggestions>();
			var ret = result.sections.SelectMany(section => section.issues);
			return ret;
        }

		public async Task<bool> AddWorklog(Worklog worklog)
		{
			string issueIdOrKey = ! worklog.issueId.IsNullOrEmpty () ? worklog.issueId : worklog.issueKey;
			var response = await client.PostAsJsonAsync($"issue/{issueIdOrKey}/worklog", worklog);
			return response.IsSuccessStatusCode;
        }

		public async Task<bool> EditWorklog(Worklog worklog)
		{
			string issueIdOrKey = !worklog.issueId.IsNullOrEmpty() ? worklog.issueId : worklog.issueKey;
			var response = await client.PutAsJsonAsync($"issue/{issueIdOrKey}/worklog/{worklog.id}", worklog);
			return response.IsSuccessStatusCode;
		}

		public async Task<bool> DeleteWorklog(Worklog worklog)
		{
            string issueIdOrKey = !worklog.issueId.IsNullOrEmpty() ? worklog.issueId : worklog.issueKey;
            var response = await client.DeleteAsync($"issue/{issueIdOrKey}/worklog/{worklog.id}");
			return response.IsSuccessStatusCode;
		}
	}
}
