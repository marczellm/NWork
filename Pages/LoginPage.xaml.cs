using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NWork.Pages;

public partial class LoginPage : ContentPage
{
	private JiraClient.JiraClient client;
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

	public LoginPage(JiraClient.JiraClient client, string initUserName, string initToken)
	{
		this.client = client;
		InitializeComponent();
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
		if (await client.Login(UsernameField.Text, TokenField.Text))
		{
			await Navigation.PopModalAsync();
		}
		else
		{
			ErrorVisible = true;
		}		
	}
}