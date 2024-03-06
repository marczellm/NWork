using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Web.Http.Headers;

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

	class UpdatedWorklog
	{
		public int worklogId { get; set; }
	}

	class UpdatedWorklogs
	{
		public string nextPage { get; set; } = string.Empty;
		public bool lastPage { get; set; } = false;
		public IEnumerable<UpdatedWorklog> values { get; set; } = [];
	}

	class IssueFields
	{
		public string summary { get; set; } = string.Empty;
	}

	class Issue
	{
		public string key { get; set; } = string.Empty;
		public IssueFields? fields { get; set; }
	}

	public class Worklog
	{
		public User? author { get; set; }
		public string issueId { get; set; } = string.Empty;
		public string started { get; set; } = string.Empty;
		public int timeSpentSeconds { get; set; }

		public string issueKey { get; set; } = string.Empty;
		public string issueTitle { get; set; } = string.Empty;
		public DateTime startDate { get { return DateTime.Parse(started); } }
		public DateTime endDate { get { return new DateTimeOffset(startDate).Add(TimeSpan.FromSeconds(timeSpentSeconds)).UtcDateTime;  } }
	}

	public class JiraClient
	{
		private HttpClient client = new();

		public string Username { get; set; } = "mmarczell@graphisoft.com";
		public string AccountId { get; set; } = "5c6bec9cb5b4a2652dac59e9";
		public string APIToken { get; set; } = string.Empty;

		private List<Worklog> worklogs = [];
		private DateTime earliestWorklogUpdateDate = DateTime.Now;

		public JiraClient()
		{
			Login();
		}

		public void Login()
		{
			APIToken = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "../../../../../token.txt");
			var buffer = CryptographicBuffer.ConvertStringToBinary(Username + ":" + APIToken, BinaryStringEncoding.Utf8);
			string base64token = CryptographicBuffer.EncodeToBase64String(buffer);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64token);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.BaseAddress = new Uri("https://graphisoft.atlassian.net/rest/api/3/");
		}

		public async Task<UserInfo> GetUser()
		{
			var response = await client.GetAsync("myself");
			return await response.Content.ReadAsAsync<UserInfo>();
		}

		async Task<Issue> GetIssue(string id)
		{
			var response = await client.GetAsync("issue/" + id + "?fields=summary");
			return await response.Content.ReadAsAsync<Issue>();
		}

		public async Task<IEnumerable<Worklog>> GetWorklogsBetween(DateTime start, DateTime end)
		{
			if (start < earliestWorklogUpdateDate)
			{
				await RefreshWorklogsUpdatedSince(start);
			}
			return worklogs.Where(worklog => start <= worklog.startDate && worklog.startDate <= end);
		}

		public async Task RefreshWorklogsUpdatedSince(DateTime since)
		{
			if (since < earliestWorklogUpdateDate)
			{
				earliestWorklogUpdateDate = since;
			}
			else
			{
				since = earliestWorklogUpdateDate;
			}

			long utcDate = new DateTimeOffset(since).ToUnixTimeMilliseconds();

			bool lastPage = false;
			List<Worklog> ret = new();
			string url = "worklog/updated?since=" + utcDate;
			while (! lastPage)
			{
				var response = await client.GetAsync(url);
				var logIds = await response.Content.ReadAsAsync<UpdatedWorklogs>();
				if (logIds == null)
				{
					break;
				}
				int[] logIdList = logIds.values.Select(worklog => worklog.worklogId).ToArray();
				var logDetailsResponse = await client.PostAsJsonAsync("worklog/list", new Dictionary<string, int[]>
				{
					{ "ids", logIdList }
				});
				ret.AddRange(await logDetailsResponse.Content.ReadAsAsync<IEnumerable<Worklog>>());
				lastPage = logIds.lastPage;
				url = logIds.nextPage;
			}
			
			worklogs = ret.Where(worklog => worklog.author!.accountId == AccountId).ToList();

			foreach (var worklog in worklogs)
			{
				var issue = await GetIssue(worklog.issueId);
				worklog.issueKey = issue.key;
				worklog.issueTitle = issue.fields!.summary;
			}
		}
	}
}
