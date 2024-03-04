namespace NWork
{
	public partial class AppShell : Shell
	{
		public bool LoggedIn { get; set; } = false;
		public string UserFullName { get; private set; } = string.Empty;

		private readonly UriImageSource imageSource = new();
		private readonly ToolbarItem LogoutButton = new()
		{
			Text = "Log out"
		};
		private readonly JiraClient.JiraClient client = new();

		public AppShell()
		{
			InitializeComponent();
			BindingContext = this;
			LogoutButton.Clicked += Logout_Clicked;
			LogoutButton.IconImageSource = imageSource;
		}

		private async void Login_Clicked(object sender, EventArgs e)
		{
			var loggedInUser = await client.GetUser();
			UserFullName = loggedInUser.displayName;
			imageSource.Uri = new Uri(loggedInUser.avatarUrls.First().Value);
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
