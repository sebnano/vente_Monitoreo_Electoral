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

    async void TapGestureRecognizer_Tapped(System.Object sender, Microsoft.Maui.Controls.TappedEventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    async void ClickGestureRecognizer_Clicked(System.Object sender, System.EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
