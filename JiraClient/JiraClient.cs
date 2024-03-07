﻿using System;
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

	class WorklogResult
	{
		public uint startAt { get; set; }
		public uint maxResults { get; set; }
		public uint total { get; set; }
		public IEnumerable<Worklog> worklogs { get; set; } = [];
	}

	class IssueFields
	{
		public string summary { get; set; } = string.Empty;
		public WorklogResult? worklog { get; set; }
	}

	class Issue
	{
		public string key { get; set; } = string.Empty;
		public IssueFields? fields { get; set; }
	}

	class SearchResult
	{
		public IEnumerable<Issue> issues { get; set; } = [];
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

		async Task<WorklogResult> GetIssueWorklogs(string idOrKey)
		{
			var response = await client.GetAsync("issue/" + idOrKey + "/worklog");
			return await response.Content.ReadAsAsync<WorklogResult>();
		}

		public async Task<IEnumerable<Worklog>> GetWorklogsBetween(DateTime start, DateTime end)
		{
			var url = $"search?jql=worklogAuthor in (currentUser()) and worklogDate >= '{start:yyyy-MM-dd}' and worklogDate <= '{end:yyyy-MM-dd}'&fields=summary,worklog&maxResults=200";
			var response = await client.GetAsync(url);
			var resstr = await response.Content.ReadAsStringAsync();
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
	}
}
