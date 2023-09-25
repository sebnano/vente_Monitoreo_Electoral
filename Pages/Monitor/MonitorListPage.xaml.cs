namespace ElectoralMonitoring;

public partial class MonitorListPage : ContentPage
{
	MonitorListPageModel _vm;
	public MonitorListPage(MonitorListPageModel vm)
	{
		InitializeComponent();
		vm.ContextPage = this;
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _vm.Init().ConfigureAwait(false);
    }
}
