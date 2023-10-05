namespace ElectoralMonitoring;

public partial class HomePage : ContentPage
{
	public HomePage(HomePageModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}
