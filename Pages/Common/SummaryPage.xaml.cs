namespace ElectoralMonitoring;

public partial class SummaryPage : ContentPage
{
	public SummaryPage(SummaryPageModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}
