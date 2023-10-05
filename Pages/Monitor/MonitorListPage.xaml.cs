namespace ElectoralMonitoring;

public partial class MonitorListPage : ContentPage
{
	MonitorListPageModel _vm;
	public MonitorListPage(MonitorListPageModel vm)
	{
		InitializeComponent();
        BindingContext = _vm = vm;
        vm.ContextPage = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _vm.Init().ConfigureAwait(false);
    }
}
