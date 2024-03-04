using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Web.Http.Headers;

namespace NWork.JiraClient
{
	public class User
	{
		public string displayName { get; set; } = string.Empty;
		public Dictionary<string, string> avatarUrls { get; set; } = new();
	}

	public class JiraClient
	{
		private HttpClient client = new();

		public string Username { get; set; } = "mmarczell@graphisoft.com";
		public string AccountId { get; set; } = "5c6bec9cb5b4a2652dac59e9";
		public string APIToken { get; set; } = "ATATT3xFfGF06D-kKeFGdLnKgbassEeGBhGDRX9-zGonInmyzGCzFIZ72j6cJh7W0647Tx8Sz1ucwQww70z4ihx7GHXnqwG3QkQncdRxXXOJznrOUXJpJcVadmEjyQxhxq8G-1UBw3Rbtn2Ip1wUK5kCJPe7uJNy7gYgKb17lCgCrpfkEkzbtHo=D488AD93";

		public JiraClient()
		{
			Login();
		}

		public void Login()
		{
			var buffer = CryptographicBuffer.ConvertStringToBinary(Username + ":" + APIToken, BinaryStringEncoding.Utf8);
			string base64token = CryptographicBuffer.EncodeToBase64String(buffer);
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64token);
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.BaseAddress = new Uri("https://graphisoft.atlassian.net/rest/api/3/");
			
		}

		public async Task<User> GetUser()
		{
			var response = await client.GetAsync("user?accountId=" + AccountId);
			//var str = await response.Content.ReadAsStringAsync();
			var ret = await response.Content.ReadAsAsync<User>();
			return ret;
		}
	}
}
