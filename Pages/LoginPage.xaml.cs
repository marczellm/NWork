namespace NWork.Pages;

public partial class LoginPage : ContentPage
{
	public LoginPage()
	{
		InitializeComponent();
	}

	private void ManageAPITokens(object sender, TappedEventArgs e)
	{
		Launcher.OpenAsync("https://id.atlassian.com/manage-profile/security/api-tokens");
	}

	private void DoLogin(object sender, EventArgs e)
	{
		Navigation.PopModalAsync();
	}
}