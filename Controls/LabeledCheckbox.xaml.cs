using OnCheckedChanged = System.EventHandler<Microsoft.Maui.Controls.CheckedChangedEventArgs>;

namespace ElectoralMonitoring;

public partial class LabeledCheckbox : HorizontalStackLayout
{
    public static readonly BindableProperty TextProperty = BindableProperty.Create(
        propertyName: nameof(Text),
        returnType: typeof(string),
        declaringType: typeof(LabeledCheckbox),
        defaultValue: "",
        defaultBindingMode: BindingMode.OneWay);

    public string Text
    {
        get => (string)GetValue(TextProperty);
        set { SetValue(TextProperty, value); }
    }

    public static readonly BindableProperty KeyProperty = BindableProperty.Create(
        propertyName: nameof(Key),
        returnType: typeof(string),
        declaringType: typeof(LabeledCheckbox),
        defaultValue: "",
        defaultBindingMode: BindingMode.OneWay);

    public string Key
    {
        get => (string)GetValue(KeyProperty);
        set { SetValue(KeyProperty, value); }
    }

    public event OnCheckedChanged CheckedChanged;

    public bool IsChecked => CheckBox.IsChecked;

    public LabeledCheckbox()
    {
        InitializeComponent();
    }

    private void OnLabelClicked(object sender, EventArgs e)
    {
        CheckBox.IsChecked = !CheckBox.IsChecked;
    }

    private void OnCheckChanged(object sender, CheckedChangedEventArgs e)
    {
        CheckedChanged?.Invoke(this, e);
    }
}