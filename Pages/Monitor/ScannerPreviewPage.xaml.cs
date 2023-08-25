namespace ElectoralMonitoring;

public partial class ScannerPreviewPage : ContentPage
{
	public ScannerPreviewPage(ScannerPreviewPageModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
