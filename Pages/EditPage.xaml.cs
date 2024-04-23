using zoft.MauiExtensions.Core.Extensions;

namespace NWork.Pages;

public partial class EditPage : ContentPage
{
    public EditPageViewModel ViewModel {  get; private set; }

    public EditPage(EditPageViewModel weekViewModel)
    {
        ViewModel = weekViewModel;
        ViewModel.SaveFinished += () =>
        {
			Navigation.PopModalAsync();
		};
		InitializeComponent();
	}

    private void OnCancel(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
        ViewModel.RemovePlaceholder();
    }

    private async void AutoCompleteEntry_TextChanged(object sender, zoft.MauiExtensions.Controls.AutoCompleteEntryTextChangedEventArgs e)
    {
        ViewModel.SearchResults.Clear();
        if (IssueSearchBar.Text.IsNullOrEmpty()) 
        {
			ViewModel.SelectedIssue = null;
            return;
        }
        var results = await ViewModel.GetPickerSuggestions(IssueSearchBar.Text);
        ViewModel.SearchResults.AddRange(results);
#if MACCATALYST
        IssueSearchBar.IsSuggestionListOpen = ViewModel.SearchResults.Count > 0;
#endif
    }

    private void Save(object sender, EventArgs e)
    {
        Spinner.IsRunning = true;
    }

	private void TimeSpent_TextChanged(object sender, TextChangedEventArgs e)
	{
		if (TimeSpan.TryParse(e.NewTextValue, out TimeSpan result))
		{
			ViewModel.EnteredTimespan = result;
		}
		else
		{
			ViewModel.EnteredTimespan = null;
		}
	}
}