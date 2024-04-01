using NWork.JiraClient;
using NWork.WeekView;
using System.Collections.ObjectModel;
using zoft.MauiExtensions.Core.Extensions;

namespace NWork.Pages;

public partial class EditPage : ContentPage
{
    public ObservableCollection<SuggestedIssue> SearchResults { get; set; } = [];

    public SuggestedIssue? SelectedIssue
    {
        get => selectedIssue; 
        set
        {
            selectedIssue = value;
            OnPropertyChanged(nameof(SaveEnabled));
        }
    }

    public bool SaveEnabled => SelectedIssue != null;

    private readonly WeekViewModel viewModel;
    private SuggestedIssue? selectedIssue = null;

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
        if (IssueSearchBar.Text.IsNullOrEmpty()) 
        {
            SelectedIssue = null;
            return;
        }
        SearchResults.AddRange(await viewModel.GetPickerSuggestions(IssueSearchBar.Text));
    }

    private void Save(object sender, EventArgs e)
    {

    }
}