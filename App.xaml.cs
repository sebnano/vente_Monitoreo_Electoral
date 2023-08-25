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

    protected override async void OnStart()
    {
        base.OnStart();

        if (!_authService.IsAuthenticated)
        {
            await Shell.Current.GoToAsync(nameof(LoginPageModel));
        }
    }
}

