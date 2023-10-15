namespace ElectoralMonitoring;

public enum SelectFieldType
{
    Centros,
    SelectOpts
}
public class SelectOption
{
    public string Key { get; set; }
    public string Value { get; set; }

    public override string ToString()
    {
        return Value;
    }
}
public partial class OptionsSelectFieldControl : ContentView, IFieldControl
{
    public static List<string> TypesAvailable = new()
    {
        "options_select"
    };

    public static readonly BindableProperty KeyProperty = BindableProperty.Create(nameof(Key), typeof(string), typeof(OptionsSelectFieldControl), string.Empty, BindingMode.OneTime);
    public static readonly BindableProperty TitleProperty = BindableProperty.Create(nameof(Title), typeof(string), typeof(OptionsSelectFieldControl), string.Empty);
    public static readonly BindableProperty PlaceholderProperty = BindableProperty.Create(nameof(Placeholder), typeof(string), typeof(OptionsSelectFieldControl), string.Empty);
    public static readonly BindableProperty FieldTypeProperty = BindableProperty.Create(nameof(FieldType), typeof(SelectFieldType), typeof(OptionsSelectFieldControl), SelectFieldType.SelectOpts, propertyChanged: OnFieldTypePropertyChanged);
    public static readonly BindableProperty IconProperty = BindableProperty.Create(nameof(Icon), typeof(string), typeof(OptionsSelectFieldControl), null);
    public static readonly BindableProperty IsRequiredFieldProperty = BindableProperty.Create(nameof(IsRequiredField), typeof(bool), typeof(OptionsSelectFieldControl), null);

    static void OnFieldTypePropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is OptionsSelectFieldControl control)
        {
            if (newValue is SelectFieldType type)
            {
                switch (type)
                {
                    case SelectFieldType.Centros:
                        //carga desde centros de votacion del usuario
                        break;
                    case SelectFieldType.SelectOpts:
                        //carga picker desde valores
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public bool IsRequiredField
    {
        get => (bool)GetValue(IsRequiredFieldProperty);
        set => SetValue(IsRequiredFieldProperty, value);
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

    public string Placeholder
    {
        get => (string)GetValue(PlaceholderProperty);
        set => SetValue(PlaceholderProperty, value);
    }

    public string Key
    {
        get => (string)GetValue(KeyProperty);
        set => SetValue(KeyProperty, value);
    }

    public FieldType FieldType
    {
        get => (FieldType)GetValue(FieldTypeProperty);
        set => SetValue(FieldTypeProperty, value);
    }

    public List<SelectOption> Options { get; set; }

    public OptionsSelectFieldControl()
	{
		InitializeComponent();
	}

    /// <summary>
    /// Inicializa el picker con las opciones como string a|Option 1,b|Option2,c|Option 3
    /// </summary>
    /// <param name="value">The string values</param>
    public void InitControl(string value)
    {
        if (string.IsNullOrEmpty(value)) return;
        Options = new();
        var splited = value.Split(",");
        foreach (var item in splited)
        {
            var itemSplited = item.Split("|");
            Options.Add(new() { Key = itemSplited[0].TrimStart().TrimEnd(), Value = itemSplited[1] });
        }
        PickerSelect.ItemsSource = Options;
    }

    /// <summary>
    /// Inicializa el picker con las opciones como Objeto de opciones
    /// </summary>
    /// <param name="value">The string values</param>
    public void InitControl(List<SelectOption> values)
    {
        PickerSelect.ItemsSource = Options = values;
    }

    public string GetKey()
    {
        return Key;
    }

    public object GetValue()
    {
        return (PickerSelect.SelectedItem as SelectOption)?.Key ?? string.Empty;
    }

    public bool HasValue()
    {
        return PickerSelect.SelectedIndex >= 0;
    }

    public bool IsRequired()
    {
        return IsRequiredField;
    }

    public void SetRequiredStatus()
    {
        MyBorder.SetDynamicResource(Border.StrokeProperty, "Red");
        RequiredLabel.IsVisible = true;
    }

    public void ClearStatusRequired()
    {
        if (!RequiredLabel.IsVisible) return;
        RequiredLabel.IsVisible = false;
        var dark = Color.FromArgb("#404040");
        var light = Color.FromArgb("#ACACAC");
        MyBorder.SetAppTheme(Border.StrokeProperty, light, dark);
    }

    public void SetValue(object value)
    {
        var index = Options.FindIndex(x => x.Key == value.ToString());

        PickerSelect.SelectedIndex = index;
    }

    void PickerSelect_SelectedIndexChanged(System.Object sender, System.EventArgs e)
    {
        if(sender is Picker picker)
        {
            if (picker.SelectedIndex > 0)
                ClearStatusRequired();
        }
    }
}
