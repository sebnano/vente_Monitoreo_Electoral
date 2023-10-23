namespace ElectoralMonitoring;

public partial class LoginPage : ContentPage
{
	public LoginPage(LoginPageModel vm)
	{
        InitializeComponent();
        BindingContext = vm;
    }

    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}
