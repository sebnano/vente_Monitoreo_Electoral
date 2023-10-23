namespace ElectoralMonitoring;

public partial class App : Application
{
    readonly AuthService _authService;
    public App(AuthService authService)
	{
        _authService = authService;
		InitializeComponent();

		MainPage = new AppShell();
	}

    protected override void OnStart()
    {
        base.OnStart();

        if (!_authService.IsAuthenticated)
        {
            Shell.Current.Dispatcher.Dispatch(async() => await Shell.Current.GoToAsync(nameof(LoginPageModel)));
        }
    }
}

