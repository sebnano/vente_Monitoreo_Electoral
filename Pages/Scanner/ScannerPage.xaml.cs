namespace ProRecords;

public partial class ScannerPage : ContentPage
{
	public ScannerPage(ScannerPageModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}
