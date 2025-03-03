namespace NWork.Pages;

public partial class LoginPage : ContentPage
{
	private readonly JiraClient.JiraClient client;
	private bool errorVisible = false;

	public bool ErrorVisible
	{
		get => errorVisible; 
		
		set
		{
			errorVisible = value;
			OnPropertyChanged(nameof(ErrorVisible));
		}
	}

	public LoginPage(JiraClient.JiraClient client, string initSiteUrl, string initUserName, string initToken)
	{
		this.client = client;
		InitializeComponent();
		SiteUrlField.Text = initSiteUrl;
		UsernameField.Text = initUserName;
		TokenField.Text = initToken;
	}

	private void ManageAPITokens(object sender, TappedEventArgs e)
	{
		Launcher.OpenAsync("https://id.atlassian.com/manage-profile/security/api-tokens");
	}

	private async void DoLogin(object sender, EventArgs e)
	{
		ErrorVisible = false;
		if (await client.Login(SiteUrlField.Text, UsernameField.Text, TokenField.Text))
		{
			await Navigation.PopModalAsync();
		}
		else
		{
			ErrorVisible = true;
		}		
	}

	private void OnCancel(object sender, EventArgs e)
	{
		Navigation.PopModalAsync();
	}
}