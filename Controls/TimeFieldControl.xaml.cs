namespace ElectoralMonitoring;

public partial class TimeFieldControl : ContentView, IFieldControl
{

    public static List<string> TypesAvailable = new()
    {
        "time_widget"
    };

    public static readonly BindableProperty TimeProperty = BindableProperty.Create(nameof(Time), typeof(TimeSpan), typeof(TimeFieldControl), DateTime.Now.TimeOfDay, BindingMode.TwoWay);
    public static readonly BindableProperty KeyProperty = BindableProperty.Create(nameof(Key), typeof(string), typeof(TimeFieldControl), string.Empty, BindingMode.OneTime);
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(TimeFieldControl), string.Empty);
    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(TimeFieldControl), null);

    public TimeSpan Time
    {
        get => (TimeSpan)GetValue(TimeProperty);
        set => SetValue(TimeProperty, value);
    }

    public string Icon
    {
        get => (string)GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Key
    {
        get => (string)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public TimeFieldControl()
	{
		InitializeComponent();
	}

    public object GetValue()
    {
        return (Time.Hours * 3600) + (Time.Minutes * 60);
    }

    public void SetValue(object value)
    {
        throw new NotImplementedException();
    }

    public bool HasValue()
    {
        return true;
    }

    public bool IsRequired()
    {
        return false;
    }

    public string GetKey()
    {
        return Key;
    }

    public void SetRequiredStatus()
    {
        
    }
}
