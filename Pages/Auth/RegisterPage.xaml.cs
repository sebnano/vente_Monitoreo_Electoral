namespace ProRecords;

public partial class RegisterPage : ContentPage
{
	public RegisterPage(RegisterPageModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
	}

    protected override bool OnBackButtonPressed()
    {
        return true;
    }

    async void ClickGestureRecognizer_Clicked(System.Object sender, System.EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(LoginPageModel));
    }
}
