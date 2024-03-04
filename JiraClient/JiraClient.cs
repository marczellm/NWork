using System;
using System.Buffers.Text;
using System.Collections.Generic;
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
		public IEnumerable<UpdatedWorklog> values { get; set; } = [];
	}

	public class Worklog
	{
		public User? author { get; set; }
		public string issueId { get; set; } = string.Empty;
		public string started { get; set; } = string.Empty;
		public int timeSpentSeconds { get; set; }
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
			var response = await client.GetAsync("user?accountId=" + AccountId);
			//var str = await response.Content.ReadAsStringAsync();
			var ret = await response.Content.ReadAsAsync<UserInfo>();
			return ret;
		}

		public async Task<IEnumerable<Worklog>> GetWorklogs()
		{
			long utcDate = new DateTimeOffset(new DateTime(2024, 1, 1)).ToUnixTimeMilliseconds();
			var response = await client.GetAsync("worklog/updated?since=" + utcDate);
			var logIds = await response.Content.ReadAsAsync<UpdatedWorklogs>();
			if (logIds == null)
			{
				return [];
			}
			int[] logIdList = logIds.values.Select(worklog => worklog.worklogId).ToArray();
			var logDetailsResponse = await client.PostAsJsonAsync("worklog/list", logIdList);
			var str = await logDetailsResponse.Content.ReadAsStringAsync();
			var ret = await logDetailsResponse.Content.ReadAsAsync<IEnumerable<Worklog>>();
			return ret;
		}
	}
}
