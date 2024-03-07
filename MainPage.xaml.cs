using NWork.JiraClient;
using NWork.Pages;

namespace NWork
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			client.LoggedIn += OnLoggedIn;
			client.LoggedOut += OnLoggedOut;
			ViewModel = new(client);
			InitializeComponent();
			LogoutButton.Clicked += Logout_Clicked;
			LogoutButton.IconImageSource = imageSource;

			// Get previous login from preferences

			string username = Preferences.Default.Get("username", "");
			string apitoken = Preferences.Default.Get("apitoken", "");
			if (username != "" && apitoken != "")
			{
				_ = client.Login(username, apitoken);
			}
		}

		public bool LoggedIn { get; set; } = false;
		public string UserFullName { get; private set; } = string.Empty;

		private readonly UriImageSource imageSource = new();
		private readonly ToolbarItem LogoutButton = new()
		{
			Text = "Log out"
		};
		private readonly JiraClient.JiraClient client = new();
		public ViewModel.WeekViewModelImpl ViewModel { get; set; }

		private async void Login_Clicked(object sender, EventArgs e)
		{
			await Navigation.PushModalAsync(new LoginPage(client,
				Preferences.Default.Get("username", ""),
				Preferences.Default.Get("apitoken", "")));
		}

		private void Logout_Clicked(object? sender, EventArgs e)
		{
			client.Logout();
		}

		private void OnLoggedIn(UserInfo user)
		{
			UserFullName = user.displayName;
			imageSource.Uri = new Uri(user.avatarUrls["48x48"]);
			ToolbarItems.Remove(LoginButton);
			ToolbarItems.Add(LogoutButton);
		}

		private void OnLoggedOut(object? sender, EventArgs e)
		{
			ToolbarItems.Add(LoginButton);
			ToolbarItems.Remove(LogoutButton);
		}
	}

}
