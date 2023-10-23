namespace ElectoralMonitoring;

public partial class ReportFormPage : ContentPage
{
	public ReportFormPage(ReportFormPageModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
