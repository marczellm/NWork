namespace NWork
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			ViewModel = new(client);
			InitializeComponent();
			LogoutButton.Clicked += Logout_Clicked;
			LogoutButton.IconImageSource = imageSource;
		}

		public bool LoggedIn { get; set; } = false;
		public string UserFullName { get; private set; } = string.Empty;

		private readonly UriImageSource imageSource = new();
		private readonly ToolbarItem LogoutButton = new()
		{
			Text = "Log out"
		};
		private JiraClient.JiraClient client = new();
		public ViewModel.WeekViewModelImpl ViewModel { get; set; }

		private async void Login_Clicked(object sender, EventArgs e)
		{
			var loggedInUser = await client.GetUser();
			UserFullName = loggedInUser.displayName;
			imageSource.Uri = new Uri(loggedInUser.avatarUrls["48x48"]);
			ToolbarItems.Remove(LoginButton);
			ToolbarItems.Add(LogoutButton);
		}

		private void Logout_Clicked(object? sender, EventArgs e)
		{
			ToolbarItems.Add(LoginButton);
			ToolbarItems.Remove(LogoutButton);
		}
	}

}
