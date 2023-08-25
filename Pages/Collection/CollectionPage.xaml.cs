namespace ElectoralMonitoring;

public partial class CollectionPage : ContentPage
{
	public CollectionPage(CollectionPageModel vm)
	{
		InitializeComponent();
        BindingContext = vm;
    }
}
