namespace ElectoralMonitoring;

public partial class MonitorListPage : ContentPage
{
	public MonitorListPage(MonitorListPageModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}
