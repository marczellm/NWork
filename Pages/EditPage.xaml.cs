using NWork.JiraClient;
using NWork.WeekView;
using System.Collections.ObjectModel;
using zoft.MauiExtensions.Core.Extensions;

namespace NWork.Pages;

public partial class EditPage : ContentPage
{
    public ObservableCollection<SuggestedIssue> SearchResults { get; set; } = [];
    private readonly WeekViewModel viewModel;

	public EditPage(WeekViewModel weekViewModel)
	{
        this.viewModel = weekViewModel;
		InitializeComponent();
	}

    private void OnCancel(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private async void AutoCompleteEntry_TextChanged(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntryTextChangedEventArgs e)
    {
        SearchResults.Clear();
        SearchResults.AddRange(await viewModel.GetPickerSuggestions(IssueSearchBar.Text));
    }
}