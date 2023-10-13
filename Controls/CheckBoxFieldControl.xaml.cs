using Android.Graphics.Drawables;
using static Android.Icu.Text.CaseMap;
using static System.Net.Mime.MediaTypeNames;

namespace ElectoralMonitoring;

public partial class CheckBoxFieldControl : ContentView, IFieldControl
{
    public static List<string> TypesAvailable = new()
    {
        "boolean_checkbox"
    };

    public static readonly BindableProperty KeyProperty = BindableProperty.Create(nameof(Key), typeof(string), typeof(CheckBoxFieldControl), string.Empty, BindingMode.OneTime);
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(CheckBoxFieldControl), string.Empty);

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

    public CheckBoxFieldControl()
	{
		InitializeComponent();
	}

    public object GetValue()
    {
        return FieldControl.IsChecked ? "1" : "0";
    }

    public void SetValue(object value)
    {
        FieldControl.IsChecked = value.ToString() == "1";
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

    void FieldControl_CheckedChanged(System.Object sender, Microsoft.Maui.Controls.CheckedChangedEventArgs e)
    {
        MessagingCenter.Send(this, "CheckBoxFieldControlChanged");
    }
}
