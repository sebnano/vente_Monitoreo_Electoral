namespace ElectoralMonitoring;

public partial class ReportListPage : ContentPage
{
	public ReportListPage(ReportListPageModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
