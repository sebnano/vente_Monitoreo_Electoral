namespace ElectoralMonitoring;

public partial class MonitorListPage : ContentPage
{
	public MonitorListPage(MonitorListPageModel vm)
	{
		InitializeComponent();
		vm.ContextPage = this;
        BindingContext = vm;

    }
}
